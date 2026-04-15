using SQLite;

namespace TagAndTrack.Backend.Data.Entities
{
    [Table("LoanSpecimens")]
    public class LoanSpecimenEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int LoanId { get; set; }
        public int SpecimenId { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}
