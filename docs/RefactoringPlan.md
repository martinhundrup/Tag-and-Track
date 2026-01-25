# Tag and Track - Comprehensive Refactoring Plan

**Date:** January 23, 2026  
**Author:** GitHub Copilot  
**Project:** Tag and Track (.NET MAUI)  
**Target Framework:** .NET 8

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Current State Analysis](#2-current-state-analysis)
3. [Refactoring Phases Overview](#3-refactoring-phases-overview)
4. [Phase 1: Code Cleanup & Standards](#phase-1-code-cleanup--standards)
5. [Phase 2: Architecture & MVVM Implementation](#phase-2-architecture--mvvm-implementation)
6. [Phase 3: Database & Data Layer](#phase-3-database--data-layer)
7. [Phase 4: Missing Features Implementation](#phase-4-missing-features-implementation)
8. [Phase 5: Unit Testing Framework](#phase-5-unit-testing-framework)
9. [Phase 6: Performance Optimization](#phase-6-performance-optimization)
10. [Project Structure Reorganization](#project-structure-reorganization)
11. [Implementation Timeline](#implementation-timeline)
12. [Questions & Assumptions](#questions--assumptions)

---

## 1. Executive Summary

This document outlines a comprehensive refactoring plan for the Tag and Track .NET MAUI application. The current codebase has functional scanning and loan management features but suffers from architectural issues, code duplication, and incomplete features that prevent production readiness.

### Key Goals
- ✅ Clean up code and enforce consistent coding standards
- ✅ Implement MVVM architecture with proper separation of concerns
- ✅ Create a proper data layer with repository pattern (replacing CSV-based debug data)
- ✅ Complete missing features (SQLite database, employee login, settings, containers, offline mode)
- ✅ Add data export functionality (.xlsx/.csv)
- ✅ Add comprehensive unit testing with NUnit
- ✅ Optimize performance (especially DataTableTemplate memory leaks)

### Target Platform
**Primary:** iPadOS (deployed via direct installation, not App Store)

### Critical Issues Requiring Immediate Attention
1. **Memory Leaks** - ~150+ event handlers in `DataTableTemplate` that are never unsubscribed
2. **Global Mutable State** - `ScannedQRItem.lastScannedItem` used for navigation state
3. **Two Parallel Data Systems** - SQLite database exists but isn't used; CSV debug data is primary
4. **No Employee Login** - `LoginPage` is an empty shell
5. **Hardcoded Email Credentials** - Security risk in `Emailer.cs`
6. **Missing Container Feature** - Container model exists but no UI or full implementation
7. **No Offline Mode** - Required but not implemented
8. **No Data Export** - Users need to export loan history to .xlsx/.csv

---

## 2. Current State Analysis

### 2.1 Project Overview

| Aspect | Current State |
|--------|---------------|
| Framework | .NET MAUI (.NET 8) |
| Target Platforms | iOS, macOS Catalyst, Windows |
| Architecture | Code-behind (no MVVM) |
| Database | SQLite (implemented but unused) |
| Data Source | CSV strings embedded in code |
| Testing | None |
| DI Container | Not configured |

### 2.2 File Structure Analysis

```
TagAndTrack/
├── Backend/
│   ├── DebugLogger.cs           # File-based logging
│   ├── debugSpecimens.csv       # Debug data (not used, data in DebugItems.cs)
│   ├── Employees/
│   │   ├── Employee.cs          # Basic employee model
│   │   └── EmployeeManager.cs   # Static manager (in-memory only)
│   ├── Items/
│   │   ├── Item.cs              # Base item class
│   │   ├── ContainerItem.cs     # Container model (stub)
│   │   ├── DebugItems.cs        # CSV strings embedded as constants
│   │   ├── ItemManager.cs       # Static manager with reflection abuse
│   │   ├── LoanItem.cs          # Loan model
│   │   ├── ScannedQRItem.cs     # GLOBAL MUTABLE STATE for navigation
│   │   └── SpecimenItem.cs      # Specimen model
│   └── Utils/
│       ├── Emailer.cs           # SMTP email with hardcoded credentials
│       ├── LoanCreator.cs       # Static loan builder
│       └── UniqueIdGenerator.cs # ID generation
├── Components/
│   ├── DataTableTemplate.cs     # 708 lines, massive memory leaks
│   ├── HeaderTemplate.cs        # Header with theme subscription
│   ├── ScanView.cs              # QR scanner wrapper
│   ├── TagAndTrackButton.cs     # Custom button
│   └── Themes/
│       ├── CurrentTheme.cs      # Singleton theme manager
│       ├── DarkTheme.cs
│       ├── LightTheme.cs
│       └── Theme.cs
├── Databases/
│   ├── Container.cs             # SQLite entity (unused)
│   ├── Database.cs              # SQLite connection (unused)
│   ├── Loan.cs                  # SQLite entity (unused)
│   └── Specimen.cs              # SQLite entity (unused)
└── Pages/
    ├── MainPages/
    │   ├── AddItemPage.cs
    │   ├── AllSpecimensPage.cs
    │   ├── CheckInLoanPage.cs   # Empty ScanCaptured handler
    │   ├── LoanHistoryPage.cs
    │   ├── LoginPage.cs         # Empty shell
    │   ├── ScanItemPage.cs
    │   ├── SettingsPage.cs      # Shows debug QR codes only
    │   ├── StartLoanPage.cs
    │   └── TagAndTrackPage.cs   # Base page class
    └── SupportPages/
        ├── FinalizeLoanPage.cs
        ├── ViewEditLoanItemsPage.cs
        └── ViewItemPage.cs
```

### 2.3 Critical Code Issues

#### 2.3.1 Memory Leaks (CRITICAL)

**File:** `DataTableTemplate.cs` (708 lines)

The `DataTableTemplate` class creates dozens to hundreds of event subscriptions that are **never unsubscribed**:

```csharp
// This pattern appears 30+ times in the file
CurrentTheme.Instance.PropertyChanged += (s, e) =>
{
    if (e.PropertyName == nameof(CurrentTheme.Theme))
    {
        border.Stroke = CurrentTheme.Instance.Theme.Borders;
        // ...
    }
};
```

**Impact:** Each row in a data table creates multiple subscriptions. With 1000 specimens, this creates thousands of event handlers that:
- Keep `DataTableTemplate` instances alive indefinitely
- Cause increasing memory usage
- Slow down theme changes exponentially

**Fix:** Implement `IDisposable`, track subscriptions, unsubscribe in `Dispose()` or use weak event patterns.

#### 2.3.2 Global Mutable State (HIGH)

**File:** `ScannedQRItem.cs`

```csharp
internal static class ScannedQRItem
{
    public static string? lastScannedItem;  // Global state!
}
```

Used throughout pages to pass navigation state:
```csharp
ScannedQRItem.lastScannedItem = qr;
await Navigation.PushAsync(new ViewItemPage());
// ViewItemPage reads ScannedQRItem.lastScannedItem
```

**Impact:** Race conditions, unpredictable behavior, untestable code.

**Fix:** Use navigation parameters, dependency injection, or a proper navigation service.

#### 2.3.3 Reflection Abuse (MEDIUM)

**File:** `ItemManager.cs`

```csharp
private static readonly PropertyInfo? IdProp =
    typeof(Item).GetProperty("ID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

// Used to set protected setters
IdProp?.SetValue(specimen, id);
```

**Impact:** Bypasses type safety, harder to refactor, performance overhead.

**Fix:** Make setters `internal` or use factory methods/constructors.

#### 2.3.4 Duplicate Theme Subscription Code (MEDIUM)

Every page has nearly identical code:
```csharp
Background = CurrentTheme.Instance.Theme.Background;
CurrentTheme.Instance.PropertyChanged += (s, e) =>
{
    if (e.PropertyName == nameof(CurrentTheme.Theme))
        Background = CurrentTheme.Instance.Theme.Background;
};
```

**Fix:** Move to `TagAndTrackPage` base class or use XAML bindings.

#### 2.3.5 Hardcoded Credentials (CRITICAL SECURITY)

**File:** `Emailer.cs`

```csharp
const string fromEmail = "TagAndTrackWSU@gmail.com";
const string appPassword = "";  // Was filled in, now empty
```

**Fix:** Use secure configuration (environment variables, Azure Key Vault, or .NET Secret Manager).

#### 2.3.6 Two Parallel Data Systems (HIGH)

**System 1: SQLite Database** (Unused)
- `Databases/Database.cs` - Full CRUD implementation
- `Databases/Specimen.cs`, `Loan.cs`, `Container.cs` - SQLite entities

**System 2: In-Memory CSV** (Active)
- `Backend/Items/DebugItems.cs` - CSV strings
- `Backend/Items/ItemManager.cs` - Parses CSV at runtime

**Impact:** Database code is dead weight; data doesn't persist between sessions.

**Fix:** Remove CSV system, activate SQLite database with proper repository pattern.

#### 2.3.7 Incomplete Features

| Feature | Status | Priority | Notes |
|---------|--------|----------|-------|
| `LoginPage` | Empty shell | **HIGH** | No employee login logic |
| `SettingsPage` | Debug only | **HIGH** | Shows test QR codes, no settings |
| `CheckInLoanPage.ScanCaptured` | Empty | **HIGH** | Handler does nothing |
| Container Management | Model only | **HIGH** | No UI or full CRUD |
| Offline Mode | Not started | **HIGH** | Required - must work without network |
| Data Export | Not started | **MEDIUM** | Export to .xlsx/.csv required |
| Due date picker | TODO comment | **MEDIUM** | Uses `DateTime.MaxValue` as placeholder |
| Search functionality | UI only | **MEDIUM** | No implementation |
| Arctos API integration | Not started | **LOW** | Deferred - waiting on API keys |

### 2.4 Architecture Issues Summary

| Issue | Severity | Files Affected |
|-------|----------|----------------|
| No MVVM pattern | High | All 12+ pages |
| Memory leaks (event handlers) | Critical | `DataTableTemplate`, all components |
| Global mutable state | High | `ScannedQRItem`, `ItemManager`, `LoanCreator` |
| No dependency injection | High | Entire application |
| Reflection for property access | Medium | `ItemManager` |
| Duplicate theme code | Medium | Every page and component |
| Unused database implementation | High | `Databases/` folder |
| Hardcoded credentials | Critical | `Emailer.cs` |

---

## 3. Refactoring Phases Overview

```
┌─────────────────────────────────────────────────────────────────┐
│  Phase 1: Code Cleanup & Standards (2-3 weeks)                  │
│  - Fix critical bugs (memory leaks, thread safety)              │
│  - Standardize naming, remove dead code                         │
│  - Configure analyzers                                          │
├─────────────────────────────────────────────────────────────────┤
│  Phase 2: Architecture & MVVM (3-4 weeks)                       │
│  - Implement base ViewModel                                     │
│  - Add dependency injection                                     │
│  - Create navigation service                                    │
│  - Migrate pages to MVVM                                        │
├─────────────────────────────────────────────────────────────────┤
│  Phase 3: Database & Data Layer (2-3 weeks)                     │
│  - Unify data models                                            │
│  - Implement repository pattern                                 │
│  - Migrate from CSV to SQLite                                   │
│  - Add data seeding                                             │
├─────────────────────────────────────────────────────────────────┤
│  Phase 4: Missing Features (4-5 weeks)                          │
│  - Employee login (username only, no password)                  │
│  - Settings management (admin email config)                     │
│  - Container management (full CRUD + UI)                        │
│  - Offline mode with sync                                       │
│  - Data export (.xlsx/.csv)                                     │
│  - Complete loan check-in                                       │
│  - Due date picker                                              │
│  - Search functionality                                         │
├─────────────────────────────────────────────────────────────────┤
│  Phase 5: Unit Testing (2 weeks)                                │
│  - Set up NUnit project                                         │
│  - Test services and repositories                               │
│  - Test ViewModels                                              │
├─────────────────────────────────────────────────────────────────┤
│  Phase 6: Performance Optimization (1-2 weeks)                  │
│  - Profile and optimize data loading                            │
│  - Implement virtualization for large lists                     │
│  - Review and optimize async patterns                           │
└─────────────────────────────────────────────────────────────────┘
```

---

## Phase 1: Code Cleanup & Standards

### 1.1 Fix Critical Memory Leaks

#### Task 1.1.1: Create Disposable Base Class for Components

```csharp
// New file: Components/DisposableContentView.cs
public abstract class DisposableContentView : ContentView, IDisposable
{
    private readonly List<IDisposable> _subscriptions = new();
    private bool _disposed;

    protected void TrackSubscription(Action unsubscribe)
    {
        _subscriptions.Add(new ActionDisposable(unsubscribe));
    }

    protected void SubscribeToTheme(Action<Theme> onThemeChanged)
    {
        void Handler(object? s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentTheme.Theme))
                onThemeChanged(CurrentTheme.Instance.Theme);
        }
        
        CurrentTheme.Instance.PropertyChanged += Handler;
        TrackSubscription(() => CurrentTheme.Instance.PropertyChanged -= Handler);
        
        // Apply immediately
        onThemeChanged(CurrentTheme.Instance.Theme);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        foreach (var sub in _subscriptions)
            sub.Dispose();
        _subscriptions.Clear();
        
        OnDispose();
        GC.SuppressFinalize(this);
    }

    protected virtual void OnDispose() { }

    private class ActionDisposable : IDisposable
    {
        private Action? _action;
        public ActionDisposable(Action action) => _action = action;
        public void Dispose() { _action?.Invoke(); _action = null; }
    }
}
```

#### Task 1.1.2: Refactor DataTableTemplate

**Current:** 708 lines with 30+ inline event subscriptions  
**Target:** ~300 lines using `CollectionView` with proper data binding

```csharp
// Replacement approach using CollectionView
public class SpecimenDataTable : DisposableContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<SpecimenItem>), 
            typeof(SpecimenDataTable), propertyChanged: OnItemsSourceChanged);

    public IEnumerable<SpecimenItem> ItemsSource
    {
        get => (IEnumerable<SpecimenItem>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private readonly CollectionView _collectionView;

    public SpecimenDataTable()
    {
        _collectionView = new CollectionView
        {
            ItemTemplate = new DataTemplate(() => CreateRow()),
            SelectionMode = SelectionMode.None
        };
        
        SubscribeToTheme(theme => BackgroundColor = theme.Background);
        Content = _collectionView;
    }

    private View CreateRow() { /* ... */ }
}
```

### 1.2 Fix Global State Issues

#### Task 1.2.1: Remove ScannedQRItem Static Class

Replace with navigation parameters:

```csharp
// ViewItemPage.cs - Before
public ViewItemPage()
{
    var qrid = ScannedQRItem.lastScannedItem; // Global state
}

// ViewItemPage.cs - After
public ViewItemPage(string qrId)
{
    _qrId = qrId;
}

// Calling page
await Navigation.PushAsync(new ViewItemPage(item.QRID));
```

Better approach with DI and query parameters (Phase 2):
```csharp
[QueryProperty(nameof(QrId), "qrId")]
public partial class ViewItemPage : ContentPage
{
    public string QrId { get; set; }
}

// Navigation
await Shell.Current.GoToAsync($"viewitem?qrId={item.QRID}");
```

### 1.3 Fix Naming & Standards

| Current | Fixed | Location |
|---------|-------|----------|
| `spceimen` | `specimen` | `DataTableTemplate.cs` (multiple) |
| `UniqeIdGenerator` | `UniqueIdGenerator` | File rename + class rename |
| `lastScannedItem` | Remove entirely | `ScannedQRItem.cs` |
| `specimentIDs` | `specimenIds` | `Databases/Loan.cs` |

### 1.4 Remove Dead Code

Files to evaluate for removal:
- [ ] `Backend/debugSpecimens.csv` - Unused file
- [ ] `Databases/` folder contents - If migrating to unified model
- [ ] `NewFolder/` - Empty directory

### 1.5 Configure Static Analysis

Add `.editorconfig` and analyzer packages:

```xml
<!-- Add to TagAndTrack.csproj -->
<ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" />
    <PackageReference Include="CommunityToolkit.Maui" Version="7.0.0" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
</ItemGroup>

<PropertyGroup>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
</PropertyGroup>
```

---

## Phase 2: Architecture & MVVM Implementation

### 2.1 Project Structure (Target)

```
TagAndTrack/
├── App.xaml / App.xaml.cs
├── AppShell.xaml / AppShell.xaml.cs
├── MauiProgram.cs
├── Models/
│   ├── Item.cs
│   ├── SpecimenItem.cs
│   ├── LoanItem.cs
│   ├── ContainerItem.cs
│   ├── Employee.cs
│   └── Settings.cs
├── ViewModels/
│   ├── Base/
│   │   └── BaseViewModel.cs
│   ├── MainPageViewModel.cs
│   ├── ScanItemViewModel.cs
│   ├── StartLoanViewModel.cs
│   ├── AllSpecimensViewModel.cs
│   ├── ViewItemViewModel.cs
│   ├── LoginViewModel.cs
│   └── SettingsViewModel.cs
├── Views/
│   ├── MainPage.xaml
│   ├── ScanItemPage.xaml
│   ├── StartLoanPage.xaml
│   ├── AllSpecimensPage.xaml
│   ├── ViewItemPage.xaml
│   ├── LoginPage.xaml
│   └── SettingsPage.xaml
├── Services/
│   ├── Interfaces/
│   │   ├── INavigationService.cs
│   │   ├── IItemService.cs
│   │   ├── ILoanService.cs
│   │   ├── IEmployeeService.cs
│   │   ├── IEmailService.cs
│   │   └── ISettingsService.cs
│   ├── NavigationService.cs
│   ├── ItemService.cs
│   ├── LoanService.cs
│   ├── EmployeeService.cs
│   ├── EmailService.cs
│   └── SettingsService.cs
├── Data/
│   ├── Repositories/
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs
│   │   │   ├── ISpecimenRepository.cs
│   │   │   ├── ILoanRepository.cs
│   │   │   └── IEmployeeRepository.cs
│   │   ├── SpecimenRepository.cs
│   │   ├── LoanRepository.cs
│   │   └── EmployeeRepository.cs
│   ├── Entities/
│   │   ├── SpecimenEntity.cs
│   │   ├── LoanEntity.cs
│   │   └── EmployeeEntity.cs
│   ├── TagAndTrackDbContext.cs
│   └── DatabaseInitializer.cs
├── Components/
│   ├── DataTable.cs
│   ├── HeaderView.cs
│   ├── ScannerView.cs
│   └── QrCodeView.cs
├── Themes/
│   ├── ITheme.cs
│   ├── LightTheme.cs
│   ├── DarkTheme.cs
│   └── ThemeService.cs
├── Helpers/
│   ├── Constants.cs
│   └── Extensions.cs
└── Resources/
    └── (existing structure)
```

### 2.2 Base ViewModel Implementation

```csharp
// ViewModels/Base/BaseViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TagAndTrack.ViewModels.Base;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    public bool IsNotBusy => !IsBusy;

    protected readonly INavigationService NavigationService;

    protected BaseViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    [RelayCommand]
    protected virtual async Task GoBackAsync()
    {
        await NavigationService.GoBackAsync();
    }

    protected async Task ExecuteAsync(Func<Task> operation, string? loadingMessage = null)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            await operation();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            // Log error
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### 2.3 Dependency Injection Setup

```csharp
// MauiProgram.cs
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Database
        builder.Services.AddSingleton<TagAndTrackDbContext>();
        builder.Services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();

        // Repositories
        builder.Services.AddSingleton<ISpecimenRepository, SpecimenRepository>();
        builder.Services.AddSingleton<ILoanRepository, LoanRepository>();
        builder.Services.AddSingleton<IEmployeeRepository, EmployeeRepository>();

        // Services
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IItemService, ItemService>();
        builder.Services.AddSingleton<ILoanService, LoanService>();
        builder.Services.AddSingleton<IEmployeeService, EmployeeService>();
        builder.Services.AddSingleton<IEmailService, EmailService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();

        // ViewModels
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<ScanItemViewModel>();
        builder.Services.AddTransient<StartLoanViewModel>();
        builder.Services.AddTransient<AllSpecimensViewModel>();
        builder.Services.AddTransient<ViewItemViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ScanItemPage>();
        builder.Services.AddTransient<StartLoanPage>();
        builder.Services.AddTransient<AllSpecimensPage>();
        builder.Services.AddTransient<ViewItemPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
```

### 2.4 Navigation Service

```csharp
// Services/Interfaces/INavigationService.cs
public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoToAsync(string route, IDictionary<string, object> parameters);
    Task GoBackAsync();
    Task GoToRootAsync();
}

// Services/NavigationService.cs
public class NavigationService : INavigationService
{
    public async Task GoToAsync(string route)
    {
        await Shell.Current.GoToAsync(route);
    }

    public async Task GoToAsync(string route, IDictionary<string, object> parameters)
    {
        await Shell.Current.GoToAsync(route, parameters);
    }

    public async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    public async Task GoToRootAsync()
    {
        await Shell.Current.GoToAsync("//main");
    }
}
```

### 2.5 Example ViewModel Migration: ScanItemPage

```csharp
// ViewModels/ScanItemViewModel.cs
public partial class ScanItemViewModel : BaseViewModel
{
    private readonly IItemService _itemService;

    [ObservableProperty]
    private string _scanStatus = "Looking for a QR code...";

    [ObservableProperty]
    private bool _isScanning = true;

    public ScanItemViewModel(
        INavigationService navigationService,
        IItemService itemService) : base(navigationService)
    {
        _itemService = itemService;
        Title = "Scan Item";
    }

    [RelayCommand]
    private async Task ProcessScanAsync(string qrCode)
    {
        if (IsBusy) return;

        await ExecuteAsync(async () =>
        {
            IsScanning = false;
            
            var item = await _itemService.GetItemByQrIdAsync(qrCode);
            
            if (item == null)
            {
                ScanStatus = $"Value {qrCode} not recognized!";
                IsScanning = true;
                return;
            }

            await NavigationService.GoToAsync("viewitem", new Dictionary<string, object>
            {
                { "qrId", qrCode }
            });
        });
    }
}
```

---

## Phase 3: Database & Data Layer

### 3.1 Unified Data Model

Create a single source of truth for domain models:

```csharp
// Models/Item.cs
public abstract class Item
{
    public ulong Id { get; set; }
    public string? ArctosId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPresent { get; set; } = true;
    public ItemType Type { get; protected set; }
    
    public string QrId => $"{Type}:{Id}";
}

public enum ItemType
{
    Unknown = 0,
    Specimen = 1,
    Loan = 2,
    Container = 3
}

// Models/SpecimenItem.cs
public class SpecimenItem : Item
{
    public SpecimenItem()
    {
        Type = ItemType.Specimen;
    }
}

// Models/LoanItem.cs
public class LoanItem : Item
{
    public LoanItem()
    {
        Type = ItemType.Loan;
    }

    public string? Borrower { get; set; }
    public string? BorrowerEmail { get; set; }
    public DateTime DateCheckedOut { get; set; }
    public DateTime DateDue { get; set; }
    public List<SpecimenItem> Specimens { get; set; } = new();
}
```

### 3.2 Database Entities (Separate from Domain Models)

```csharp
// Data/Entities/SpecimenEntity.cs
[Table("Specimens")]
public class SpecimenEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string? ArctosId { get; set; }
    
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public bool IsPresent { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Data/Entities/LoanEntity.cs
[Table("Loans")]
public class LoanEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Borrower { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string BorrowerEmail { get; set; } = string.Empty;
    
    public DateTime DateCheckedOut { get; set; }
    public DateTime DateDue { get; set; }
    public bool IsReturned { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Data/Entities/LoanSpecimenEntity.cs (Junction table)
[Table("LoanSpecimens")]
public class LoanSpecimenEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Indexed]
    public int LoanId { get; set; }
    
    [Indexed]
    public int SpecimenId { get; set; }
}
```

### 3.3 Repository Pattern Implementation

```csharp
// Data/Repositories/Interfaces/IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<int> AddAsync(T entity);
    Task<int> UpdateAsync(T entity);
    Task<int> DeleteAsync(int id);
}

// Data/Repositories/Interfaces/ISpecimenRepository.cs
public interface ISpecimenRepository : IRepository<SpecimenEntity>
{
    Task<SpecimenEntity?> GetByArctosIdAsync(string arctosId);
    Task<IEnumerable<SpecimenEntity>> GetByStatusAsync(bool isPresent);
    Task<IEnumerable<SpecimenEntity>> SearchAsync(string searchTerm);
}

// Data/Repositories/SpecimenRepository.cs
public class SpecimenRepository : ISpecimenRepository
{
    private readonly TagAndTrackDbContext _context;

    public SpecimenRepository(TagAndTrackDbContext context)
    {
        _context = context;
    }

    public async Task<SpecimenEntity?> GetByIdAsync(int id)
    {
        return await _context.Connection
            .Table<SpecimenEntity>()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<SpecimenEntity>> GetAllAsync()
    {
        return await _context.Connection
            .Table<SpecimenEntity>()
            .ToListAsync();
    }

    public async Task<int> AddAsync(SpecimenEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _context.Connection.InsertAsync(entity);
    }

    public async Task<int> UpdateAsync(SpecimenEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        return await _context.Connection.UpdateAsync(entity);
    }

    public async Task<int> DeleteAsync(int id)
    {
        return await _context.Connection.DeleteAsync<SpecimenEntity>(id);
    }

    public async Task<SpecimenEntity?> GetByArctosIdAsync(string arctosId)
    {
        return await _context.Connection
            .Table<SpecimenEntity>()
            .FirstOrDefaultAsync(s => s.ArctosId == arctosId);
    }

    public async Task<IEnumerable<SpecimenEntity>> GetByStatusAsync(bool isPresent)
    {
        return await _context.Connection
            .Table<SpecimenEntity>()
            .Where(s => s.IsPresent == isPresent)
            .ToListAsync();
    }

    public async Task<IEnumerable<SpecimenEntity>> SearchAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();
        return await _context.Connection
            .Table<SpecimenEntity>()
            .Where(s => s.Name.ToLower().Contains(term) || 
                       s.Description.ToLower().Contains(term) ||
                       s.ArctosId.ToLower().Contains(term))
            .ToListAsync();
    }
}
```

### 3.4 Database Context

```csharp
// Data/TagAndTrackDbContext.cs
public class TagAndTrackDbContext
{
    private SQLiteAsyncConnection? _connection;
    private readonly string _dbPath;

    public SQLiteAsyncConnection Connection => _connection 
        ?? throw new InvalidOperationException("Database not initialized");

    public TagAndTrackDbContext()
    {
        _dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TagAndTrack.db3");
    }

    public async Task InitializeAsync()
    {
        if (_connection != null) return;

        _connection = new SQLiteAsyncConnection(_dbPath, SQLiteOpenFlags.ReadWrite | 
            SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

        await _connection.CreateTableAsync<SpecimenEntity>();
        await _connection.CreateTableAsync<LoanEntity>();
        await _connection.CreateTableAsync<LoanSpecimenEntity>();
        await _connection.CreateTableAsync<EmployeeEntity>();
        await _connection.CreateTableAsync<SettingsEntity>();
    }
}
```

### 3.5 Data Migration from CSV

Create a one-time migration service to import existing debug data:

```csharp
// Data/DatabaseInitializer.cs
public interface IDatabaseInitializer
{
    Task InitializeAsync();
    Task SeedDebugDataAsync();
}

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly TagAndTrackDbContext _context;
    private readonly ISpecimenRepository _specimenRepository;

    public DatabaseInitializer(
        TagAndTrackDbContext context,
        ISpecimenRepository specimenRepository)
    {
        _context = context;
        _specimenRepository = specimenRepository;
    }

    public async Task InitializeAsync()
    {
        await _context.InitializeAsync();
    }

    public async Task SeedDebugDataAsync()
    {
        // Only seed if database is empty
        var existingSpecimens = await _specimenRepository.GetAllAsync();
        if (existingSpecimens.Any()) return;

        // Parse and insert debug specimens
        // (Migrate logic from ItemManager.LoadDebugSpecimens)
    }
}
```

---

## Phase 4: Missing Features Implementation

### 4.1 Employee Authentication

#### 4.1.1 Employee Entity

```csharp
// Data/Entities/EmployeeEntity.cs
[Table("Employees")]
public class EmployeeEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [MaxLength(100), Unique]
    public string Username { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
    
    // No password - username-only login per stakeholder requirements
    
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
```

#### 4.1.2 Employee Service (Simplified - No Password)

```csharp
// Services/Interfaces/IEmployeeService.cs
public interface IEmployeeService
{
    Task<Employee?> GetCurrentEmployeeAsync();
    Task<bool> LoginAsync(string username);
    Task LogoutAsync();
    Task<Employee> RegisterAsync(string username, string displayName, string email);
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    bool IsLoggedIn { get; }
}

// Services/EmployeeService.cs
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPreferences _preferences;
    private Employee? _currentEmployee;

    public bool IsLoggedIn => _currentEmployee != null;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IPreferences preferences)
    {
        _employeeRepository = employeeRepository;
        _preferences = preferences;
    }

    public async Task<bool> LoginAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;
            
        var employee = await _employeeRepository.GetByUsernameAsync(username);
        if (employee == null || !employee.IsActive)
            return false;

        employee.LastLoginAt = DateTime.UtcNow;
        await _employeeRepository.UpdateAsync(employee);

        _currentEmployee = MapToModel(employee);
        _preferences.Set("current_employee_id", employee.Id);
        
        return true;
    }

    public async Task<Employee?> GetCurrentEmployeeAsync()
    {
        if (_currentEmployee != null)
            return _currentEmployee;
            
        var employeeId = _preferences.Get("current_employee_id", 0);
        if (employeeId == 0)
            return null;
            
        var entity = await _employeeRepository.GetByIdAsync(employeeId);
        if (entity == null)
            return null;
            
        _currentEmployee = MapToModel(entity);
        return _currentEmployee;
    }

    public Task LogoutAsync()
    {
        _currentEmployee = null;
        _preferences.Remove("current_employee_id");
        return Task.CompletedTask;
    }

    public async Task<Employee> RegisterAsync(string username, string displayName, string email)
    {
        var existing = await _employeeRepository.GetByUsernameAsync(username);
        if (existing != null)
            throw new InvalidOperationException($"Username '{username}' is already taken.");

        var entity = new EmployeeEntity
        {
            Username = username,
            DisplayName = displayName,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _employeeRepository.AddAsync(entity);
        return MapToModel(entity);
    }

    private static Employee MapToModel(EmployeeEntity entity) => new()
    {
        Id = entity.Id,
        Username = entity.Username,
        DisplayName = entity.DisplayName,
        Email = entity.Email,
        LastLoginAt = entity.LastLoginAt
    };
}
```

#### 4.1.3 Login ViewModel (Username Only)

```csharp
// ViewModels/LoginViewModel.cs
public partial class LoginViewModel : BaseViewModel
{
    private readonly IEmployeeService _employeeService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    [ObservableProperty]
    private Employee? _selectedEmployee;

    public LoginViewModel(
        INavigationService navigationService,
        IEmployeeService employeeService) : base(navigationService)
    {
        _employeeService = employeeService;
        Title = "Employee Login";
    }

    [RelayCommand]
    private async Task LoadEmployeesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            Employees = new ObservableCollection<Employee>(employees);
        });
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        // Can login via username text or selected employee from picker
        var usernameToUse = SelectedEmployee?.Username ?? Username;
        
        if (string.IsNullOrWhiteSpace(usernameToUse))
        {
            ErrorMessage = "Please enter or select a username";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var success = await _employeeService.LoginAsync(usernameToUse);
            
            if (success)
            {
                await NavigationService.GoToAsync("//main");
            }
            else
            {
                ErrorMessage = "Employee not found or inactive";
            }
        });
    }

    [RelayCommand]
    private async Task RegisterNewEmployeeAsync()
    {
        await NavigationService.GoToAsync("register");
    }
}
```

### 4.2 Settings Management

#### 4.2.1 Settings Entity

```csharp
// Data/Entities/SettingsEntity.cs
[Table("Settings")]
public class SettingsEntity
{
    [PrimaryKey]
    public string Key { get; set; } = string.Empty;
    
    public string Value { get; set; } = string.Empty;
    
    public DateTime UpdatedAt { get; set; }
}
```

#### 4.2.2 Settings Service

```csharp
// Services/Interfaces/ISettingsService.cs
public interface ISettingsService
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value);
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value) where T : class;
    
    // Strongly-typed settings
    Task<bool> GetDarkModeAsync();
    Task SetDarkModeAsync(bool value);
    Task<string?> GetDefaultEmailAsync();
    Task SetDefaultEmailAsync(string email);
    Task<int> GetLoanDurationDaysAsync();
    Task SetLoanDurationDaysAsync(int days);
}
```

#### 4.2.3 Settings ViewModel

```csharp
// ViewModels/SettingsViewModel.cs
public partial class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private bool _darkModeEnabled;

    [ObservableProperty]
    private string _defaultEmail = string.Empty;

    [ObservableProperty]
    private int _defaultLoanDurationDays = 30;

    public SettingsViewModel(
        INavigationService navigationService,
        ISettingsService settingsService,
        IThemeService themeService) : base(navigationService)
    {
        _settingsService = settingsService;
        _themeService = themeService;
        Title = "Settings";
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        await ExecuteAsync(async () =>
        {
            DarkModeEnabled = await _settingsService.GetDarkModeAsync();
            DefaultEmail = await _settingsService.GetDefaultEmailAsync() ?? string.Empty;
            DefaultLoanDurationDays = await _settingsService.GetLoanDurationDaysAsync();
        });
    }

    partial void OnDarkModeEnabledChanged(bool value)
    {
        _themeService.SetTheme(value ? ThemeType.Dark : ThemeType.Light);
        _ = _settingsService.SetDarkModeAsync(value);
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        await ExecuteAsync(async () =>
        {
            await _settingsService.SetDefaultEmailAsync(DefaultEmail);
            await _settingsService.SetLoanDurationDaysAsync(DefaultLoanDurationDays);
        });
    }
}
```

### 4.3 Complete Loan Check-In

```csharp
// ViewModels/CheckInLoanViewModel.cs
public partial class CheckInLoanViewModel : BaseViewModel
{
    private readonly ILoanService _loanService;
    private readonly IItemService _itemService;

    [ObservableProperty]
    private string _scanStatus = "Scan a loan QR code to check in";

    [ObservableProperty]
    private bool _isScanning = true;

    [ObservableProperty]
    private LoanItem? _scannedLoan;

    public CheckInLoanViewModel(
        INavigationService navigationService,
        ILoanService loanService,
        IItemService itemService) : base(navigationService)
    {
        _loanService = loanService;
        _itemService = itemService;
        Title = "Check In Loan";
    }

    [RelayCommand]
    private async Task ProcessScanAsync(string qrCode)
    {
        await ExecuteAsync(async () =>
        {
            if (!qrCode.StartsWith("Loan:"))
            {
                ScanStatus = "Please scan a loan QR code";
                return;
            }

            var loan = await _loanService.GetLoanByQrIdAsync(qrCode);
            if (loan == null)
            {
                ScanStatus = "Loan not found";
                return;
            }

            if (loan.IsPresent)
            {
                ScanStatus = "This loan has already been checked in";
                return;
            }

            ScannedLoan = loan;
            IsScanning = false;
        });
    }

    [RelayCommand]
    private async Task ConfirmCheckInAsync()
    {
        if (ScannedLoan == null) return;

        await ExecuteAsync(async () =>
        {
            await _loanService.CheckInLoanAsync(ScannedLoan.Id);
            
            // Show success and navigate back
            await Shell.Current.DisplayAlert("Success", 
                $"Loan {ScannedLoan.Name} and all specimens have been checked in.", "OK");
            
            await NavigationService.GoBackAsync();
        });
    }

    [RelayCommand]
    private void CancelCheckIn()
    {
        ScannedLoan = null;
        IsScanning = true;
        ScanStatus = "Scan a loan QR code to check in";
    }
}
```

### 4.4 Due Date Picker

Add MAUI Community Toolkit for better date picker:

```csharp
// In FinalizeLoanViewModel
[ObservableProperty]
private DateTime _dueDate = DateTime.Today.AddDays(30);

[ObservableProperty]
private DateTime _minimumDueDate = DateTime.Today.AddDays(1);

[ObservableProperty]
private DateTime _maximumDueDate = DateTime.Today.AddYears(1);
```

```xml
<!-- In FinalizeLoanPage.xaml -->
<DatePicker 
    Date="{Binding DueDate}"
    MinimumDate="{Binding MinimumDueDate}"
    MaximumDate="{Binding MaximumDueDate}"
    Format="yyyy-MM-dd" />
```

### 4.5 Search Functionality

```csharp
// ViewModels/AllSpecimensViewModel.cs
public partial class AllSpecimensViewModel : BaseViewModel
{
    private readonly IItemService _itemService;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SpecimenItem> _specimens = new();

    [ObservableProperty]
    private bool _isRefreshing;

    public AllSpecimensViewModel(
        INavigationService navigationService,
        IItemService itemService) : base(navigationService)
    {
        _itemService = itemService;
        Title = "All Specimens";
    }

    [RelayCommand]
    private async Task LoadSpecimensAsync()
    {
        await ExecuteAsync(async () =>
        {
            var items = await _itemService.GetAllSpecimensAsync();
            Specimens = new ObservableCollection<SpecimenItem>(items);
        });
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadSpecimensAsync();
                return;
            }

            var items = await _itemService.SearchSpecimensAsync(SearchText);
            Specimens = new ObservableCollection<SpecimenItem>(items);
        });
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadSpecimensAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task ViewSpecimenAsync(SpecimenItem specimen)
    {
        await NavigationService.GoToAsync("viewitem", new Dictionary<string, object>
        {
            { "qrId", specimen.QrId }
        });
    }
}
```

### 4.6 Container Management (Full Implementation)

Containers represent physical storage locations (shelves, cabinets, rooms) that hold specimens.

#### 4.6.1 Container Entity

```csharp
// Data/Entities/ContainerEntity.cs
[Table("Containers")]
public class ContainerEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string? ArctosId { get; set; }
    
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;  // Physical location description
    
    public int? ParentContainerId { get; set; }  // For nested containers (shelf in a cabinet)
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Data/Entities/ContainerSpecimenEntity.cs (Junction table)
[Table("ContainerSpecimens")]
public class ContainerSpecimenEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Indexed]
    public int ContainerId { get; set; }
    
    [Indexed]
    public int SpecimenId { get; set; }
    
    public DateTime AddedAt { get; set; }
}
```

#### 4.6.2 Container Model

```csharp
// Models/ContainerItem.cs
public class ContainerItem : Item
{
    public ContainerItem()
    {
        Type = ItemType.Container;
    }

