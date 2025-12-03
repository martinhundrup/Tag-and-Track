using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Databases
{
    public class Loan
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [SQLite.MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [SQLite.MaxLength(100)]
        public string Description { get; set; } = string.Empty;

        [SQLite.MaxLength(100)]
        public string Borrower { get; set; } = string.Empty;

        [SQLite.MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        public DateTime DateCheckedOut { get; set; }

        public DateTime DateDue { get; set; }

        public List<int> specimenIDs { get; set; } = new List<int>();

        public string? QRID
        {
            get { return "Specimen:" + ID; }
        }
    }
}
