# Tag and Track - Simplified Refactoring Plan (v2)

**Date:** January 25, 2026  
**Author:** GitHub Copilot  
**Project:** Tag and Track (.NET MAUI)  
**Target Framework:** .NET 8

---

## Why This Plan Exists

The previous refactoring plan (v1) was **catastrophically overcomplicated**. It proposed:
- Full MVVM rewrite with 15+ ViewModels
- Repository pattern with multiple abstraction layers
- Dependency injection overhaul
- New navigation service architecture
- 6 phases spanning 6 months
- 3000+ lines of new code

**Reality check:** This is a museum QR code scanner. Staff scan specimens, create loans, and view history. That's it.

This plan focuses on **minimal, surgical fixes** that make the app work without breaking what already exists.

---

## What Actually Needs to Change

### 1. App Flow Change: Login -> Home

**Current:** App starts at `MainPage` (home), login is optional button  
**New:** App starts at `LoginPage`, login goes to `MainPage` (home)

This is a 2-file change.

### 2. Make LoginPage Actually Work

**Current:** Empty shell with just a header  
**New:** Simple employee picker (no passwords!) that sets the active employee

### 3. Fix the Worst Memory Leak

**Current:** `DataTableTemplate` creates hundreds of theme subscriptions that never unsubscribe  
**Fix:** Unsubscribe in a simple cleanup method

### 4. Migrate to SQLite Database

**Current:** In-memory data from CSV strings in `DebugItems.cs`, parsed by `ItemManager`  
**New:** Proper SQLite database with a static `DbService` class for consistent API access

Keep it simple:
- One static `DbService` class with methods like `GetAllSpecimens()`, `GetLoanById()`, etc.
- No repository pattern, no interfaces, no DI - just a clean static API
- Delete `DebugItems.cs` and CSV parsing code after migration

### 5. Add Container Support

**What:** Containers represent physical storage locations (shelves, cabinets, drawers)  
**Model:** Simple - just an ID, name, description, and a list of specimen IDs  
**UI:** Basic list page + detail page (similar to existing loan pages)

### 6. Store Employees in Database

**Current:** In-memory `EmployeeManager` with hardcoded list  
**New:** Employees table in SQLite, loaded via `DbService`

### 7. Add Unit Testing

**Framework:** NUnit (standard for .NET)  
**Scope:** Test `DbService` methods and core business logic only  
**Not testing:** UI, MAUI components, themes

---

## The Actual Changes (In Order)

### Step 1: Change App Start to LoginPage (~10 lines)

**File:** `AppShell.xaml`

Change the Shell content to start at LoginPage instead of MainPage. Register MainPage as a route.

### Step 2: Implement Simple LoginPage (~80 lines)

**File:** `LoginPage.cs`

Add:
- A `Picker` with employee names (loaded from DB)
- A "New Employee" entry field for adding yourself
- A "Login" button that:
  1. Sets `EmployeeManager.ActiveEmployee`
  2. Navigates to MainPage

**No passwords. No authentication service.** Just pick your name and go.

### Step 3: Clean Up MainPage (~5 lines)

**File:** `MainPage.xaml.cs`

- Remove the "Login" button (login is now mandatory at start)
- Optionally show "Logged in as: {name}" in the header

### Step 4: Fix DataTableTemplate Memory Leak (~20 lines)

**File:** `DataTableTemplate.cs`

- Track theme subscriptions in a list
- Add a `Cleanup()` method that unsubscribes all
- Call `Cleanup()` when the parent page disappears

### Step 5: Create SQLite Database & DbService (~200 lines)

**New File:** `Backend/Data/DbService.cs`

Static class with all data access methods:

