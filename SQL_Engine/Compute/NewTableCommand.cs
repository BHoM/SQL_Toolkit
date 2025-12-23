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

using BH.Engine.Base;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.SQL
{
    public static partial class Compute 
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static string NewTableCommand(this Type objectType, string tableName = "")
        {
            if (objectType == null)
                return "";

            // Set the table name to the object type name if not provided by the user
            if (string.IsNullOrWhiteSpace(tableName))
                tableName = objectType.Name;

            // Collect the valid properties
            Dictionary<string, Type> properties = objectType.GetProperties()
                .Where(x => x.CanRead && x.GetMethod.GetParameters().Count() == 0)
                .ToDictionary(x => x.Name, x => x.PropertyType);

            // Collect the valid columns for the SQL table
            Dictionary<string, Type> columns = new Dictionary<string, Type>();
            foreach (var prop in properties)
            {
                Type propertyType = prop.Value;
                if (propertyType.IsPrimitive || propertyType.IsEnum || propertyType == typeof(string) || propertyType == typeof(Guid) || propertyType == typeof(DateTime))
                    columns.Add(prop.Key, propertyType);
                else
                    Engine.Base.Compute.RecordWarning($"Property {prop.Key} was not added to the table as it is not a primitive type, an enum, a string, a date, or a Guid.");
            }

            // Create the SQL command to generate the new table
            return $"CREATE TABLE {tableName} ("
                    + "\n\t[id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,"
                    + columns.Select(x => $"\n\t[{x.Key}] {x.Value.ToSqlTypeString()}").Aggregate((a, b) => a + "," + b)
                    + "\n)";
        }

        /***************************************************/
    }
}





