using TagAndTrack.Backend.Items;

namespace TagAndTrack.Tests;

public class LoanItemTests
{
    [Fact]
    public void Type_IsLoan()
    {
        var loan = new LoanItem();
        Assert.Equal(Item.ItemType.Loan, loan.Type);
    }

    [Fact]
    public void Constructor_SetsNameAndDescription()
    {
        var loan = new LoanItem("Loan 1", "Field trip loan");
        Assert.Equal("Loan 1", loan.Name);
        Assert.Equal("Field trip loan", loan.Description);
    }

    [Fact]
    public void Checkin_SetsOwnStatusToTrue()
    {
        var loan = new LoanItem();
        loan.Checkout();
        loan.Checkin();

        Assert.True(loan.Status);
    }

    [Fact]
    public void Checkin_SetsReturnDatesForAllSpecimens()
    {
        var loan = new LoanItem("Loan", "desc");
        var s1 = new SpecimenItem("A", "a", "desc a");
        var s2 = new SpecimenItem("B", "b", "desc b");
        loan.AddSpecimen(s1);
        loan.AddSpecimen(s2);

        loan.Checkout();

        loan.Checkin();

        Assert.True(loan.Status);
        Assert.NotNull(loan.GetSpecimenReturnDate(s1.ID));
        Assert.NotNull(loan.GetSpecimenReturnDate(s2.ID));
    }

    [Fact]
    public void CheckinSpecimen_SetsReturnDate()
    {
        var loan = new LoanItem("Loan", "desc");
        var s1 = new SpecimenItem("A", "a", "desc a");
        loan.AddSpecimen(s1);
        loan.Checkout();

        var returnDate = new DateTime(2026, 4, 14, 10, 30, 0);
        loan.CheckinSpecimen(s1.ID, returnDate);

        Assert.Equal(returnDate, loan.GetSpecimenReturnDate(s1.ID));
    }

    [Fact]
    public void CheckinSpecimen_WhenAllReturned_MarksLoanReturned()
    {
        var loan = new LoanItem("Loan", "desc");
        var s1 = new SpecimenItem("A", "a", "desc a");
        var s2 = new SpecimenItem("B", "b", "desc b");
        loan.AddSpecimen(s1);
        loan.AddSpecimen(s2);
        loan.Checkout();

        var now = DateTime.Now;
        loan.CheckinSpecimen(s1.ID, now);
        Assert.False(loan.Status); // not all returned yet

        loan.CheckinSpecimen(s2.ID, now);
        Assert.True(loan.Status); // all returned, loan auto-checked in
    }

    [Fact]
    public void CheckinSpecimen_WhenSomeReturned_LoanStillActive()
    {
        var loan = new LoanItem("Loan", "desc");
        var s1 = new SpecimenItem("A", "a", "desc a");
        var s2 = new SpecimenItem("B", "b", "desc b");
        var s3 = new SpecimenItem("C", "c", "desc c");
        loan.AddSpecimen(s1);
        loan.AddSpecimen(s2);
        loan.AddSpecimen(s3);
        loan.Checkout();

        loan.CheckinSpecimen(s1.ID, DateTime.Now);

        Assert.False(loan.Status);
        Assert.NotNull(loan.GetSpecimenReturnDate(s1.ID));
        Assert.Null(loan.GetSpecimenReturnDate(s2.ID));
        Assert.Null(loan.GetSpecimenReturnDate(s3.ID));
    }

    [Fact]
    public void CheckinLoan_SkipsAlreadyReturnedSpecimens()
    {
        var loan = new LoanItem("Loan", "desc");
        var s1 = new SpecimenItem("A", "a", "desc a");
        var s2 = new SpecimenItem("B", "b", "desc b");
        loan.AddSpecimen(s1);
        loan.AddSpecimen(s2);
        loan.Checkout();

        // Check in s1 first at an earlier time
        var earlyDate = new DateTime(2026, 4, 10);
        loan.CheckinSpecimen(s1.ID, earlyDate);

        // Now check in entire loan at a later time
        var lateDate = new DateTime(2026, 4, 14);
        loan.Checkin(lateDate);

        // s1 should keep its original date, s2 gets the later date
        Assert.Equal(earlyDate, loan.GetSpecimenReturnDate(s1.ID));
        Assert.Equal(lateDate, loan.GetSpecimenReturnDate(s2.ID));
        Assert.True(loan.Status);
    }

