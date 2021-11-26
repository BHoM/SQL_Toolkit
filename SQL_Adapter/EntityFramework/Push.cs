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
using System.Linq;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Adapter;
using System.Data.SqlClient;
using BH.oM.Adapters.SQL;
using BH.Engine.Reflection;
using BH.Engine.SQL;
using System.Data;
using System.Reflection;

using System.Data.Entity;

namespace BH.Adapter.SQL
{
    public partial class SqlAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public List<object> PushEntityFramework(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            List<object> result = new List<object>();

            if (objects == null)
                return result;

            objects = objects.Where(x => x != null).ToList();
            if (objects.Count() == 0)
                return result;

            // Get the type of the pushed objects
            List<Type> objectTypes = objects.Select(x => x.GetType()).Distinct().ToList();
            Dictionary<Type, List<object>> objectsByTypes = new Dictionary<Type, List<object>>();
            foreach (Type t in objectTypes)
                objectsByTypes.Add(t, objects.Where(x => x.GetType() == t).ToList());

            var minfo = typeof(DbContext).GetMethods().Where(x => x.Name == "Set").FirstOrDefault(x => x.IsGenericMethod);
            List<DbSet> sets = new List<DbSet>();

            foreach (KeyValuePair<Type, List<object>> kvp in objectsByTypes)
            {
                MethodInfo method = minfo.MakeGenericMethod(kvp.Key);
                var s = method.Invoke(m_DatabaseContext, null);

                Type setType = typeof(DbSet<>).MakeGenericType(new[] { kvp.Key });

                var set = System.Convert.ChangeType(s, setType); // (DbSet)Activator.CreateInstance(setType);

                MethodInfo addMethod = setType.GetMethods().Where(x => x.Name == "Add").FirstOrDefault();

                foreach (object o in kvp.Value)
                {
                    addMethod.Invoke(set, new[] { o });
                    result.Add(o);
                }
            }

            m_DatabaseContext.SaveChanges();

            return result;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/
    }
}


