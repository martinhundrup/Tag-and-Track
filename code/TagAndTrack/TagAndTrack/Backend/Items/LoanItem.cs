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

        // borrower signature (JSON-serialized stroke data)
        public byte[]? SignatureImageBytes { get; private set; }

        // linked specimens
        public IReadOnlyList<SpecimenItem> Specimens => specimens;
        private readonly List<SpecimenItem> specimens = new();

        // per-specimen return dates: specimen ID → return date (null = still on loan)
        private readonly Dictionary<ulong, DateTime?> specimenReturnDates = new();
        public IReadOnlyDictionary<ulong, DateTime?> SpecimenReturnDates => specimenReturnDates;

        // internal setters used by loader
        internal void SetBorrower(string? borrower) => Borrower = borrower;
        internal void SetEmail(string? email) => Email = email;
        internal void SetDates(DateTime outDate, DateTime dueDate)
        {
            DateCheckedOut = outDate;
            DateDue = dueDate;
        }
        internal void AddSpecimen(SpecimenItem s)
        {
            specimens.Add(s);
            if (!specimenReturnDates.ContainsKey(s.ID))
                specimenReturnDates[s.ID] = null;
        }
        internal void SetSignature(byte[]? data) => SignatureImageBytes = data;
        internal void SetSpecimenReturnDate(ulong specimenId, DateTime? date) => specimenReturnDates[specimenId] = date;

        /// <summary>
        /// Gets the return date for a specimen in this loan, or null if not yet returned.
        /// </summary>
        public DateTime? GetSpecimenReturnDate(ulong specimenId)
        {
            return specimenReturnDates.TryGetValue(specimenId, out var date) ? date : null;
        }

        /// <summary>
        /// Checks in a single specimen for this loan. If all specimens are now returned,
        /// the loan itself is marked as returned.
        /// </summary>
        public void CheckinSpecimen(ulong specimenId, DateTime returnDate)
        {
            if (!specimenReturnDates.ContainsKey(specimenId)) return;
            if (specimenReturnDates[specimenId] != null) return; // already returned for this loan

            specimenReturnDates[specimenId] = returnDate;

            // Check if all specimens are now returned
            if (specimenReturnDates.Values.All(d => d != null))
            {
                base.Checkin();
            }
        }

        /// <summary>
        /// Checks in all specimens that haven't been returned yet (for this loan).
        /// Each gets the same return timestamp. Skips specimens already returned.
        /// </summary>
        public override void Checkin()
        {
            var now = DateTime.Now;
            foreach (var specimenId in specimenReturnDates.Keys.ToList())
            {
                if (specimenReturnDates[specimenId] == null)
                    specimenReturnDates[specimenId] = now;
            }

            base.Checkin();
        }

        /// <summary>
        /// Overload that accepts a specific timestamp for all remaining check-ins.
        /// </summary>
        public void Checkin(DateTime returnDate)
        {
            foreach (var specimenId in specimenReturnDates.Keys.ToList())
            {
                if (specimenReturnDates[specimenId] == null)
                    specimenReturnDates[specimenId] = returnDate;
            }

            base.Checkin();
        }
    }
}
