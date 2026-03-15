using System.Reflection;
using TagAndTrack.Backend.Employees;

namespace TagAndTrack.Tests;

public class EmployeeManagerTests : IDisposable
{
    public EmployeeManagerTests()
    {
        // Reset static state before each test
        var field = typeof(EmployeeManager).GetField("employees", BindingFlags.NonPublic | BindingFlags.Static)!;
        ((List<Employee>)field.GetValue(null)!).Clear();
        EmployeeManager.SetActiveEmployee(null);
    }

    public void Dispose() { }

    [Fact]
    public void AddEmployee_ThenGetByID_ReturnsEmployee()
    {
        var emp = new Employee("Alice", 10);
        EmployeeManager.AddEmployee(emp);

        var result = EmployeeManager.GetEmployeeByID(10);

        Assert.NotNull(result);
        Assert.Equal("Alice", result!.Name);
    }

    [Fact]
    public void AddEmployee_DuplicateId_Throws()
    {
        var emp1 = new Employee("Alice", 20);
        var emp2 = new Employee("Bob", 20);
        EmployeeManager.AddEmployee(emp1);

        Assert.Throws<ArgumentException>(() => EmployeeManager.AddEmployee(emp2));
    }

    [Fact]
    public void GetEmployeeByID_UnknownId_ReturnsNull()
    {
        var result = EmployeeManager.GetEmployeeByID(999);
        Assert.Null(result);
    }

    [Fact]
    public void SetActiveEmployee_RoundTrips()
    {
        var emp = new Employee("Carol", 30);
        EmployeeManager.SetActiveEmployee(emp);

        Assert.Equal(emp, EmployeeManager.ActiveEmployee);
    }

    [Fact]
    public void SetActiveEmployee_Null_ClearsActive()
    {
        var emp = new Employee("Dave", 40);
        EmployeeManager.SetActiveEmployee(emp);
        EmployeeManager.SetActiveEmployee(null);

        Assert.Null(EmployeeManager.ActiveEmployee);
    }
}
