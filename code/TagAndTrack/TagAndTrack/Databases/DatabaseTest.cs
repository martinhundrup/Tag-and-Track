using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace TagAndTrack.Databases
{
    public class DatabaseTest
    {
        private readonly SQLiteConnection testDatabase;

        public DatabaseTest()
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "testData.db3");

            testDatabase = new SQLiteConnection(filePath);
            testDatabase.CreateTable<DataTest>();
        }

        public void AddData(string arc_ID, string name, string desc, bool present)
        {
            var data = new DataTest { ARC_ID = arc_ID, Name = name, Description = desc, Present = present };
            testDatabase.Insert(data);
        }

        public List<DataTest> GetListOfData()
        {
            return testDatabase.Table<DataTest>().ToList();
        }

        public DataTest? FindDataByID(int id)
        {
            return testDatabase.Table<DataTest>().FirstOrDefault(data => data.ID == id);
        }

        public DataTest? FindDataByName(string name)
        {
            return testDatabase.Table<DataTest>().FirstOrDefault(data => data.Name == name);
        }

        public void RemoveData(int id)
        {
            testDatabase.Delete(id);
        }
    }
}