    public string Location { get; set; } = string.Empty;
    public int? ParentContainerId { get; set; }
    public ContainerItem? ParentContainer { get; set; }
    public List<ContainerItem> ChildContainers { get; set; } = new();
    public List<SpecimenItem> Specimens { get; set; } = new();
}
```

#### 4.6.3 Container Service

```csharp
// Services/Interfaces/IContainerService.cs
public interface IContainerService
{
    Task<ContainerItem?> GetContainerByIdAsync(int id);
    Task<ContainerItem?> GetContainerByQrIdAsync(string qrId);
    Task<IEnumerable<ContainerItem>> GetAllContainersAsync();
    Task<IEnumerable<ContainerItem>> GetRootContainersAsync();  // Top-level only
    Task<IEnumerable<ContainerItem>> GetChildContainersAsync(int parentId);
    Task<IEnumerable<SpecimenItem>> GetSpecimensInContainerAsync(int containerId);
    Task<ContainerItem> CreateContainerAsync(string name, string description, string location, int? parentId = null);
    Task UpdateContainerAsync(ContainerItem container);
    Task DeleteContainerAsync(int id);
    Task AddSpecimenToContainerAsync(int containerId, int specimenId);
    Task RemoveSpecimenFromContainerAsync(int containerId, int specimenId);
    Task MoveSpecimenAsync(int specimenId, int fromContainerId, int toContainerId);
}
```

#### 4.6.4 Container ViewModel

```csharp
// ViewModels/ContainerViewModel.cs
public partial class ContainerViewModel : BaseViewModel
{
    private readonly IContainerService _containerService;

