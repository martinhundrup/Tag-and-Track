namespace TagAndTrack.Backend.Items
{
    public class LoanItem : Item
    {
        public LoanItem()
        {
            Type = ItemType.Loan;
        }

        public LoanItem(string name, string description) : this()
        {
            Name = name;
            Description = description;
        }

        // loan metadata
        public string? Borrower { get; set; }
        public string? Email { get; set; }
        public DateTime DateCheckedOut { get; set; }
        public DateTime DateDue { get; set; }

        // linked specimens
        public IReadOnlyList<SpecimenItem> Specimens => specimens;
        private readonly List<SpecimenItem> specimens = new();

        // internal setters used by loader
        internal void SetBorrower(string? borrower) => Borrower = borrower;
        internal void SetEmail(string? email) => Email = email;
        internal void SetDates(DateTime outDate, DateTime dueDate)
        {
            DateCheckedOut = outDate;
            DateDue = dueDate;
        }
        internal void AddSpecimen(SpecimenItem s) => specimens.Add(s);
    }
}
