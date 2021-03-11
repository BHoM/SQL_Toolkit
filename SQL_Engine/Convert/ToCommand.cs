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
            return $"SELECT {request.Filter} FROM {request.Table}";
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