    [ObservableProperty]
    private ObservableCollection<ContainerItem> _containers = new();

    [ObservableProperty]
    private ContainerItem? _selectedContainer;

    [ObservableProperty]
    private ObservableCollection<SpecimenItem> _specimensInContainer = new();

    public ContainerViewModel(
        INavigationService navigationService,
        IContainerService containerService) : base(navigationService)
    {
        _containerService = containerService;
        Title = "Containers";
    }

    [RelayCommand]
    private async Task LoadContainersAsync()
    {
        await ExecuteAsync(async () =>
        {
            var containers = await _containerService.GetRootContainersAsync();
            Containers = new ObservableCollection<ContainerItem>(containers);
        });
    }

    [RelayCommand]
    private async Task SelectContainerAsync(ContainerItem container)
    {
        SelectedContainer = container;
        await ExecuteAsync(async () =>
        {
            var specimens = await _containerService.GetSpecimensInContainerAsync(container.Id);
            SpecimensInContainer = new ObservableCollection<SpecimenItem>(specimens);
        });
    }

    [RelayCommand]
    private async Task AddSpecimenToContainerAsync(string qrCode)
    {
        if (SelectedContainer == null) return;
        
        await ExecuteAsync(async () =>
        {
            // Parse specimen ID from QR code
            if (!qrCode.StartsWith("Specimen:"))
            {
                ErrorMessage = "Please scan a specimen QR code";
                return;
            }

            var specimenId = int.Parse(qrCode.Replace("Specimen:", ""));
            await _containerService.AddSpecimenToContainerAsync(SelectedContainer.Id, specimenId);
            await SelectContainerAsync(SelectedContainer);  // Refresh
        });
    }

