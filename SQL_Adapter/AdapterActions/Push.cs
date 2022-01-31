/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.Engine.SQL;
using System.Data;
using System.Reflection;

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
                Engine.Base.Compute.RecordError(message);
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
                    if (m_TableTypes[table].Contains(objectType))
                        return table;
                    else
                    {
                        string message = $"Table {table} expects objects of type {m_TableTypes[table].Select(x => x.ToString()).Aggregate((a,b) => a + " or " + b)}."
                            + "\nThis doesn't match the type of the objects to push ({objectType.ToString()}).";
                        Engine.Base.Compute.RecordError(message);
                        return null;
                    }
                }
                else
                    return table;
            }
            else
                return GetMatchingTable(objectType);
        }

        /***************************************************/

        private List<object> InsertObjects(SqlConnection connection, string table, IEnumerable<object> data)
        {
            // Get the schema for the table
            DataTable dataTable = GetSchema(connection, table);

            // Add the data to push as rows in the table
            List<object> newRows = GetRowsToPush(data, dataTable);

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
                    newRows.Clear();
                    BH.Engine.Base.Compute.RecordError("Failed to push the data into the database. Error:\n" + e.Message);
                }
            }

            return newRows;
        }

        /***************************************************/

        private DataTable GetSchema(SqlConnection connection, string table)
        {
            DataTable dataTable = new DataTable();
            using (SqlCommand schemaCommand = connection.CreateCommand())
            {
                schemaCommand.CommandText = $"SELECT TOP 0 * FROM {table}";
                SqlDataAdapter da = new SqlDataAdapter(schemaCommand);
                da.Fill(dataTable);
                da.Dispose();
            }

            return dataTable;
        }

        /***************************************************/

        private List<object> GetRowsToPush(IEnumerable<object> data, DataTable dataTable)
        {
            if (data.Count() == 0)
                return new List<object>();

            // Collect the list of properties that need to be added to the table
            List<string> columns = new List<string>();
            for (int i = 0; i < dataTable.Columns.Count; i++)
                columns.Add(dataTable.Columns[i].ColumnName);

            // Collect the properties of the objects
            Dictionary<string, PropertyInfo> properties = data.First().GetType()
                .GetProperties().Where(x => x.CanRead)
                .ToDictionary(x => x.Name);

            // Collect the rows to push
            List<object> rows = new List<object>();
            foreach (object item in data)
            {
                Dictionary<string, object> customData = new Dictionary<string, object>();
                if (item is BHoMObject)
                    customData = ((BHoMObject)item).CustomData;

                DataRow row = dataTable.NewRow();
                foreach (string column in columns)
                {
                    try
                    {
                        if (properties.ContainsKey(column))
                            row[column] = properties[column].GetValue(item);
                        else if (customData.ContainsKey(column))
                            row[column] = customData[column];
                    }
                    catch { }
                }
                dataTable.Rows.Add(row);
                rows.Add(item);
            }

            return rows;
        }

        /***************************************************/
    }
}



