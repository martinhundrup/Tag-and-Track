using TagAndTrack.Backend;

namespace TagAndTrack.Tests;

public class UniqueIdGeneratorTests
{
    [Fact]
    public void NewId_ReturnsNonZero()
    {
        var id = UniqueIdGenerator.NewId();
        Assert.NotEqual(0UL, id);
    }

    [Fact]
    public void NewId_ProducesDistinctValues()
    {
        var ids = Enumerable.Range(0, 50).Select(_ => UniqueIdGenerator.NewId()).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void NewStringId_ReturnsNonEmpty()
    {
        var id = UniqueIdGenerator.NewStringId();
        Assert.False(string.IsNullOrEmpty(id));
    }

    [Fact]
    public void NewStringId_ContainsOnlyBase36Characters()
    {
        var id = UniqueIdGenerator.NewStringId();
        Assert.Matches("^[0-9A-Z]+$", id);
    }
}
