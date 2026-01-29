// using System.Text.Json;

namespace TagAndTrack.Backend.Employees
{
    public class Employee
    {
        public string Name { get; private set; }

        public DateTime LastLogin { get; set; }

        public int ID { get; private set; }

        public Employee(string name = "", int id = 0)
        {
            Name = name;
            ID = id;
            LastLogin = DateTime.MinValue;
        }

        public void Login()
        {
            LastLogin = DateTime.Now;
        }
    }
}