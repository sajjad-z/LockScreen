using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Models
{
    public class myContext : DbContext
    {
        public myContext() : base("constr")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            var initializer = new InitialDb(modelBuilder);
            Database.SetInitializer(initializer);
        }

        public DbSet<tbl_Setting> tbl_Settings { get; set; }
    }
}
