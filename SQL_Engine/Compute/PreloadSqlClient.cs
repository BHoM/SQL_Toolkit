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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        [Description("Make sure the correct version of Microsoft.Data.SqlClient before Rhino 8 has a chance to load its stub dll. Temporary solution while wating for a fix from McNeel.")]
        [Output("success", "return true if the code was executed without error.")]
        public static bool PreloadSqlClient()
        {
            // Only relevant on .NET 5+ (CoreCLR / Rhino 8).
            // On .NET Framework 4.7.2 (Rhino 7) the System.Data.SqlClient inbox provider is used
            // and Microsoft.Data.SqlClient is not involved.
            if (Environment.Version.Major < 5)
                return true;

            // Assembly path for the version of the SqlCleint we want to load
            string assemblyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "BHoM", "Assemblies", "net7.0", "Microsoft.Data.SqlClient.dll");

            // Guard: if SqlClient is already in the Default ALC we are too late.
            // Check whether what is loaded is the real implementation or Rhino's stub.
            Assembly already = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Microsoft.Data.SqlClient");

            if (already != null)
            {
                if (!already.Location.Contains("BHoM"))
                {
                    BH.Engine.Base.Compute.RecordWarning(
                        "Microsoft.Data.SqlClient was already loaded from Rhino's directory before the SQL Toolkit " +
                        $"pre-load could run: {already.Location}. The SQL Adapter will not work correctly in Rhino 8. " +
                        "This indicates that a Rhino plugin loaded the stub at Rhino startup. " +
                        "Consider switching to the ALC-isolation approach described in 14_OptionA_ALC_Implementation.md.");
                    return false;
                }

                // Already loaded from BHoM's own directory — nothing to do.
                return true;
            }

            if (!File.Exists(assemblyPath))
            {
                BH.Engine.Base.Compute.RecordWarning(
                    $"Microsoft.Data.SqlClient.dll was not found at the expected path: {assemblyPath}. " +
                    "The SQL Adapter may not work correctly in Rhino 8. " +
                    "Ensure SQL_Adapter has been built with the net7.0 target framework.");
                return false;
            }

            try
            {
                Assembly.LoadFrom(assemblyPath);
                return true;
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordError(ex, "Failed to pre-load Microsoft.Data.SqlClient from the BHoM Assemblies folder.");
                return false;
            }
        }

        /***************************************************/
    }
}





