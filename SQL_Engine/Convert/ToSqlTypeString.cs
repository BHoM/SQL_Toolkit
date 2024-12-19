/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.SQL;
using BH.oM.Base;
using BH.oM.Data.Requests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.SQL
{
    public static partial class Convert 
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static string ToSqlTypeString(this Type type)
        {
            // TODO: This is just prototype code for when we enable creation of tables from the adapter
            switch (type.Name)
            {
                case "Boolean":
                    return "bit";
                case "Int16":
                case "UInt16":
                case "Int32":
                case "UInt32":
                case "Int64":
                case "UInt64":
                    return "bigint";
                case "Char":
                    return "char";
                case "Single":
                case "Double":
                    return "float";
                case "String":
                    return "varchar(255)";
                case "Guid":
                    return SqlDbType.UniqueIdentifier.ToString();
                case "DateTime":
                    return "datetime";
                default:
                    if (type.IsEnum)
                        return "varchar(100)";
                    else
                        return SqlDbType.Variant.ToString();
            }

        }

        /***************************************************/
    }
}




