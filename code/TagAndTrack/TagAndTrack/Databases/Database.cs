using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Databases
{
    public class Database
    {
        private static SQLiteAsyncConnection tableConnection;
        private static Database instance;

        private Database() { }

        public static Database Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Database();
                }
                return instance;
            }
        }

        public async Task InitAsync()
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TagAndTrack_Database.db3");

            tableConnection = new SQLiteAsyncConnection(filePath);
            await tableConnection.CreateTableAsync<Specimen>();
            await tableConnection.CreateTableAsync<Loan>();
            await tableConnection.CreateTableAsync<Container>();
        }

        public Task<int> AddSpecimen(string arc_ID, string name, string desc)
        {
            var data = new Specimen { ARC_ID = arc_ID, Name = name, Description = desc};
            return tableConnection.InsertAsync(data);
        }

        public Task<List<Specimen>> GetListOfSpecimen()
        {
            return tableConnection.Table<Specimen>().ToListAsync();
        }

        public Task<Specimen> FindSpecimenByID(int id)
        {
            return tableConnection.Table<Specimen>().FirstOrDefaultAsync(data => data.ID == id);
        }

        public Task<Specimen> FindSpecimenByName(string name)
        {
            return tableConnection.Table<Specimen>().FirstOrDefaultAsync(data => data.Name == name);
        }

        public Task<int> RemoveSpecimen(int id)
        {
            return tableConnection.DeleteAsync<Specimen>(id);
        }

        public Task<int> AddLoan(string name, string desc, string borrower, string email, DateTime dateCheckedOut, List<int> specimentIDs)
        {
            var data = new Loan { Name = name, Description = desc, Borrower = borrower, Email = email, DateCheckedOut = dateCheckedOut, specimenIDs = specimentIDs };
            return tableConnection.InsertAsync(data);
        }

        public Task<List<Loan>> GetListOfLoans()
        {
            return tableConnection.Table<Loan>().ToListAsync();
        }

        public Task<Loan> FindLoanByID(int id)
        {
            return tableConnection.Table<Loan>().FirstOrDefaultAsync(data => data.ID == id);
        }

        public Task<Loan> FindLoanByName(string name)
        {
            return tableConnection.Table<Loan>().FirstOrDefaultAsync(data => data.Name == name);
        }

        public Task<int> RemoveLoan(int id)
        {
            return tableConnection.DeleteAsync<Loan>(id);
        }

        //container functions will go here once we know how Arctos stores container items

    }
}
