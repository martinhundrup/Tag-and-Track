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
    public void Checkin_CascadesToSpecimens()
    {
        var loan = new LoanItem("Loan", "desc");
        var s1 = new SpecimenItem("A", "a");
        var s2 = new SpecimenItem("B", "b");
        loan.AddSpecimen(s1);
        loan.AddSpecimen(s2);

        // Check everything out
        loan.Checkout();
        s1.Checkout();
        s2.Checkout();

        Assert.False(s1.Status);
        Assert.False(s2.Status);

        // Check in the loan — should cascade
        loan.Checkin();

        Assert.True(loan.Status);
        Assert.True(s1.Status);
        Assert.True(s2.Status);
    }

    [Fact]
    public void Specimens_IsReadOnlyView()
    {
        var loan = new LoanItem();
        var s1 = new SpecimenItem("A", "a");
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