    [RelayCommand]
    private async Task CreateContainerAsync(string name, string description, string location)
    {
        await ExecuteAsync(async () =>
        {
            await _containerService.CreateContainerAsync(name, description, location, SelectedContainer?.Id);
            await LoadContainersAsync();
        });
    }
}
```

### 4.7 Offline Mode with Sync

The app must work without network connectivity and sync when connection is restored.

#### 4.7.1 Connectivity Service

```csharp
// Services/Interfaces/IConnectivityService.cs
public interface IConnectivityService
{
    bool IsConnected { get; }
    event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged;
    Task<bool> CheckConnectivityAsync();
}

// Services/ConnectivityService.cs
public class ConnectivityService : IConnectivityService
{
    public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;

    public ConnectivityService()
    {
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        ConnectivityChanged?.Invoke(this, e);
    }

    public Task<bool> CheckConnectivityAsync()
    {
        return Task.FromResult(IsConnected);
    }
}
```

#### 4.7.2 Offline Queue for Pending Operations

```csharp
// Data/Entities/PendingSyncEntity.cs
[Table("PendingSync")]
public class PendingSyncEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string OperationType { get; set; } = string.Empty;  // Create, Update, Delete
    public string EntityType { get; set; } = string.Empty;     // Specimen, Loan, Container
    public int EntityId { get; set; }
    public string JsonPayload { get; set; } = string.Empty;    // Serialized entity data
    public DateTime CreatedAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? LastError { get; set; }
}

