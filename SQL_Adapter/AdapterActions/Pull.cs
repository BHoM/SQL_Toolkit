/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Data.Requests;
using BH.oM.Adapter;
using System.Data.SqlClient;
using BH.oM.Adapters.SQL;
using BH.Engine.SQL;

namespace BH.Adapter.SQL
{
    public partial class SqlAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public override IEnumerable<object> Pull(IRequest query, PullType pullType = PullType.AdapterDefault, ActionConfig actionConfig = null)
        {
            List<object> result = new List<object>();

            if (query == null)
                return result;

            using (SqlConnection connection = new SqlConnection(m_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = query.IToSqlCommand();
                    Type objectType = GetDataType(query);

                    SqlDataReader reader = command.ExecuteReader();
                    while(reader.Read())
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            dic.Add(reader.GetName(i), reader.GetValue(i));
                        result.Add(Engine.SQL.Convert.FromDictionary(dic, objectType));
                    }
                    reader.Close();
                }

                connection.Close();
            }

            return result;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private Type GetDataType(IRequest request)
        {
            if (request == null)
                return null;
            
            if (request is ITypeStrongRequest)
            {
                Type type = ((ITypeStrongRequest)request).DataType;
                if (type != null)
                    return type;
            }
            
            if (request is ISingleTableRequest)
            {
                string table = ((ISingleTableRequest)request).Table;
                if (m_TableTypes.ContainsKey(table) && m_TableTypes[table].Count == 1)
                    return m_TableTypes[table].First();
            }

            return null;
        }


        /***************************************************/
    }
}




