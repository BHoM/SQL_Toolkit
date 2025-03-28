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

using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BH.Engine.Base;

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
                type = BH.Engine.Base.Create.Type(dic["_t"] as string);

            if (type == null)
                type = typeof(CustomObject);

            object instance = Activator.CreateInstance(type);
            foreach (var kvp in dic)
            {
                try
                {
                    if (kvp.Value != null && !(kvp.Value is DBNull))
                    {
                        if (kvp.Key.StartsWith("_") && instance is BHoMObject)
                            ((BHoMObject)instance).CustomData[kvp.Key] = kvp.Value;
                        else
                            instance.SetPropertyValue(kvp.Key, kvp.Value);
                    }
                    
                }
                catch { }
            }
                
            return instance;
        }

        /***************************************************/
    }
}




