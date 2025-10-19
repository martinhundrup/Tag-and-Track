using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Items
{
    internal class LoanItem : Item
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
