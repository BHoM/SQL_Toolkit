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


