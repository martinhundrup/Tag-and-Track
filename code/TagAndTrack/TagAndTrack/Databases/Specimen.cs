using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Databases
{
    public class Specimen
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [SQLite.MaxLength(100)]
        public string ARC_ID { get; set; } = string.Empty;

        [SQLite.MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [SQLite.MaxLength(100)]
        public string Description { get; set; } = string.Empty;

        public bool Status { get; set; } = true;

        public string? QRID {
            get { return "Specimen:" + ID; }
        }
    }
}