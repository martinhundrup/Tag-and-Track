using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if !UNIT_TESTING
using TagAndTrack.Backend.Data;
#endif
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
            if (LoanItems.Any(x => x.ID == item.ID)) // already in loan (compare by ID, not reference)
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

        public static int AddContainerItems(ContainerItem container)
        {
            int added = 0;
            foreach (var specimen in container.Specimens)
            {
                if (AddItem(specimen) == null)
                    added++;
            }
            return added;
        }

        public static void RemoveItem(SpecimenItem item) 
        {
            if (!LoanItems.Contains(item)) return;
            LoanItems.Remove(item);
        }

#if !UNIT_TESTING
        public static async Task<LoanItem> FinalizeLoanAsync(string loanName, string loanDescription, string borrower, string borrowerEmail, DateTime dueDate, byte[]? signatureBytes = null)
        {
            var loan = new LoanItem(loanName, loanDescription);

            // add and checkout specimens – persist each checkout to the database
            foreach (var item in LoanItems)
            {
                loan.AddSpecimen(item);
                item.Checkout();
                await DbService.UpdateSpecimenAsync((int)item.ID, false);
            }

            loan.Borrower = borrower;
            loan.Email = borrowerEmail;
            loan.DateCheckedOut = DateTime.Now;
            loan.DateDue = dueDate;
            loan.SetSignature(signatureBytes);

            // Persist loan + join rows to database
            await DbService.AddLoanAsync(loan);

            return loan;
        }
#endif
    }
}
