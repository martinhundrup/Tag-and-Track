using SQLite;

namespace TagAndTrack.Backend.Data.Entities
{
    [Table("Employees")]
    public class EmployeeEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime? LastLogin { get; set; }
    }
}
