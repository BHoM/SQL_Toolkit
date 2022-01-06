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

using BH.Engine.Reflection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Adapter.SQL
{
    public partial class SqlAdapter : BHoMAdapter
    {
        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        public SqlAdapter(string server, string database)
        {
            m_ConnectionString = $"Server = {server}; Database = {database}; Trusted_Connection = True;";
            Initialise();
        }

        /***************************************************/

        public SqlAdapter(string connectionString)
        {
            m_ConnectionString = connectionString;
            Initialise();
        }


        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public List<string> GetMatchingTables(Type type)
        {
            return m_TableTypes.Where(x => x.Value.Contains(type)).Select(x => x.Key).ToList();
        }

        /***************************************************/

        public string GetMatchingTable(Type type)
        {
            List<string> tables = GetMatchingTables(type);
            if (tables.Count == 1)
                return tables[0];
            else if (tables.Count == 0)
            {
                string message = "There is no registered table for the type " + type.IToText();
                Engine.Reflection.Compute.RecordError(message);
                return null;
            }
            else
            {
                string message = $"There are multiple tables registed for the type {type.IToText()} so the operation was aborded."
                    + "\nThe existing tables for that type are "
                    + tables.Aggregate((a, b) => a + ", " + b);
                Engine.Reflection.Compute.RecordError(message);
                return null;
            }
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private void Initialise()
        {
            using (SqlConnection connection = new SqlConnection(m_ConnectionString))
            {
                connection.Open();

                // Grab the defined types from the _tableTypes table if it exists in the database
                GrabTableTypes(connection);

                connection.Close();
            }
        }

        /***************************************************/

        private void GrabTableTypes(SqlConnection connection)
        {
            // Make sure the _tableTypes table exists
            if (!CheckTableExists(connection, "_tableTypes"))
                return;

            // Make sure the _tableTypes table contains the required columns
            List<string> columns = GetTableColumns(connection, "_tableTypes");
            if (!columns.Contains("TableName") || !columns.Contains("TypeName"))
                return;

            // Grab the list of table that have an explicit type associated to them
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT * FROM _tableTypes";
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string tableName = reader["TableName"] as string;
                    string typeName = reader["TypeName"] as string;
                    if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(typeName))
                        continue;

                    Type type = Engine.Reflection.Create.Type(typeName, true);
                    if (type != null)
                    {
                        if (!m_TableTypes.ContainsKey(tableName))
                            m_TableTypes[tableName] = new List<Type>();
                        m_TableTypes[tableName].Add(type);
                    }
                        
                }
                reader.Close();
            }
        }

        /***************************************************/

        private bool CheckTableExists(SqlConnection connection, string table)
        {
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '_tableTypes'";
                int nbMatch = (int)command.ExecuteScalar();
                return nbMatch == 1;
            }
        }

        /***************************************************/

        private List<string> GetTableColumns(SqlConnection connection, string table)
        {
            // Get the columns names if the table exists
            List<string> columns = new List<string>();
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{table}'";
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                    columns.Add(reader["COLUMN_NAME"].ToString());
                reader.Close();
            }

            return columns;
        }


        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private string m_ConnectionString = "";

        private Dictionary<string, List<Type>> m_TableTypes = new Dictionary<string, List<Type>>();

        /***************************************************/
    }
}

