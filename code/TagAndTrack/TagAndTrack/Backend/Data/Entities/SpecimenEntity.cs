using SQLite;

namespace TagAndTrack.Backend.Data.Entities
{
    [Table("Specimens")]
    public class SpecimenEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? ArctosId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsPresent { get; set; } = true;
    }
}
