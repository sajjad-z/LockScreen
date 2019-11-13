using System.Data.Entity;

namespace Models
{
    /// <summary>
    /// Created By Sz => sz
    /// </summary>

    public class InitialDb : SQLite.CodeFirst.SqliteCreateDatabaseIfNotExists<myContext>
    {
        public InitialDb(DbModelBuilder modelBuilder) : base(modelBuilder)
        {
        }

        // for default value
        protected override void Seed(myContext context)
        {
            var setting = new tbl_Setting()
            {
                userName = "admin",
                passWord = "123",
                isStartUp = false,
                title = "سلام خوش آمدی ..."
            };
            context.tbl_Settings.Add(setting);
            base.Seed(context);
        }

    }
}