    [Fact]
    public void CheckinSpecimen_AlreadyReturned_DoesNotOverwrite()
    {
        var loan = new LoanItem("Loan", "desc");
        var s1 = new SpecimenItem("A", "a", "desc a");
        loan.AddSpecimen(s1);
        loan.Checkout();

        var firstDate = new DateTime(2026, 4, 10);
        var secondDate = new DateTime(2026, 4, 14);
        loan.CheckinSpecimen(s1.ID, firstDate);
        loan.CheckinSpecimen(s1.ID, secondDate); // should be ignored

        Assert.Equal(firstDate, loan.GetSpecimenReturnDate(s1.ID));
    }

    [Fact]
    public void Scenario_ReusedSpecimen_NotDoubleCheckedIn()
    {
        // 1. Create Loan1 with specimens A, B, C
        var loan1 = new LoanItem("Loan 1", "First loan");
        var specimenA = new SpecimenItem("ARC-A", "Item A", "desc A");
        var specimenB = new SpecimenItem("ARC-B", "Item B", "desc B");
        var specimenC = new SpecimenItem("ARC-C", "Item C", "desc C");
        loan1.AddSpecimen(specimenA);
        loan1.AddSpecimen(specimenB);
        loan1.AddSpecimen(specimenC);
        loan1.Checkout();

        // 2. Item A is returned — check it in on Loan1
        var returnDateA = new DateTime(2026, 4, 10, 14, 0, 0);
        loan1.CheckinSpecimen(specimenA.ID, returnDateA);

        Assert.Equal(returnDateA, loan1.GetSpecimenReturnDate(specimenA.ID));
        Assert.Null(loan1.GetSpecimenReturnDate(specimenB.ID));
        Assert.Null(loan1.GetSpecimenReturnDate(specimenC.ID));
        Assert.False(loan1.Status); // loan not fully returned

        // 3. Create Loan2 with specimens A, Y, Z (A re-loaned)
        var loan2 = new LoanItem("Loan 2", "Second loan");
        var specimenY = new SpecimenItem("ARC-Y", "Item Y", "desc Y");
        var specimenZ = new SpecimenItem("ARC-Z", "Item Z", "desc Z");
        loan2.AddSpecimen(specimenA);
        loan2.AddSpecimen(specimenY);
        loan2.AddSpecimen(specimenZ);
        loan2.Checkout();

        // 4. Items B and C are returned — user checks in Loan1 entirely
        var loanCheckinDate = new DateTime(2026, 4, 14, 9, 0, 0);
        loan1.Checkin(loanCheckinDate);

        // Assertions for Loan1:
        Assert.True(loan1.Status); // fully returned
        Assert.Equal(returnDateA, loan1.GetSpecimenReturnDate(specimenA.ID)); // A keeps ORIGINAL date
        Assert.Equal(loanCheckinDate, loan1.GetSpecimenReturnDate(specimenB.ID)); // B gets bulk date
        Assert.Equal(loanCheckinDate, loan1.GetSpecimenReturnDate(specimenC.ID)); // C gets bulk date

        // Assertions for Loan2: should be completely unaffected
        Assert.False(loan2.Status); // still active
        Assert.Null(loan2.GetSpecimenReturnDate(specimenA.ID)); // A not returned for Loan2
        Assert.Null(loan2.GetSpecimenReturnDate(specimenY.ID));
        Assert.Null(loan2.GetSpecimenReturnDate(specimenZ.ID));
    }

    [Fact]
    public void Specimens_IsReadOnlyView()
    {
        var loan = new LoanItem();
        var s1 = new SpecimenItem("A", "a", "desc a");
        loan.AddSpecimen(s1);

        Assert.Single(loan.Specimens);
        Assert.Equal(s1.ID, loan.Specimens[0].ID);
    }

    [Fact]
    public void SetBorrowerAndEmail_Roundtrips()
    {
        var loan = new LoanItem();
        loan.SetBorrower("Jane");
        loan.SetEmail("jane@test.com");

        Assert.Equal("Jane", loan.Borrower);
        Assert.Equal("jane@test.com", loan.Email);
    }

    [Fact]
    public void SetDates_Roundtrips()
    {
        var loan = new LoanItem();
        var outDate = new DateTime(2026, 1, 1);
        var dueDate = new DateTime(2026, 2, 1);
        loan.SetDates(outDate, dueDate);

        Assert.Equal(outDate, loan.DateCheckedOut);
        Assert.Equal(dueDate, loan.DateDue);
    }
}
