using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

using BH.oM.Geometry;

namespace BH.oM.SQL
{
    public class DatabaseContextTest : DbContext, IDatabaseContext, IImmutable
    {
        public DatabaseContextTest(string connectionString) : base(connectionString)
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<TestPoint>().ToTable("Point");
        }
    }
}



