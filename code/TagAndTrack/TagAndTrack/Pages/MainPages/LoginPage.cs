using TagAndTrack.Backend;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Employees;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class LoginPage : TagAndTrackPage
    {
        private Picker? employeePicker;
        private TextboxTemplate? newEmployeeEntry;
        private List<Employee> employees = new();

        public LoginPage()
        {
            DebugLogger.Log("LoginPage constructor called");
            Initialize();
        }

        protected override void Initialize()
        {
            Title = null; // No title in navigation bar
            DebugLogger.Log("LoginPage.Initialize() starting");
            Background = CurrentTheme.Instance.Theme.Background;
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    Background = CurrentTheme.Instance.Theme.Background;
            };

            var header = new HeaderTemplate("Employee Login", true);

            var instructions = new Label
            {
                Text = "Select your name or enter a new one:",
                FontSize = 18,
                TextColor = CurrentTheme.Instance.Theme.Text,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 30, 0, 10)
            };
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    instructions.TextColor = CurrentTheme.Instance.Theme.Text;
            };

            // Employee picker - will be populated from DB
            employeePicker = new Picker
            {
                Title = "-- Select Employee --",
                HorizontalOptions = LayoutOptions.Fill,
                WidthRequest = 300,
                FontSize = 16,
                TextColor = CurrentTheme.Instance.Theme.Text,
                TitleColor = Colors.Gray,
                BackgroundColor = CurrentTheme.Instance.Theme.Background
            };
            // When user selects from dropdown, update the text entry to show selected name
            employeePicker.SelectedIndexChanged += (s, e) =>
            {
                if (employeePicker.SelectedIndex >= 0 && employeePicker.SelectedItem != null)
                {
                    var selectedName = employeePicker.SelectedItem.ToString();
                    if (newEmployeeEntry != null)
                    {
                        newEmployeeEntry.textbox.Text = selectedName;
                    }
                }
            };
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    employeePicker.TextColor = CurrentTheme.Instance.Theme.Text;
                    employeePicker.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                }
            };

            // Wrap picker in a border for visibility
            var pickerBorder = new Border
            {
                Stroke = Colors.Gray,
                StrokeThickness = 1,
                Padding = new Thickness(5),
                BackgroundColor = CurrentTheme.Instance.Theme.Background,
                Content = employeePicker
            };
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    pickerBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
            };

            var orLabel = new Label
            {
                Text = "— OR —",
                FontSize = 14,
                TextColor = CurrentTheme.Instance.Theme.Text,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 15)
            };
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    orLabel.TextColor = CurrentTheme.Instance.Theme.Text;
            };

            newEmployeeEntry = new TextboxTemplate(300, "Enter your name");

            var loginButton = new TagAndTrackButton("Login", new Command(async () => await LoginAsync()), "enter.png");

            var pageContent = new VerticalStackLayout
            {
                Padding = 40,
                Spacing = 15,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    instructions,
                    pickerBorder,
                    orLabel,
                    newEmployeeEntry,
                    loginButton
                }
            };

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        header,
                        pageContent
                    }
                }
            };

            // Load employees from database
            DebugLogger.Log("LoginPage.Initialize() calling LoadEmployeesAsync()");
            _ = LoadEmployeesAsync();
            DebugLogger.Log("LoginPage.Initialize() complete");
        }

        private async Task LoadEmployeesAsync()
        {
            try
            {
                DebugLogger.Log("LoadEmployeesAsync() starting...");
                employees = await DbService.GetAllEmployeesAsync();
                DebugLogger.Log($"LoadEmployeesAsync() got {employees.Count} employees");

                foreach (var emp in employees)
                {
                    DebugLogger.Log($"  - Employee: '{emp.Name}' (ID: {emp.ID})");
                }

                if (employeePicker != null)
                {
                    var names = employees.Select(e => e.Name).ToList();
                    DebugLogger.Log($"Setting picker ItemsSource with {names.Count} items");
                    employeePicker.ItemsSource = names;
                }
                else
                {
                    DebugLogger.Log("WARNING: employeePicker is null!");
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"LoadEmployeesAsync ERROR: {ex.GetType().Name}: {ex.Message}");
                DebugLogger.Log($"Stack trace: {ex.StackTrace}");
            }
        }

        private async Task LoginAsync()
        {
            try
            {
                DebugLogger.Log("LoginAsync() called");

                string? name = newEmployeeEntry?.textbox?.Text?.Trim();
                DebugLogger.Log($"Entry text value: '{name ?? "(null)"}'");

                // Prefer typed name over picker selection
                if (string.IsNullOrWhiteSpace(name) && employeePicker?.SelectedItem != null)
                {
                    name = employeePicker.SelectedItem.ToString();
                    DebugLogger.Log($"Using picker selection: '{name}'");
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    DebugLogger.Log("No name provided, showing error alert");
                    await DisplayAlert("Error", "Please select or enter your name", "OK");
                    return;
                }

                DebugLogger.Log($"Looking for existing employee with name: '{name}'");
                DebugLogger.Log($"Current employees list has {employees.Count} items");

                // Check if employee exists
                var existing = employees.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                Employee employee;

                if (existing != null)
                {
                    DebugLogger.Log($"Found existing employee: {existing.Name} (ID: {existing.ID})");
                    employee = existing;
                    DebugLogger.Log("Calling UpdateEmployeeLoginAsync...");
                    await DbService.UpdateEmployeeLoginAsync(employee.ID);
                    DebugLogger.Log("UpdateEmployeeLoginAsync completed");
                }
                else
                {
                    DebugLogger.Log($"Creating new employee: '{name}'");
                    int newId = await DbService.AddEmployeeAsync(name);
                    DebugLogger.Log($"New employee created with ID: {newId}");
                    employee = new Employee(name, newId);
                }

                DebugLogger.Log("Calling employee.Login()");
                employee.Login();

                DebugLogger.Log("Calling EmployeeManager.SetActiveEmployee()");
                EmployeeManager.SetActiveEmployee(employee);

                DebugLogger.Log($"Employee logged in: {employee.Name} (ID: {employee.ID})");

                DebugLogger.Log("Navigating to MainPage...");
                await Shell.Current.GoToAsync("//MainPage");
                DebugLogger.Log("Navigation completed");
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"LoginAsync ERROR: {ex.GetType().Name}: {ex.Message}");
                DebugLogger.Log($"Stack trace: {ex.StackTrace}");
                await DisplayAlert("Login Error", ex.Message, "OK");
            }
        }
    }
}