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
                    return "bit(1)";
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
                    return "real";
                case "String":
                    return "varchar(510)";
                case "Guid":
                    return SqlDbType.UniqueIdentifier.ToString();
                case "DateTime":
                    return SqlDbType.DateTime.ToString();
                default:
                    if (type.IsEnum)
                        return "varchar(255)";
                    else
                        return SqlDbType.Variant.ToString();
            }

        }

        /***************************************************/
    }
}