```csharp
public static class DbService
{
    private static SQLiteAsyncConnection _db;
    
    public static async Task InitAsync()
    {
        if (_db != null) return;
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tagandtrack.db3");
        _db = new SQLiteAsyncConnection(dbPath);
        await _db.CreateTableAsync<SpecimenEntity>();
        await _db.CreateTableAsync<LoanEntity>();
        await _db.CreateTableAsync<ContainerEntity>();
        await _db.CreateTableAsync<EmployeeEntity>();
    }
    
    // Specimens
    public static async Task<List<SpecimenItem>> GetAllSpecimensAsync() { ... }
    public static async Task<SpecimenItem?> GetSpecimenByIdAsync(ulong id) { ... }
    public static async Task AddSpecimenAsync(SpecimenItem specimen) { ... }
    public static async Task UpdateSpecimenAsync(SpecimenItem specimen) { ... }
    
    // Loans
    public static async Task<List<LoanItem>> GetAllLoansAsync() { ... }
    public static async Task<LoanItem?> GetLoanByIdAsync(ulong id) { ... }
    public static async Task AddLoanAsync(LoanItem loan) { ... }
    
    // Containers
    public static async Task<List<ContainerItem>> GetAllContainersAsync() { ... }
    public static async Task<ContainerItem?> GetContainerByIdAsync(ulong id) { ... }
    public static async Task AddContainerAsync(ContainerItem container) { ... }
    
    // Employees
    public static async Task<List<Employee>> GetAllEmployeesAsync() { ... }
    public static async Task AddEmployeeAsync(Employee employee) { ... }
}
```

**New File:** `Backend/Data/Entities/` - Simple SQLite entity classes

```csharp
[Table("Specimens")]
public class SpecimenEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string? ArctosId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsPresent { get; set; } = true;
}

[Table("Containers")]
public class ContainerEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string SpecimenIds { get; set; } = ""; // Comma-separated IDs, simple approach
}

[Table("Employees")]
public class EmployeeEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime? LastLogin { get; set; }
}
```

### Step 6: Add Container Model & Pages (~150 lines)

**Update:** `Backend/Items/ContainerItem.cs` - flesh out the existing stub

```csharp
public class ContainerItem : Item
{
    public List<SpecimenItem> Specimens { get; set; } = new();
    
    public ContainerItem(string name, string description) : base(name, description)
    {
        Type = ItemType.Container;
    }
    
    public void AddSpecimen(SpecimenItem specimen) => Specimens.Add(specimen);
    public void RemoveSpecimen(SpecimenItem specimen) => Specimens.Remove(specimen);
}
```

**New Files:**
- `Pages/MainPages/AllContainersPage.cs` - List all containers (copy pattern from `AllSpecimensPage`)
- `Pages/SupportPages/ViewContainerPage.cs` - View/edit container contents

**Update:** `MainPage.xaml.cs` - Add "Containers" button to home grid

### Step 7: Update Existing Code to Use DbService (~50 lines of changes)

**Delete:** `Backend/Items/DebugItems.cs` (CSV data)  
**Delete:** CSV parsing code from `ItemManager.cs`

**Update:** `ItemManager.cs` - Simplify to just call `DbService`:

```csharp
public static class ItemManager
{
    public static async Task LoadAllItemsAsync()
    {
        // Just delegate to DbService now
        await DbService.InitAsync();
    }
    
    public static Item? GetItemByQRID(string qrid)
    {
        // Parse type and ID from QRID, fetch from DbService
    }
}
```

**Update:** `App.xaml.cs` - Initialize database on startup:

```csharp
public App()
{
    InitializeComponent();
    Task.Run(async () => await DbService.InitAsync()).Wait();
    MainPage = new AppShell();
}
```

### Step 8: Seed Initial Data (~30 lines)

**Add to DbService:**

```csharp
public static async Task SeedIfEmptyAsync()
{
    var specimens = await GetAllSpecimensAsync();
    if (specimens.Count > 0) return; // Already seeded
    
    // Add some sample specimens for testing
    await AddSpecimenAsync(new SpecimenItem("Ursus arctos", "Brown Bear skull"));
    await AddSpecimenAsync(new SpecimenItem("Canis lupus", "Gray Wolf pelt"));
    // ... etc
}
```

### Step 9: Add Unit Tests (~150 lines)

**New Project:** `TagAndTrack.Tests` (NUnit)

```
TagAndTrack.Tests/
├── DbServiceTests.cs
├── ContainerTests.cs
└── LoanTests.cs
```

**Example test file:**

