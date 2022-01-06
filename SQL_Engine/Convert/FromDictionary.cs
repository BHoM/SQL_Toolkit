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
using BH.oM.Base;
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
        /**** Public Methods                            ****/
        /***************************************************/

        public static object FromDictionary(this Dictionary<string, object> dic, Type type = null)
        {
            if ((type == null || type.IsAbstract) && dic.ContainsKey("_t"))
                type = BH.Engine.Reflection.Create.Type(dic["_t"] as string);

            if (type == null)
                type = typeof(CustomObject);

            object instance = Activator.CreateInstance(type);
            foreach (var kvp in dic)
            {
                try
                {
                    if (instance is BHoMObject || !kvp.Key.StartsWith("_"))
                        instance.SetPropertyValue(kvp.Key, kvp.Value);
                }
                catch { }
            }
                
            return instance;
        }

        /***************************************************/
    }
}

