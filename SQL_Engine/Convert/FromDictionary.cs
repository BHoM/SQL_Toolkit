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

        public static object FromDictionary(Dictionary<string, object> dic, Type type = null)
        {
            if (type == null && dic.ContainsKey("_t"))
                type = BH.Engine.Reflection.Create.Type(dic["_t"] as string);

            if (type == null)
                type = typeof(CustomObject);

            object instance = Activator.CreateInstance(type);
            foreach (var kvp in dic)
            {
                PropertyInfo prop = type.GetProperty(kvp.Key);
                if (prop != null)
                    prop.SetValue(instance, kvp.Value);
                else if (instance is BHoMObject)
                    ((BHoMObject)instance).CustomData[kvp.Key] = kvp.Value;
            }

            return instance;
        }

        /***************************************************/
    }
}
