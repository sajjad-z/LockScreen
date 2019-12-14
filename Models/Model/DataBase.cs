using LiteDB;
using System;

namespace Models
{
    public class DataBase<TEntity> where TEntity : class
    {
        LiteDatabase db;
        LiteCollection<TEntity> collection;

        public DataBase()
        {
            // Open database (or create if not exits)
            db = new LiteDatabase(System.AppDomain.CurrentDomain.BaseDirectory + @"MyDB.db");

            // Get table collection
            if (typeof(TEntity) == typeof(tbl_Setting))
            {
                collection = db.GetCollection<TEntity>("tbl_settings");
                if (collection.FindById(1) == null)
                {
                    tbl_Setting setting = new tbl_Setting()
                    {
                        id = 1,
                        isStartUp = true,
                        passWord = "123",
                        title = "سلام خوش آمدی ...",
                        userName = "admin"
                    };
                    collection.Insert(setting as TEntity);
                }
            }
        }

        public TEntity Select(int id)
        {
            try
            {
                return collection.FindById(id);
            }
            catch { return null; }
        }

        public bool Update(TEntity record)
        {
            try
            {
                collection.Update(record);
                return true;
            }
            catch { return false; }
        }

    }
}
