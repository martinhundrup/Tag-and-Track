// using System.Text.Json;

namespace TagAndTrack.Backend.Employees
{
    public class Employee
    {
        public string Name
        {
            get;
            private set;
        }

        public DateTime LastLogin
        {
            get;
            set;
        }

        public int ID{
            get;
            private set;
        }

        public Employee(string name = "", int id = 0)
        {
            Name = name;
            ID = id;
            LastLogin = DateTime.MinValue;
        }

        public void Login(){
            LastLogin = DateTime.Now;
        }

        // Not needed, perhaps useful later
        // private string ToJson()
        // {
        //     return JsonSerializer.Serialize(this);
        // }

        // public Employee FromJson(string json)
        // {
        //     var other = JsonSerializer.Deserialize<Employee>(json);
        //     if (other == null) throw new ArgumentException("Invalid JSON for Employee", nameof(json));

        //     Name = other.Name;
        //     LastLogin = other.LastLogin;
        //     return this;
        // }
    }
}