// Services/Interfaces/ISyncService.cs
public interface ISyncService
{
    Task QueueOperationAsync(string operationType, string entityType, int entityId, object payload);
    Task<int> GetPendingOperationCountAsync();
    Task ProcessPendingOperationsAsync();
    event EventHandler<SyncProgressEventArgs> SyncProgress;
}

// Services/SyncService.cs
public class SyncService : ISyncService
{
    private readonly TagAndTrackDbContext _context;
    private readonly IConnectivityService _connectivityService;
    // Future: private readonly IArctosApiService _arctosApi;

    public event EventHandler<SyncProgressEventArgs>? SyncProgress;

    public SyncService(
        TagAndTrackDbContext context,
        IConnectivityService connectivityService)
    {
        _context = context;
        _connectivityService = connectivityService;
        
        // Auto-sync when connectivity is restored
        _connectivityService.ConnectivityChanged += async (s, e) =>
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
            {
                await ProcessPendingOperationsAsync();
            }
        };
    }

    public async Task QueueOperationAsync(string operationType, string entityType, int entityId, object payload)
    {
        var pendingOp = new PendingSyncEntity
        {
            OperationType = operationType,
            EntityType = entityType,
            EntityId = entityId,
            JsonPayload = JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Connection.InsertAsync(pendingOp);
    }

    public async Task<int> GetPendingOperationCountAsync()
    {
        return await _context.Connection.Table<PendingSyncEntity>().CountAsync();
    }

    public async Task ProcessPendingOperationsAsync()
    {
        if (!_connectivityService.IsConnected)
            return;

        var pendingOps = await _context.Connection
            .Table<PendingSyncEntity>()
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        int processed = 0;
        int total = pendingOps.Count;

        foreach (var op in pendingOps)
        {
            try
            {
                // Future: Send to Arctos API based on operation type
                // For now, just mark as processed (local-only mode)
                await _context.Connection.DeleteAsync(op);
                processed++;
                
                SyncProgress?.Invoke(this, new SyncProgressEventArgs(processed, total));
            }
            catch (Exception ex)
            {
                op.RetryCount++;
                op.LastError = ex.Message;
                await _context.Connection.UpdateAsync(op);
            }
        }
    }
}

