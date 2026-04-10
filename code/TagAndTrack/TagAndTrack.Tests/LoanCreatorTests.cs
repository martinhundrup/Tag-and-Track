using TagAndTrack.Backend.Items;
using TagAndTrack.Backend.Utils;

namespace TagAndTrack.Tests;

public class LoanCreatorTests : IDisposable
{
    public LoanCreatorTests()
    {
        LoanCreator.ClearLoan();
    }

    public void Dispose()
    {
        LoanCreator.ClearLoan();
    }

    [Fact]
    public void AddContainerItems_AllAvailable_AllAdded()
    {
        var container = new ContainerItem("Shelf A", "Top shelf");
        var s1 = new SpecimenItem("ARC-1", "Rock", "A rock");
        var s2 = new SpecimenItem("ARC-2", "Fossil", "A fossil");
        var s3 = new SpecimenItem("ARC-3", "Mineral", "A mineral");
        container.AddSpecimen(s1);
        container.AddSpecimen(s2);
        container.AddSpecimen(s3);

        int added = LoanCreator.AddContainerItems(container);

        Assert.Equal(3, added);
        Assert.Equal(3, LoanCreator.LoanItems.Count);
        Assert.Contains(LoanCreator.LoanItems, x => x.ID == s1.ID);
        Assert.Contains(LoanCreator.LoanItems, x => x.ID == s2.ID);
        Assert.Contains(LoanCreator.LoanItems, x => x.ID == s3.ID);
    }

    [Fact]
    public void AddContainerItems_SomeCheckedOut_OnlyAvailableAdded()
    {
        var container = new ContainerItem("Shelf B", "Middle shelf");
        var s1 = new SpecimenItem("ARC-1", "Rock", "A rock");
        var s2 = new SpecimenItem("ARC-2", "Fossil", "A fossil");
        var s3 = new SpecimenItem("ARC-3", "Mineral", "A mineral");
        s2.Checkout(); // already checked out
        container.AddSpecimen(s1);
        container.AddSpecimen(s2);
        container.AddSpecimen(s3);

        int added = LoanCreator.AddContainerItems(container);

        Assert.Equal(2, added);
        Assert.Equal(2, LoanCreator.LoanItems.Count);
        Assert.Contains(LoanCreator.LoanItems, x => x.ID == s1.ID);
        Assert.DoesNotContain(LoanCreator.LoanItems, x => x.ID == s2.ID);
        Assert.Contains(LoanCreator.LoanItems, x => x.ID == s3.ID);
    }

    [Fact]
    public void AddContainerItems_AllCheckedOut_NoneAdded()
    {
        var container = new ContainerItem("Shelf C", "Bottom shelf");
        var s1 = new SpecimenItem("ARC-1", "Rock", "A rock");
        var s2 = new SpecimenItem("ARC-2", "Fossil", "A fossil");
        s1.Checkout();
        s2.Checkout();
        container.AddSpecimen(s1);
        container.AddSpecimen(s2);

        int added = LoanCreator.AddContainerItems(container);

        Assert.Equal(0, added);
        Assert.Empty(LoanCreator.LoanItems);
    }

    [Fact]
    public void AddContainerItems_DuplicatesAlreadyInLoan_NotAddedAgain()
    {
        var s1 = new SpecimenItem("ARC-1", "Rock", "A rock");
        var s2 = new SpecimenItem("ARC-2", "Fossil", "A fossil");

        // Pre-add s1 to the loan directly
        LoanCreator.AddItem(s1);
        Assert.Single(LoanCreator.LoanItems);

        // Now scan a container that also contains s1
        var container = new ContainerItem("Shelf D", "Side shelf");
        container.AddSpecimen(s1);
        container.AddSpecimen(s2);

        int added = LoanCreator.AddContainerItems(container);

        Assert.Equal(1, added); // only s2 was new
        Assert.Equal(2, LoanCreator.LoanItems.Count);
        Assert.Contains(LoanCreator.LoanItems, x => x.ID == s2.ID);
    }

    [Fact]
    public void AddItem_ToEmptyLoan_ReturnsNullOnSuccess()
    {
        var specimen = new SpecimenItem("ARC-1", "Rock", "A rock");

        string? result = LoanCreator.AddItem(specimen);

        Assert.Null(result);
        Assert.Single(LoanCreator.LoanItems);
        Assert.Equal(specimen.ID, LoanCreator.LoanItems[0].ID);
    }
}
