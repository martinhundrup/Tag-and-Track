using TagAndTrack.Backend.Employees;

namespace TagAndTrack.Tests;

public class EmployeeTests
{
    [Fact]
    public void Constructor_SetsNameAndId()
    {
        var emp = new Employee("Alice", 42);
        Assert.Equal("Alice", emp.Name);
        Assert.Equal(42, emp.ID);
    }

    [Fact]
    public void DefaultConstructor_UsesDefaults()
    {
        var emp = new Employee();
        Assert.Equal("", emp.Name);
        Assert.Equal(0, emp.ID);
    }

    [Fact]
    public void LastLogin_DefaultsToMinValue()
    {
        var emp = new Employee("Bob", 1);
        Assert.Equal(DateTime.MinValue, emp.LastLogin);
    }

    [Fact]
    public void Login_SetsLastLoginToApproximatelyNow()
    {
        var emp = new Employee("Carol", 2);
        var before = DateTime.Now;
        emp.Login();
        var after = DateTime.Now;

        Assert.InRange(emp.LastLogin, before, after);
    }
}