public class SyncProgressEventArgs : EventArgs
{
    public int Processed { get; }
    public int Total { get; }
    public double PercentComplete => Total > 0 ? (double)Processed / Total * 100 : 0;

    public SyncProgressEventArgs(int processed, int total)
    {
        Processed = processed;
        Total = total;
    }
}
```

#### 4.7.3 Offline-Aware Base Service

```csharp
// Services/Base/OfflineAwareService.cs
public abstract class OfflineAwareService
{
    protected readonly ISyncService SyncService;
    protected readonly IConnectivityService ConnectivityService;

    protected OfflineAwareService(ISyncService syncService, IConnectivityService connectivityService)
    {
        SyncService = syncService;
        ConnectivityService = connectivityService;
    }

    protected async Task ExecuteWithSyncAsync(
        string operationType,
        string entityType,
        int entityId,
        object payload,
        Func<Task> localOperation)
    {
        // Always perform local operation first
        await localOperation();

        // Queue for sync if needed (future Arctos integration)
        if (!ConnectivityService.IsConnected)
        {
            await SyncService.QueueOperationAsync(operationType, entityType, entityId, payload);
        }
    }
}
```

### 4.8 Data Export (.xlsx/.csv)

Export loan history and specimen data to Excel or CSV format.

#### 4.8.1 Export Service

```csharp
// Services/Interfaces/IExportService.cs
public interface IExportService
{
    Task<string> ExportLoansToExcelAsync(IEnumerable<LoanItem> loans);
    Task<string> ExportLoansToCsvAsync(IEnumerable<LoanItem> loans);
    Task<string> ExportSpecimensToExcelAsync(IEnumerable<SpecimenItem> specimens);
    Task<string> ExportSpecimensToCsvAsync(IEnumerable<SpecimenItem> specimens);
    Task ShareFileAsync(string filePath);
}

// Services/ExportService.cs
public class ExportService : IExportService
{
    private readonly IShare _share;

    public ExportService(IShare share)
    {
        _share = share;
    }

    public async Task<string> ExportLoansToCsvAsync(IEnumerable<LoanItem> loans)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID,Name,Description,Borrower,Email,DateCheckedOut,DateDue,IsReturned,SpecimenCount");
        
        foreach (var loan in loans)
        {
            sb.AppendLine($"{loan.Id}," +
                $"\"{EscapeCsv(loan.Name)}\"," +
                $"\"{EscapeCsv(loan.Description)}\"," +
                $"\"{EscapeCsv(loan.Borrower)}\"," +
                $"\"{EscapeCsv(loan.BorrowerEmail)}\"," +
                $"{loan.DateCheckedOut:yyyy-MM-dd}," +
                $"{loan.DateDue:yyyy-MM-dd}," +
                $"{loan.IsPresent}," +
                $"{loan.Specimens.Count}");
        }

        var fileName = $"loans_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllTextAsync(filePath, sb.ToString());
        
