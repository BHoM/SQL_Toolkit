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

        public static string IToCommand(this IRequest request)
        {
            return ToCommand(request as dynamic);
        }


        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static string ToCommand(this TableRequest request)
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

        public static string ToCommand(this oM.Adapters.SQL.CustomRequest request)
        {
            return request.Query;
        }


        /***************************************************/
        /**** Fallback Methods                          ****/
        /***************************************************/

        private static string ToCommand(this object request)
        {
            return "";
        }

        /***************************************************/
    }
}
