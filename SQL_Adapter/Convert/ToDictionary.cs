using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace BH.Adapter.SQL
{
    public static partial class Convert
    {
        public static Dictionary<string, object> ToDictionary(object o)
        {
            List<PropertyInfo> objectProperties = new List<PropertyInfo>(o.GetType().GetProperties());
            Dictionary<string, object> propertyVals = new Dictionary<string, object>();

            foreach(var pi in objectProperties)
            {
                object val = pi.GetValue(o);
                propertyVals.Add(pi.Name, val);
            }

            return propertyVals;
        }
    }
}