        return filePath;
    }

    public async Task<string> ExportLoansToExcelAsync(IEnumerable<LoanItem> loans)
    {
        // Using ClosedXML or similar library
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Loans");
        
        // Headers
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Description";
        worksheet.Cell(1, 4).Value = "Borrower";
        worksheet.Cell(1, 5).Value = "Email";
        worksheet.Cell(1, 6).Value = "Date Checked Out";
        worksheet.Cell(1, 7).Value = "Date Due";
        worksheet.Cell(1, 8).Value = "Returned";
        worksheet.Cell(1, 9).Value = "Specimen Count";
        
        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        foreach (var loan in loans)
        {
            worksheet.Cell(row, 1).Value = loan.Id;
            worksheet.Cell(row, 2).Value = loan.Name;
            worksheet.Cell(row, 3).Value = loan.Description;
            worksheet.Cell(row, 4).Value = loan.Borrower;
            worksheet.Cell(row, 5).Value = loan.BorrowerEmail;
            worksheet.Cell(row, 6).Value = loan.DateCheckedOut;
            worksheet.Cell(row, 7).Value = loan.DateDue;
            worksheet.Cell(row, 8).Value = loan.IsPresent ? "Yes" : "No";
            worksheet.Cell(row, 9).Value = loan.Specimens.Count;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        var fileName = $"loans_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        workbook.SaveAs(filePath);
        
        return filePath;
    }

    public async Task<string> ExportSpecimensToCsvAsync(IEnumerable<SpecimenItem> specimens)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID,ArctosID,Name,Description,IsPresent,QRCode");
        
        foreach (var specimen in specimens)
        {
            sb.AppendLine($"{specimen.Id}," +
                $"\"{EscapeCsv(specimen.ArctosId)}\"," +
                $"\"{EscapeCsv(specimen.Name)}\"," +
                $"\"{EscapeCsv(specimen.Description)}\"," +
                $"{specimen.IsPresent}," +
                $"{specimen.QrId}");
        }

        var fileName = $"specimens_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllTextAsync(filePath, sb.ToString());
        
        return filePath;
    }

    public async Task<string> ExportSpecimensToExcelAsync(IEnumerable<SpecimenItem> specimens)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Specimens");
        
        // Headers
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Arctos ID";
        worksheet.Cell(1, 3).Value = "Name";
        worksheet.Cell(1, 4).Value = "Description";
        worksheet.Cell(1, 5).Value = "Present";
        worksheet.Cell(1, 6).Value = "QR Code";
        
        var headerRange = worksheet.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        foreach (var specimen in specimens)
        {
            worksheet.Cell(row, 1).Value = specimen.Id;
            worksheet.Cell(row, 2).Value = specimen.ArctosId;
            worksheet.Cell(row, 3).Value = specimen.Name;
            worksheet.Cell(row, 4).Value = specimen.Description;
            worksheet.Cell(row, 5).Value = specimen.IsPresent ? "Yes" : "No";
            worksheet.Cell(row, 6).Value = specimen.QrId;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        var fileName = $"specimens_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        workbook.SaveAs(filePath);
        
        return filePath;
    }

    public async Task ShareFileAsync(string filePath)
    {
        await _share.RequestAsync(new ShareFileRequest
        {
            Title = "Export Data",
            File = new ShareFile(filePath)
        });
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"");
    }
}
```

#### 4.8.2 Export ViewModel Integration

```csharp
// Add to LoanHistoryViewModel
[RelayCommand]
private async Task ExportLoansAsync()
{
    var exportFormat = await Shell.Current.DisplayActionSheet(
        "Export Format", "Cancel", null, "Excel (.xlsx)", "CSV (.csv)");
    
    if (exportFormat == "Cancel" || string.IsNullOrEmpty(exportFormat))
        return;

    await ExecuteAsync(async () =>
    {
        var loans = await _loanService.GetAllLoansAsync();
        string filePath;
        
        if (exportFormat.Contains("xlsx"))
            filePath = await _exportService.ExportLoansToExcelAsync(loans);
        else
            filePath = await _exportService.ExportLoansToCsvAsync(loans);

        await _exportService.ShareFileAsync(filePath);
    });
}
```

---

## Phase 5: Unit Testing Framework

### 5.1 Test Project Setup

Create new test project: `TagAndTrack.Tests`

```xml
<!-- TagAndTrack.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <IsPackable>false</IsPackable>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
        <PackageReference Include="NUnit" Version="4.0.1" />
        <PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
        <PackageReference Include="NUnit.Analyzers" Version="4.0.1" />
        <PackageReference Include="Moq" Version="4.20.70" />
        <PackageReference Include="FluentAssertions" Version="6.12.0" />
        <PackageReference Include="coverlet.collector" Version="6.0.0" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\TagAndTrack\TagAndTrack.csproj" />
    </ItemGroup>
</Project>
```

### 5.2 Test Structure

```
TagAndTrack.Tests/
├── Unit/
│   ├── Models/
│   │   ├── ItemTests.cs
│   │   ├── LoanItemTests.cs
│   │   └── SpecimenItemTests.cs
│   ├── Services/
│   │   ├── ItemServiceTests.cs
│   │   ├── LoanServiceTests.cs
│   │   ├── AuthenticationServiceTests.cs
│   │   └── SettingsServiceTests.cs
│   ├── ViewModels/
│   │   ├── ScanItemViewModelTests.cs
│   │   ├── StartLoanViewModelTests.cs
│   │   ├── LoginViewModelTests.cs
│   │   └── AllSpecimensViewModelTests.cs
│   └── Repositories/
│       ├── SpecimenRepositoryTests.cs
│       └── LoanRepositoryTests.cs
├── Integration/
│   ├── DatabaseTests.cs
│   └── NavigationTests.cs
└── TestHelpers/
    ├── MockFactory.cs
    └── TestDataBuilder.cs
```

### 5.3 Example Tests

```csharp
// Unit/ViewModels/ScanItemViewModelTests.cs
[TestFixture]
public class ScanItemViewModelTests
{
    private Mock<INavigationService> _navigationServiceMock;
    private Mock<IItemService> _itemServiceMock;
    private ScanItemViewModel _viewModel;

    [SetUp]
    public void Setup()
    {
        _navigationServiceMock = new Mock<INavigationService>();
        _itemServiceMock = new Mock<IItemService>();
        _viewModel = new ScanItemViewModel(
            _navigationServiceMock.Object,
            _itemServiceMock.Object);
    }

    [Test]
    public async Task ProcessScan_WithValidQrCode_NavigatesToViewItem()
    {
        // Arrange
        var qrCode = "Specimen:123";
        var specimen = new SpecimenItem { Id = 123, Name = "Test Specimen" };
        _itemServiceMock.Setup(x => x.GetItemByQrIdAsync(qrCode))
            .ReturnsAsync(specimen);

        // Act
        await _viewModel.ProcessScanCommand.ExecuteAsync(qrCode);

        // Assert
        _navigationServiceMock.Verify(x => x.GoToAsync(
            "viewitem", 
            It.Is<IDictionary<string, object>>(d => d["qrId"].ToString() == qrCode)), 
            Times.Once);
    }

    [Test]
    public async Task ProcessScan_WithInvalidQrCode_ShowsError()
    {
        // Arrange
        var qrCode = "Invalid:999";
        _itemServiceMock.Setup(x => x.GetItemByQrIdAsync(qrCode))
            .ReturnsAsync((SpecimenItem?)null);

        // Act
        await _viewModel.ProcessScanCommand.ExecuteAsync(qrCode);

        // Assert
        _viewModel.ScanStatus.Should().Contain("not recognized");
        _navigationServiceMock.Verify(x => x.GoToAsync(It.IsAny<string>(), 
            It.IsAny<IDictionary<string, object>>()), Times.Never);
    }

    [Test]
    public void ProcessScan_WhenBusy_DoesNothing()
    {
        // Arrange
        _viewModel.IsBusy = true;

        // Act
        _viewModel.ProcessScanCommand.Execute("Specimen:123");

        // Assert
        _itemServiceMock.Verify(x => x.GetItemByQrIdAsync(It.IsAny<string>()), Times.Never);
    }
}

// Unit/Services/LoanServiceTests.cs
[TestFixture]
public class LoanServiceTests
{
    private Mock<ILoanRepository> _loanRepositoryMock;
    private Mock<ISpecimenRepository> _specimenRepositoryMock;
    private LoanService _service;

    [SetUp]
    public void Setup()
    {
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _specimenRepositoryMock = new Mock<ISpecimenRepository>();
        _service = new LoanService(_loanRepositoryMock.Object, _specimenRepositoryMock.Object);
    }

