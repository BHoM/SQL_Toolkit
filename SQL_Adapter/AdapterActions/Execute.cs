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
using System.Data.SqlClient;
using BH.Engine.SQL;
using BH.oM.Adapter;
using BH.oM.Adapters.SQL;
using BH.oM.Reflection;

using System.Linq;
using System.Reflection;

using System.Data;

namespace BH.Adapter.SQL
{
    public partial class SqlAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public override Output<List<object>, bool> Execute(IExecuteCommand command, ActionConfig actionConfig = null)
        {
            return ExecuteCommand(command as dynamic, actionConfig);
        }


        /***************************************************/
        /****  Execute Methods                          ****/
        /***************************************************/

        public Output<List<object>, bool> ExecuteCommand(UpsertCommand upsert, ActionConfig actionConfig = null)
        {
            Output<List<object>, bool> result = new Output<List<object>, bool> { Item1 = new List<object>(), Item2 = false };

            if (upsert == null)
                return result;

            List<Type> objectTypes = upsert.ObjectsToUpsert.Select(x => x.GetType()).Distinct().ToList();
            if (objectTypes.Count != 1)
            {
                string message = "The SQL adapter only allows to upsert objects of a single type to a table."
                    + "\nRight now you are providing objects of the following types: "
                    + objectTypes.Select(x => x.ToString()).Aggregate((a, b) => a + ", " + b);
                Engine.Reflection.Compute.RecordError(message);
                return result;
            }

            List<object> updateObjects = new List<object>();
            List<object> insertObjects = new List<object>();

            Type objectType = objectTypes[0];
            List<PropertyInfo> objectProperties = new List<PropertyInfo>(objectType.GetProperties());
            PropertyInfo primaryKeyProperty = objectProperties.Where(x => x.Name == upsert.PrimaryKey).FirstOrDefault();
            if(primaryKeyProperty == null)
            {
                BH.Engine.Reflection.Compute.RecordError($"Primary key field {upsert.PrimaryKey} does not exist on objects of type {objectType.ToString()}");
                return result;
            }

            foreach(object o in upsert.ObjectsToUpsert)
            {
                var propValue = primaryKeyProperty.GetValue(o);
                if (propValue == upsert.DefaultPrimaryKeyValue)
                    insertObjects.Add(o);
                else
                    updateObjects.Add(o);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(m_ConnectionString))
                {
                    connection.Open();
                    result.Item1.AddRange(InsertObjects(connection, upsert.Table, insertObjects));
                    connection.Close();
                }
            }
            catch(Exception ex)
            {
                BH.Engine.Reflection.Compute.RecordError($"Error in inserting {insertObjects.Count} items. Error was {ex.ToString()}");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(m_ConnectionString))
                {
                    connection.Open();

                    foreach(object o in updateObjects)
                    {
                        Dictionary<string, object> changes = Convert.ToDictionary(o);
                        if (changes == null)
                            continue;

                        string where = upsert.PrimaryKey + "=" + changes[upsert.PrimaryKey].ToString();

                        changes.Remove(upsert.PrimaryKey); //Don't update the primary key

                        string changesToSet = changes.Select(x =>
                        {
                            string val = x.Value.ToString();
                            if (!x.Value.GetType().IsPrimitive)
                                val = $"'{val}'";
                            else if (x.Value is bool)
                                val = System.Convert.ToInt32(x.Value).ToString();

                            return $"{x.Key} = {val}";
                        }).Aggregate((a, b) => a + ", " + b);

                        string commandString = $"UPDATE {upsert.Table} SET {changesToSet} WHERE {where}";

                        try
                        {
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.CommandText = commandString;
                                command.ExecuteNonQuery();
                            }
                        }
                        catch(Exception ex)
                        {
                            BH.Engine.Reflection.Compute.RecordError($"Error in updating item where {where}. Exception was {ex.ToString()}");
                        }
                    }

                    connection.Close();
                }
            }
            catch(Exception ex)
            {
                BH.Engine.Reflection.Compute.RecordError($"Error in updating items. Error was {ex.ToString()}");
                return result;
            }

            result.Item1.AddRange(updateObjects);
            result.Item2 = true;
            return result;
        }

        public Output<List<object>, bool> ExecuteCommand(UpdateCommand update, ActionConfig actionConfig = null)
        {
            Output<List<object>, bool> result = new Output<List<object>, bool> { Item1 = new List<object>(), Item2 = false };

            if (update == null)
                return result;

            if (string.IsNullOrWhiteSpace(update.Table))
                update.Table = GetMatchingTable(update.DataType);

            using (SqlConnection connection = new SqlConnection(m_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    try
                    {
                        command.CommandText = update.IToSqlCommand();
                        int nbSuccess = command.ExecuteNonQuery();

                        result.Item2 = nbSuccess > 0;
                    }
                    catch (Exception e)
                    {
                        Engine.Reflection.Compute.RecordError(e.Message);
                    }
                }

                connection.Close();
            }

            return result;
        }

        /***************************************************/

        public Output<List<object>, bool> ExecuteCommand(IExecuteCommand command, ActionConfig actionConfig = null)
        {
            Output<List<object>, bool> result = new Output<List<object>, bool> { Item1 = new List<object>(), Item2 = false };

            Engine.Reflection.Compute.RecordError("Execute is not implemented yet for the SQl adapter.");
            return result;
        }

        /***************************************************/
    }
}


