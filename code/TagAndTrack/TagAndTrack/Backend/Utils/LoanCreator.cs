using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagAndTrack.Backend.Items;

namespace TagAndTrack.Backend.Utils
{
    internal static class LoanCreator
    {
        public static List<SpecimenItem> LoanItems 
        {
            get;
            private set;
        } = new List<SpecimenItem>();

        public static void ClearLoan()
        {
            LoanItems.Clear();
        }

        public static string? AddItem(SpecimenItem item) // returns an error message if something went awry
        {
            if (LoanItems.Contains(item)) // already in loan
            {
                return $"Item {item.Name} is already present in the loan!";
            }

            if (!item.Status) // checked out
            {
                return $"Item {item.Name} is already checked out!";
            }

            LoanItems.Add(item);
            return null;
        }

        public static void RemoveItem(SpecimenItem item) 
        {
            if (!LoanItems.Contains(item)) return;
            LoanItems.Remove(item);
        }

        public static LoanItem FinalizeLoan(string loanName, string loanDescription, string borrower, string borrowerEmail, DateTime dueDate, byte[]? signatureBytes = null)
        {
            var loan = new LoanItem(loanName, loanDescription);

            // add and checkout loans
            foreach (var item in LoanItems)
            {
                loan.AddSpecimen(item);
                item.Checkout();
            }

            loan.Borrower = borrower;
            loan.Email = borrowerEmail;
            loan.DateCheckedOut = DateTime.Now;
            loan.DateDue = dueDate;
            loan.SetSignature(signatureBytes);

            ItemManager.AddItem(loan);

            return loan;
        }
    }
}
