/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Adapter;
using System.Data.SqlClient;
using BH.oM.Adapters.SQL;
using BH.Engine.Reflection;
using BH.Engine.SQL;
using System.Data;

namespace BH.Adapter.SQL
{
    public partial class SqlAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            List<object> result = new List<object>();

            if (objects == null)
                return result;

            objects = objects.Where(x => x != null).ToList();
            if (objects.Count() == 0)
                return result;

            // Get the type of the pushed objects
            List<Type> objectTypes = objects.Select(x => x.GetType()).Distinct().ToList();
            if (objectTypes.Count != 1)
            {
                string message = "The SQL adapter only allows to push objects of a single type to a table."
                    + "\nRight now you are providing objects of the following types: "
                    + objectTypes.Select(x => x.ToString()).Aggregate((a, b) => a + ", " + b);
                Engine.Reflection.Compute.RecordError(message);
                return result;
            }
            Type type = objectTypes[0];

            // Get the name of table the objects are pushed to
            string table = GetTableName(type, actionConfig as PushConfig);
            if (string.IsNullOrWhiteSpace(table))
                return result;
            
            // Pushing the objects
            using (SqlConnection connection = new SqlConnection(m_ConnectionString))
            {
                connection.Open();
                result = InsertObjects(connection, table, objects);
                connection.Close();
            }

            return result;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private string GetTableName(Type objectType, PushConfig config)
        {
            // Get possible table from push config
            string table = null;
            if (config != null && !string.IsNullOrWhiteSpace(config.Table))
                table = config.Table;

            // If table is already registered, make sure that the type matches. Return error otherwise
            if (table != null)
            {
                if (m_TableTypes.ContainsKey(table))
                {
                    if (objectType == m_TableTypes[table])
                        return table;
                    else
                    {
                        string message = $"Table {table} expects objects of type {m_TableTypes[table].ToString()}."
                            + "\nThis doesn't match the type of the objects to push ({objectType.ToString()}).";
                        Engine.Reflection.Compute.RecordError(message);
                        return null;
                    }
                }
                else
                    return table;
            }

            // Get possible table from 
            List<string> tables = m_TableTypes.Where(x => x.Value == objectType).Select(x => x.Key).ToList();
            if (tables.Count == 1)
                return tables[0];
            else if (tables.Count == 0)
            {
                string message = "The table to push the data to couldn't be infered from the action config or the type of objects pushed.\nPlease provide a PushConfig with a valid table name.";
                Engine.Reflection.Compute.RecordError(message);
                return null;
            }
            else
            {
                string message = "The table name was not provided in the PushConfig. There are multiple tables registed for the type of objects you want to push so the operation was aborded."
                    + "\nPlease provide a PushConfig with a valid table name. The existing tables for that type are "
                    + tables.Aggregate((a, b) => a + ", " + b);
                Engine.Reflection.Compute.RecordError(message);
                return null;
            }
        }

        /***************************************************/

        private List<object> InsertObjects(SqlConnection connection, string table, IEnumerable<object> data)
        {
            // Get the schema for the table
            DataTable dataTable = new DataTable();
            using (SqlCommand schemaCommand = connection.CreateCommand())
            {
                schemaCommand.CommandText = $"SELECT TOP 0 * FROM {table}";
                SqlDataAdapter da = new SqlDataAdapter(schemaCommand);
                da.Fill(dataTable);
                da.Dispose();
            }

            // Collect the list of properties that need to be added to the table
            List<string> columns = new List<string>();
            for (int i = 0; i < dataTable.Columns.Count; i++)
                columns.Add(dataTable.Columns[i].ColumnName);

            // Add the data to push as rows in the table
            List<object> addedData = new List<object>();
            foreach (object item in data)
            {
                Dictionary<string, object> properties = item.PropertyDictionary();
                DataRow row = dataTable.NewRow();

                foreach (string column in columns)
                {
                    if (properties.ContainsKey(column))
                        row[column] = properties[column];
                }
                dataTable.Rows.Add(row);
                addedData.Add(item);
            }

            // Push the table in one bulk insert
            using (SqlBulkCopy bulk = new SqlBulkCopy(connection))
            {
                try
                {
                    bulk.DestinationTableName = table;
                    bulk.WriteToServer(dataTable);
                }
                catch (Exception e)
                {
                    addedData.Clear();
                    BH.Engine.Reflection.Compute.RecordError("Failed to push the data into the database. Error:\n" + e.Message);
                }
            }

            return addedData;
        }

        /***************************************************/

        private List<string> CreateTable(SqlConnection connection, string table, Type objectType)
        {
            if (objectType == null)
                return new List<string>();

            // Collect the valid properties
            Dictionary<string, Type> properties = objectType.GetProperties()
                .Where(x => x.CanRead && x.GetMethod.GetParameters().Count() == 0)
                .ToDictionary(x => x.Name, x => x.PropertyType);

            return CreateTable(connection, table, properties);
        }

        /***************************************************/

        private List<string> CreateTable(SqlConnection connection, string table, CustomObject sampleObject)
        {
            if (sampleObject == null)
                return new List<string>();

            Dictionary<string, Type> properties = sampleObject.CustomData
                .Where(x => x.Value != null)
                .ToDictionary(x => x.Key, x => x.Value.GetType());

            return CreateTable(connection, table, properties);
        }

        /***************************************************/

        private List<string> CreateTable(SqlConnection connection, string table, Dictionary<string, Type> properties)
        {
            // Collect the valid properties
            Dictionary<string, Type> columns = new Dictionary<string, Type>();
            foreach (var prop in properties)
            {
                Type propertyType = prop.Value;
                if (propertyType.IsPrimitive || propertyType.IsEnum || propertyType == typeof(string) || propertyType == typeof(Guid) || propertyType == typeof(DateTime))
                    columns.Add(prop.Key, propertyType);
                else
                    Engine.Reflection.Compute.RecordWarning($"Property {prop.Key} was not added to the table as it is not a primitive type, an enum, a string, a date, or a Guid.");
            }

            // Create the table in the database
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = $"CREATE TABLE {table} ("
                            + columns.Select(x => $"{x.Key} + {x.Value.ToSqlTypeString()}").Aggregate((a, b) => a + ", " + b)
                            + ")";
                command.ExecuteNonQuery();
            }

            return columns.Keys.ToList();
        }

        /***************************************************/
    }
}


