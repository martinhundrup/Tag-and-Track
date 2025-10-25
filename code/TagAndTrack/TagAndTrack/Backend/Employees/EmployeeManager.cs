using TagAndTrack.Backend.Items;

namespace TagAndTrack.Backend.Employees
{
    public static class EmployeeManager
    {
        public static Employee? ActiveEmployee
        {
            get;
            private set;
        }

        private static List<Employee> employees = new List<Employee>();

        public static Employee? GetEmployeeByID(int id)
        {
            return employees.FirstOrDefault(e => e.ID == id);
        }

        public static void SetActiveEmployee(Employee? employee)
        {
            ActiveEmployee = employee;
        }

        public static void AddEmployee(Employee employee)
        {
            if (!employees.Any(e => e.ID == employee.ID))
            {
                employees.Add(employee);
            }
            else
            {
                throw new ArgumentException($"Employee with ID {employee.ID} already exists.");
            }
        }
    }
}