namespace TagAndTrack.Backend.Items
{
    public class LoanItem : Item
    {
        public LoanItem()
        {
            Type = ItemType.Loan;
        }

        public LoanItem(string  name, string description) : this()
        { 
            Name = name;
            Description = description;
        }
    }
}