    [Test]
    public async Task CreateLoan_WithValidData_ReturnsLoanWithId()
    {
        // Arrange
        var specimens = new List<SpecimenItem>
        {
            new() { Id = 1, Name = "Specimen 1", IsPresent = true },
            new() { Id = 2, Name = "Specimen 2", IsPresent = true }
        };
        
        _loanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<LoanEntity>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateLoanAsync(
            "Test Loan", 
            "Description", 
            "John Doe", 
            "john@example.com",
            DateTime.Today.AddDays(30),
            specimens);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Loan");
        _specimenRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SpecimenEntity>()), 
            Times.Exactly(2));
    }

    [Test]
    public async Task CreateLoan_WithCheckedOutSpecimen_ThrowsException()
    {
        // Arrange
        var specimens = new List<SpecimenItem>
        {
            new() { Id = 1, Name = "Specimen 1", IsPresent = false } // Already checked out
        };

        // Act & Assert
        var act = async () => await _service.CreateLoanAsync(
            "Test Loan", "Description", "John Doe", "john@example.com",
            DateTime.Today.AddDays(30), specimens);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already checked out*");
    }
}
```

### 5.4 Test Coverage Goals

| Component | Target Coverage |
|-----------|-----------------|
| ViewModels | 90% |
| Services | 95% |
| Repositories | 80% |
| Models | 100% |
| Overall | 85% |

---

## Phase 6: Performance Optimization

### 6.1 DataTable Virtualization

Replace `DataTableTemplate` grid approach with `CollectionView` virtualization:

```csharp
// Components/VirtualizedDataTable.cs
public class VirtualizedDataTable<T> : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<T>), 
            typeof(VirtualizedDataTable<T>));

    public IEnumerable<T> ItemsSource
    {
        get => (IEnumerable<T>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private readonly CollectionView _collectionView;

    public VirtualizedDataTable()
    {
        _collectionView = new CollectionView
        {
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = 1
            },
            SelectionMode = SelectionMode.None,
            // Enable recycling
            ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView
        };

        _collectionView.SetBinding(CollectionView.ItemsSourceProperty, 
            new Binding(nameof(ItemsSource), source: this));

        Content = _collectionView;
    }
}
```

### 6.2 Async Data Loading

Ensure all data operations are properly async:

```csharp
// Services/ItemService.cs
public class ItemService : IItemService
{
    private readonly ISpecimenRepository _specimenRepository;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    public async Task<IEnumerable<SpecimenItem>> GetAllSpecimensAsync(
        CancellationToken cancellationToken = default)
    {
        await _loadLock.WaitAsync(cancellationToken);
        try
        {
            var entities = await _specimenRepository.GetAllAsync();
            return entities.Select(MapToModel);
        }
        finally
        {
            _loadLock.Release();
        }
    }
}
```

### 6.3 Image/QR Code Caching

```csharp
// Components/CachedQrCodeView.cs
public class CachedQrCodeView : Image
{
    private static readonly Dictionary<string, ImageSource> _cache = new();
    private static readonly object _cacheLock = new();

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(CachedQrCodeView),
            propertyChanged: OnValueChanged);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not CachedQrCodeView view || newValue is not string value)
            return;

        lock (_cacheLock)
        {
            if (_cache.TryGetValue(value, out var cached))
            {
                view.Source = cached;
                return;
            }
        }

        // Generate QR code asynchronously
        Task.Run(() =>
        {
            var qrCode = GenerateQrCode(value);
            lock (_cacheLock)
            {
                _cache[value] = qrCode;
            }
            MainThread.BeginInvokeOnMainThread(() => view.Source = qrCode);
        });
    }

    private static ImageSource GenerateQrCode(string value)
    {
        // Use ZXing to generate
        var writer = new BarcodeWriter<SKBitmap>
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Width = 220,
                Height = 220,
                Margin = 4
            }
        };
        
        var bitmap = writer.Write(value);
        // Convert to ImageSource
        return ImageSource.FromStream(() => bitmap.Encode(SKEncodedImageFormat.Png, 100).AsStream());
    }
}
```

### 6.4 Database Query Optimization

Add indexes to frequently queried columns:

```csharp
// Ensure indexes exist
await _connection.ExecuteAsync(@"
    CREATE INDEX IF NOT EXISTS idx_specimens_arctos ON Specimens(ArctosId);
    CREATE INDEX IF NOT EXISTS idx_specimens_status ON Specimens(IsPresent);
    CREATE INDEX IF NOT EXISTS idx_loans_status ON Loans(IsReturned);
    CREATE INDEX IF NOT EXISTS idx_loan_specimens_loan ON LoanSpecimens(LoanId);
");
```

---

## Project Structure Reorganization

### Solution Structure (Target)

```
Tag-and-Track/
├── src/
│   ├── TagAndTrack/                    # Main MAUI app
│   │   ├── TagAndTrack.csproj
│   │   └── ...
│   └── TagAndTrack.Core/               # Shared business logic (optional)
│       ├── TagAndTrack.Core.csproj
│       ├── Models/
│       ├── Services/
│       └── Interfaces/
├── tests/
│   ├── TagAndTrack.Tests/              # Unit tests
│   │   └── TagAndTrack.Tests.csproj
│   └── TagAndTrack.IntegrationTests/   # Integration tests
│       └── TagAndTrack.IntegrationTests.csproj
├── docs/
│   ├── RefactoringPlan.md
│   ├── API.md
│   └── Architecture.md
├── .editorconfig
├── .gitignore
├── Directory.Build.props               # Shared MSBuild properties
├── TagAndTrack.sln
└── README.md
```

---

## Implementation Timeline

| Phase | Duration | Start | End | Dependencies |
|-------|----------|-------|-----|--------------|
| **Phase 1:** Code Cleanup | 2-3 weeks | Week 1 | Week 3 | None |
| **Phase 2:** MVVM Architecture | 3-4 weeks | Week 3 | Week 7 | Phase 1 |
| **Phase 3:** Database Layer | 2-3 weeks | Week 6 | Week 9 | Phase 2 (partial) |
| **Phase 4:** Missing Features | 4-5 weeks | Week 9 | Week 14 | Phase 3 |
| **Phase 5:** Unit Testing | 2 weeks | Week 12 | Week 14 | Phase 2-3 |
| **Phase 6:** Performance | 1-2 weeks | Week 14 | Week 16 | Phase 4-5 |

**Total Estimated Duration:** 14-18 weeks

### Milestone Checkpoints

1. **Week 3:** Memory leaks fixed, critical bugs resolved
2. **Week 7:** MVVM architecture in place, DI configured
3. **Week 9:** SQLite database operational, CSV system removed
4. **Week 11:** Employee login, settings, and containers complete
5. **Week 14:** Offline mode, data export, all features complete, tests passing
6. **Week 16:** Performance optimized, ready for production testing

---

## Design Decisions (Resolved)

Based on stakeholder feedback, the following decisions have been finalized:

### Authentication
- **Username-only login** - No password required
- Simple employee picker or text entry for username
- No roles/permissions - all employees have equal access
- No MFA or external identity providers

### Data & Storage
- **SQLite** is the final database - no server-based migration planned
- **Loan history retained indefinitely** - no archive/purge mechanism
- **Data export required** - support both .xlsx and .csv formats
- **Offline mode required** - app must function without network connectivity
  - Sync strategy: Queue operations locally, process when connectivity restored
  - Conflict resolution: Last-write-wins with timestamp comparison

### Arctos Integration
- **Deferred** - plan architecture to support it, but assume local-only DB for now
- Will add API service interface that can be implemented later
- Sync service already queues operations for future remote sync

### Container Feature
- **Full implementation required** - not low priority
- Containers represent physical storage locations (shelves, cabinets, rooms)
- Containers can contain specimens
- Support nested containers (shelf within a cabinet)

### Email Configuration
- **Admin-only configuration** - not exposed to regular users
- Simple, modifiable templates (HTML-based for future customization)
- Single provider at a time (Gmail default, configurable)
- Credentials stored securely using MAUI SecureStorage

### Platform
- **Primary target: iPadOS**
- Secondary: macOS Catalyst, Windows
- **No Android support**
- Deployment via direct installation (not App Store)

### Testing
- Unit tests and integration tests only
- **No automated UI tests** - functional tests sufficient
- **No CI/CD pipeline** - manual test runs
- Target 85% code coverage on services and ViewModels

---

## Appendix A: Package Dependencies (Target)

```xml
<ItemGroup>
    <!-- Core MAUI -->
    <PackageReference Include="Microsoft.Maui.Controls" Version="$(MauiVersion)" />
    <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="8.0.1" />
    
    <!-- MVVM & DI -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="CommunityToolkit.Maui" Version="7.0.0" />
    
    <!-- Database -->
    <PackageReference Include="sqlite-net-pcl" Version="1.9.172" />
    <PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.8" />
    
    <!-- QR Code -->
    <PackageReference Include="ZXing.Net.Maui" Version="0.4.0" />
    <PackageReference Include="ZXing.Net.Maui.Controls" Version="0.4.0" />
    
    <!-- Data Export -->
    <PackageReference Include="ClosedXML" Version="0.102.2" />  <!-- Excel export -->
    
    <!-- Analysis -->
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" />
</ItemGroup>
```

---

## Appendix B: Git Branch Strategy

```
main                    # Production-ready code
├── develop             # Integration branch
│   ├── feature/phase1-cleanup
│   ├── feature/phase2-mvvm
│   ├── feature/phase3-database
│   ├── feature/phase4-auth
│   ├── feature/phase4-settings
│   ├── feature/phase5-testing
│   └── feature/phase6-performance
└── release/v2.0        # Release candidate
```

---

*Document Version: 2.1*  
*Last Updated: January 23, 2026*  
*Status: Stakeholder Review Complete - Ready for Implementation*