```csharp
[TestFixture]
public class DbServiceTests
{
    [SetUp]
    public async Task Setup()
    {
        // Use in-memory SQLite for tests
        await DbService.InitAsync(":memory:");
    }
    
    [Test]
    public async Task AddSpecimen_ShouldPersist()
    {
        var specimen = new SpecimenItem("Test", "Description");
        await DbService.AddSpecimenAsync(specimen);
        
        var retrieved = await DbService.GetSpecimenByIdAsync(specimen.Id);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved.Name, Is.EqualTo("Test"));
    }
    
    [Test]
    public async Task Container_ShouldHoldSpecimens()
    {
        var specimen = new SpecimenItem("Test", "Desc");
        await DbService.AddSpecimenAsync(specimen);
        
        var container = new ContainerItem("Shelf A", "Top shelf");
        container.AddSpecimen(specimen);
        await DbService.AddContainerAsync(container);
        
        var retrieved = await DbService.GetContainerByIdAsync(container.Id);
        Assert.That(retrieved.Specimens.Count, Is.EqualTo(1));
    }
}
```

---

## What We Are NOT Doing

- MVVM architecture rewrite  
- Dependency injection overhaul  
- Repository pattern / interfaces  
- Navigation service abstraction  
- Offline sync queues  
- AutoMapper or entity mapping libraries
- Complex authentication (passwords, tokens)

The static `DbService` pattern keeps things simple. No interfaces, no DI, no abstractions. Just call `DbService.GetAllSpecimensAsync()` and get data.

---

## Implementation Details

### Step 1: AppShell.xaml Change

```xml
<!-- Before: -->
<ShellContent
    Title="Home"
    ContentTemplate="{DataTemplate local:MainPage}"
    Route="MainPage" />

<!-- After: -->
<ShellContent
    Title="Login"
    ContentTemplate="{DataTemplate pages:LoginPage}"
    Route="LoginPage" />
```

And in `AppShell.xaml.cs`, register MainPage as a route:

```csharp
public AppShell()
{
    InitializeComponent();
    Routing.RegisterRoute("MainPage", typeof(MainPage));
}
```

### Step 2: LoginPage Implementation

```csharp
using TagAndTrack.Backend.Employees;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class LoginPage : TagAndTrackPage
    {
        private Picker employeePicker;
        private Entry newEmployeeEntry;
        
        public LoginPage() { Initialize(); }

        protected override void Initialize()
        {
            // Theme setup (existing pattern)
            Background = CurrentTheme.Instance.Theme.Background;
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    Background = CurrentTheme.Instance.Theme.Background;
            };

            var header = new HeaderTemplate("Employee Login");
            
            var instructions = new Label
            {
                Text = "Select your name or enter a new one:",
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20, 0, 10)
            };
            
            // Employee picker with existing employees
            employeePicker = new Picker
            {
                Title = "Select Employee",
                HorizontalOptions = LayoutOptions.Fill,
                ItemsSource = new List<string> 
                { 
                    "Martin Hundrup",
                    "Museum Staff 1", 
                    "Museum Staff 2" 
                }
            };
            
            var orLabel = new Label
            {
                Text = "- OR -",
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 10)
            };
            
            // New employee entry
            newEmployeeEntry = new Entry
            {
                Placeholder = "Enter your name",
                HorizontalOptions = LayoutOptions.Fill
            };
            
            var loginButton = new TagAndTrackButton("Login", new Command(async () => await LoginAsync()));
            
            Content = new VerticalStackLayout
            {
                Padding = 40,
                Spacing = 15,
                VerticalOptions = LayoutOptions.Center,
                Children = 
                { 
                    header, 
                    instructions,
                    employeePicker, 
                    orLabel,
                    newEmployeeEntry, 
                    loginButton 
                }
            };
        }
        
        private async Task LoginAsync()
        {
            string name = newEmployeeEntry?.Text;
            
            // Prefer typed name over picker selection
            if (string.IsNullOrWhiteSpace(name))
                name = employeePicker?.SelectedItem?.ToString();
            
            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Error", "Please select or enter your name", "OK");
                return;
            }
            
            // Create employee and set as active
            var employee = new Employee(name, GetNextEmployeeId());
            EmployeeManager.SetActiveEmployee(employee);
            
            // Navigate to home
            await Shell.Current.GoToAsync("//MainPage");
        }
        
        private static int _nextId = 1;
        private int GetNextEmployeeId() => _nextId++;
    }
}
```

