using SQLite;

namespace TagAndTrack.Backend.Data.Entities
{
    [Table("Loans")]
    public class LoanEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? ArctosId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Borrower { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime DateCheckedOut { get; set; }
        public DateTime DateDue { get; set; }
        public bool IsReturned { get; set; } = false;
        public string SpecimenIds { get; set; } = ""; // Comma-separated IDs
        public byte[]? SignatureData { get; set; } // Handwritten signature stored as JSON stroke data
    }
}
