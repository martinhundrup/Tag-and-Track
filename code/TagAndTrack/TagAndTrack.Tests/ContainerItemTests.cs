using TagAndTrack.Backend.Items;

namespace TagAndTrack.Tests;

public class ContainerItemTests
{
    [Fact]
    public void Type_IsContainer()
    {
        var container = new ContainerItem();
        Assert.Equal(Item.ItemType.Container, container.Type);
    }

    [Fact]
    public void Constructor_SetsNameAndDescription()
    {
        var container = new ContainerItem("Shelf B", "Top shelf");
        Assert.Equal("Shelf B", container.Name);
        Assert.Equal("Top shelf", container.Description);
    }

    [Fact]
    public void AddSpecimen_AppearsInList()
    {
        var container = new ContainerItem();
        var specimen = new SpecimenItem("Rock", "A rock");
        container.AddSpecimen(specimen);

        Assert.Single(container.Specimens);
        Assert.Equal(specimen.ID, container.Specimens[0].ID);
    }

    [Fact]
    public void AddSpecimen_DuplicateId_IsIgnored()
    {
        var container = new ContainerItem();
        var specimen = new SpecimenItem("Rock", "A rock");
        container.AddSpecimen(specimen);
        container.AddSpecimen(specimen); // same object, same ID

        Assert.Single(container.Specimens);
    }

    [Fact]
    public void RemoveSpecimen_RemovesById()
    {
        var container = new ContainerItem();
        var specimen = new SpecimenItem("Rock", "A rock");
        container.AddSpecimen(specimen);
        container.RemoveSpecimen(specimen);

        Assert.Empty(container.Specimens);
    }

    [Fact]
    public void ClearSpecimens_EmptiesList()
    {
        var container = new ContainerItem();
        container.AddSpecimen(new SpecimenItem("A", "a"));
        container.AddSpecimen(new SpecimenItem("B", "b"));
        container.ClearSpecimens();

        Assert.Empty(container.Specimens);
    }

    [Fact]
    public void MultipleSpecimens_AllPresent()
    {
        var container = new ContainerItem();
        var s1 = new SpecimenItem("A", "a");
        var s2 = new SpecimenItem("B", "b");
        var s3 = new SpecimenItem("C", "c");
        container.AddSpecimen(s1);
        container.AddSpecimen(s2);
        container.AddSpecimen(s3);

        Assert.Equal(3, container.Specimens.Count);
    }
}
