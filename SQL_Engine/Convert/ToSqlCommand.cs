/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

using BH.oM.Adapter;
using BH.oM.Adapters.SQL;
using BH.oM.Base;
using BH.oM.Data.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.SQL
{
    public static partial class Convert 
    {
        /***************************************************/
        /**** Interface Methods                         ****/
        /***************************************************/

        public static string IToSqlCommand(this IRequest request)
        {
            return ToSqlCommand(request as dynamic);
        }

        /***************************************************/

        public static string IToSqlCommand(this IExecuteCommand request)
        {
            return ToSqlCommand(request as dynamic);
        }


        /***************************************************/
        /**** Request Methods                           ****/
        /***************************************************/

        public static string ToSqlCommand(this TableRequest request)
        {
            string select = "*";
            if (request.Columns != null && request.Columns.Count > 0)
                select = request.Columns.Aggregate((a, b) => a + ", " + b);

            string where = "";
            if (!string.IsNullOrWhiteSpace(request.Filter))
                where = "WHERE " + request.Filter;

            return $"SELECT {select} FROM {request.Table} {where}";
        }

        /***************************************************/

        public static string ToSqlCommand(this oM.Adapters.SQL.CustomRequest request)
        {
            return request.Query;
        }


        /***************************************************/
        /**** Command Methods                           ****/
        /***************************************************/

        public static string ToSqlCommand(this UpdateCommand command)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(command.Filter))
                where = "WHERE " + command.Filter;

            string changes = command.Changes.Select(x =>
            {
                string val = x.Value.ToString();
                if (!x.Value.GetType().IsPrimitive)
                    val = $"'{val}'";
                else if (x.Value is bool)
                    val = System.Convert.ToInt32(x.Value).ToString();

                return $"{x.Key} = {val}";
            }).Aggregate((a, b) => a + ", " + b);

            return $"UPDATE {command.Table} SET {changes} {where}";
        }


        /***************************************************/
        /**** Fallback Methods                          ****/
        /***************************************************/

        private static string ToSqlCommand(this object request)
        {
            return "";
        }

        /***************************************************/
    }
}





