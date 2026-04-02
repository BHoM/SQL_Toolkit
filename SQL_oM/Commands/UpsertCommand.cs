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

using BH.oM.Adapter;
using BH.oM.Data.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.oM.Adapters.SQL
{
    [Description("Specify objects to update or insert (upsert) depending on whether existing data exists. If the data does not exist based on the Primary Key given, then the data is inserted into the table. If the data did previously exist, then its columns are updated.")]
    public class UpsertCommand : IExecuteCommand
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        [Description("Specify the primary key of the data used to identify existing data to update, or to insert if the data does not exist.")]
        public virtual string PrimaryKey { get; set; }

        [Description("Specify what the default value of your primary key is to determine whether data should be updated or inserted. E.G. if your primary key is 'ID' and is of type 'int', then specify 0 for objects which are new and need inserting.")]
        public virtual object DefaultPrimaryKeyValue { get; set; }

        [Description("Set the type of the primary key value - used to ensure queried properties from objects to upsert match the right type and the DefaultPrimaryKeyValue is correctly casted to this type.")]
        public virtual Type PrimaryKeyType { get; set; }

        [Description("List of objects which are to be upserted into the database. Objects must all be of a single type.")]
        public virtual List<object> ObjectsToUpsert { get; set; }

        [Description("The name of the table which the data should be upserted to.")]
        public virtual string Table { get; set; } = "";

        /***************************************************/
    }
}