### Step 3: MainPage Changes

Remove the login button from the buttons array in `MainPage.xaml.cs`:

```csharp
var buttons = new[]
{
    new TagAndTrackButton("Scan Item", new Command(async () => await Navigation.PushAsync(new ScanItemPage()))),
    new TagAndTrackButton("Start Loan", new Command(async () => await Navigation.PushAsync(new StartLoanPage()))),
    new TagAndTrackButton("Loan History", new Command(async () => await Navigation.PushAsync(new LoanHistoryPage()))),
    new TagAndTrackButton("All Specimens", new Command(async () => await Navigation.PushAsync(new AllSpecimensPage()))),
    new TagAndTrackButton("Add Item", new Command(async () => await Navigation.PushAsync(new AddItemPage()))),
    // REMOVED: Login button - login is now required at app start
    new TagAndTrackButton("Settings", new Command(async () => await Navigation.PushAsync(new SettingsPage()))),
    new TagAndTrackButton("Light/Dark Mode", new Command(() => CurrentTheme.Instance.SwitchTheme())),
};
```

### Step 4: DataTableTemplate Memory Leak Fix

At the top of the class, add:

```csharp
private readonly List<Action> _cleanupActions = new();
```

Replace each theme subscription pattern:

```csharp
// BEFORE (leaks memory):
CurrentTheme.Instance.PropertyChanged += (s, e) =>
{
    if (e.PropertyName == nameof(CurrentTheme.Theme))
    {
        border.Stroke = CurrentTheme.Instance.Theme.Borders;
    }
};

// AFTER (can be cleaned up):
PropertyChangedEventHandler handler = (s, e) =>
{
    if (e.PropertyName == nameof(CurrentTheme.Theme))
    {
        border.Stroke = CurrentTheme.Instance.Theme.Borders;
    }
};
CurrentTheme.Instance.PropertyChanged += handler;
_cleanupActions.Add(() => CurrentTheme.Instance.PropertyChanged -= handler);
```

Add cleanup method:

```csharp
public void Cleanup()
{
    foreach (var action in _cleanupActions)
        action();
    _cleanupActions.Clear();
}
```

Call from parent pages in `OnDisappearing()`.

---

## Estimated Effort

| Step | Task | Lines | Time |
|------|------|-------|------|
| 1 | AppShell.xaml + .cs | ~15 | 10 min |
| 2 | LoginPage.cs | ~80 | 30 min |
| 3 | MainPage.xaml.cs | ~10 | 5 min |
| 4 | DataTableTemplate.cs fix | ~30 | 20 min |
| 5 | DbService + Entities | ~200 | 1.5 hr |
| 6 | Container model + pages | ~150 | 1 hr |
| 7 | Update ItemManager | ~50 | 30 min |
| 8 | Data seeding | ~30 | 15 min |
| 9 | Unit tests | ~150 | 1 hr |
| **Total** | | **~715** | **~5.5 hours** |

---

## File Summary

### New Files
- `Backend/Data/DbService.cs` - Static database access class
- `Backend/Data/Entities/SpecimenEntity.cs`
- `Backend/Data/Entities/LoanEntity.cs`
- `Backend/Data/Entities/ContainerEntity.cs`
- `Backend/Data/Entities/EmployeeEntity.cs`
- `Pages/MainPages/AllContainersPage.cs`
- `Pages/SupportPages/ViewContainerPage.cs`
- `TagAndTrack.Tests/` project (NUnit)

### Modified Files
- `AppShell.xaml` + `.cs`
- `LoginPage.cs`
- `MainPage.xaml.cs`
- `DataTableTemplate.cs`
- `ItemManager.cs`
- `ContainerItem.cs`
- `App.xaml.cs`

### Deleted Files
- `Backend/Items/DebugItems.cs`

---

## Future Improvements (Parking Lot)

If the app needs more features later, consider:
- [ ] Settings page for theme/email preferences  
- [ ] Search functionality in All Specimens
- [ ] Due date picker in Finalize Loan
- [ ] Check-in loan functionality
- [ ] Logout button on home page
- [ ] Data export to CSV/Excel

But **only add these when actually needed**, not preemptively.
