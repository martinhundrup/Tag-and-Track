using TagAndTrack.Backend.Utils;

namespace TagAndTrack.Tests;

public class SpecimenIdHelperTests
{
    [Fact]
    public void ContainsSpecimenId_IdPresent_ReturnsTrue()
    {
        Assert.True(SpecimenIdHelper.ContainsSpecimenId("1,2,3", 2));
    }

    [Fact]
    public void ContainsSpecimenId_IdAbsent_ReturnsFalse()
    {
        Assert.False(SpecimenIdHelper.ContainsSpecimenId("1,2,3", 5));
    }

    [Fact]
    public void ContainsSpecimenId_EmptyString_ReturnsFalse()
    {
        Assert.False(SpecimenIdHelper.ContainsSpecimenId("", 1));
    }

    [Fact]
    public void ContainsSpecimenId_NullString_ReturnsFalse()
    {
        Assert.False(SpecimenIdHelper.ContainsSpecimenId(null, 1));
    }

    [Fact]
    public void ContainsSpecimenId_IdAtStart_ReturnsTrue()
    {
        Assert.True(SpecimenIdHelper.ContainsSpecimenId("5,10,15", 5));
    }

    [Fact]
    public void ContainsSpecimenId_IdAtEnd_ReturnsTrue()
    {
        Assert.True(SpecimenIdHelper.ContainsSpecimenId("5,10,15", 15));
    }

    [Fact]
    public void ContainsSpecimenId_IdInMiddle_ReturnsTrue()
    {
        Assert.True(SpecimenIdHelper.ContainsSpecimenId("5,10,15", 10));
    }

    [Fact]
    public void ContainsSpecimenId_PartialNumericMatch_ReturnsFalse()
    {
        // ID 1 should NOT match "11"
        Assert.False(SpecimenIdHelper.ContainsSpecimenId("11,22,33", 1));
    }

    [Fact]
    public void ContainsSpecimenId_SingleId_ReturnsTrue()
    {
        Assert.True(SpecimenIdHelper.ContainsSpecimenId("42", 42));
    }

    [Fact]
    public void ContainsSpecimenId_SingleId_ReturnsFalse()
    {
        Assert.False(SpecimenIdHelper.ContainsSpecimenId("42", 43));
    }

    [Fact]
    public void ContainsSpecimenId_WhitespaceAroundIds_ReturnsTrue()
    {
        Assert.True(SpecimenIdHelper.ContainsSpecimenId("1, 2, 3", 2));
    }
}
