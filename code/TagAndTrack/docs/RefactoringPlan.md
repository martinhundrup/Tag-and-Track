# Tag and Track - Comprehensive Refactoring Plan

**Date:** January 23, 2026  
**Author:** GitHub Copilot  
**Project:** Tag and Track (.NET MAUI)  
**Target Framework:** .NET 8

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Current State Analysis](#2-current-state-analysis)
3. [Refactoring Phases](#3-refactoring-phases)
4. [Phase 1: Code Cleanup & Standards](#phase-1-code-cleanup--standards)
5. [Phase 2: Architecture & MVVM Implementation](#phase-2-architecture--mvvm-implementation)
6. [Phase 3: Database & Data Layer](#phase-3-database--data-layer)
7. [Phase 4: Missing Features](#phase-4-missing-features)
8. [Phase 5: Unit Testing Framework](#phase-5-unit-testing-framework)
9. [Phase 6: Performance Optimization](#phase-6-performance-optimization)
10. [Implementation Timeline](#implementation-timeline)
11. [Questions & Assumptions](#questions--assumptions)

---

## 1. Executive Summary

This document outlines a comprehensive refactoring plan for the Tag and Track .NET MAUI application. The goal is to transform the current codebase into a maintainable, testable, and production-ready application following modern .NET MAUI best practices.

### Key Goals
- Clean up code and enforce consistent coding standards
- Implement MVVM architecture for separation of concerns
- Create a proper data layer with repository pattern
- Complete missing features (database, authentication, settings)
- Add comprehensive unit testing with NUnit
- Optimize performance throughout the application

---

## 2. Current State Analysis

### 2.1 Architecture Issues

| Issue | Severity | Location |
|-------|----------|----------|
| No MVVM pattern | High | All pages |
| Tight coupling between UI and business logic | High | Pages, Components |
| Static classes for state management | Medium | `ItemManager`, `LoanCreator`, `EmployeeManager` |
| Global mutable state | High | `ScannedQRItem.lastScannedItem` |
| Reflection abuse for property setting | Medium | `ItemManager` |

### 2.2 Code Quality Issues

| Issue | Severity | Files Affected |
|-------|----------|----------------|
| Event handler memory leaks (never unsubscribed) | High | `DataTableTemplate`, all pages |
| Duplicate theme subscription code | Medium | Every page and component |
| Missing null checks/guards | Medium | Throughout |
| Inconsistent naming conventions | Low | `spceimen` typo, mixed styles |
| Non-nullable fields not initialized | Medium | `scanResultLabel` warnings |

### 2.3 Threading Issues

| Issue | Severity | Description |
|-------|----------|-------------|
| UI thread access violations | Critical | Crash on iOS in `ScanCaptured` handlers |
| Inconsistent `MainThread.InvokeOnMainThreadAsync` usage | High | Some handlers missing dispatch |

### 2.4 Missing Features

| Feature | Status | Priority |
|---------|--------|----------|
| Proper SQLite database integration | Partially implemented, not used | High |
| Employee registration/authentication | Stub only | High |
| Settings page functionality | Placeholder | Medium |
| Container management | Model exists, no UI | Medium |
| Search functionality | UI only, no implementation | Medium |
| Due date picker for loans | TODO comment | Medium |

### 2.5 Existing Database State

There are **two parallel data systems** that need to be unified:
- `TagAndTrack.Databases.Database` (SQLite, unused)
- `TagAndTrack.Backend.Items.ItemManager` (in-memory, CSV-based debug data)

---

## 3. Refactoring Phases

---

## Phase 1: Code Cleanup & Standards

### 1.1 Fix Critical Bugs

**Priority: Immediate**

#### Thread Safety in Scan Handlers

All `ScanCaptured` handlers must be thread-safe:

```csharp
// Example refactor for thread safety
private object _scanLock = new object();

public void OnScanCaptured(ScanResult result)
{
    lock (_scanLock)
    {
        // Handle scan result
    }
}
```

#### Memory Leak Fixes

- Ensure all events are unsubscribed properly, especially in `DataTableTemplate` and page `OnDisappearing` methods.
- Centralize theme subscription in `AppShell.xaml` and remove duplicates from pages/components.

#### Null Reference Fixes

- Introduce null checks and guards, especially in data handling and UI update methods.
- Use null-coalescing operators and nullable reference types where appropriate.

### 1.2 Code Cleanup

- Remove unused usings, fields, and methods.
- Consolidate similar methods and remove duplicates.
- Format code according to .NET MAUI standards (e.g., proper XML namespaces in XAML files).

### 1.3 Standards Enforcement

- Enforce consistent naming conventions (e.g., `spceimen` to `specimen`).
- Ensure all public members have appropriate access modifiers.
- Organize files and folders to follow the recommended .NET MAUI project structure.

### 1.4 Tooling Setup

- Configure Roslynator and .NET MAUI Community Toolkit analyzers for the solution.
- Set up CodeMaid or a similar tool for code cleanup and reformatting.

---

## Phase 2: Architecture & MVVM Implementation

### 2.1 Adopt MVVM Pattern

**Priority: High**

- Introduce `ICommand` implementations for all UI actions in the ViewModels.
- Bind UI components directly to ViewModel properties, removing code-behind logic.
- Use `ObservableCollection<T>` for collections bound to the UI.

### 2.2 Separate Business Logic from UI

- Move all business logic from code-behind and static classes to appropriate ViewModel or service classes.
- Introduce services for data access, navigation, and other cross-cutting concerns.

### 2.3 Dependency Injection

- Configure dependency injection for all services and repositories in the `MauiProgram.cs` file.
- Use constructor injection for all ViewModels and services.

---

## Phase 3: Database & Data Layer

### 3.1 Unified Data Model

Create a clean separation between database entities and domain models:

- Introduce separate project or folder for database entities (e.g., `TagAndTrack.Databases.Entities`).
- Map database entities to domain models using a mapping library (e.g., AutoMapper) or manual mapping.

### 3.2 Implement Repository Pattern

**Priority: High**

- Create interfaces for all repositories (e.g., `IItemRepository`, `IUserRepository`).
- Implement repositories using `Dapper` or `Entity Framework Core` for database access.
- Ensure all data access code is asynchronous.

### 3.3 SQLite Integration

- Finalize the integration of SQLite for local data storage.
- Migrate any existing data from the CSV-based system to SQLite.

### 3.4 Data Seeding and Migration

- Implement data seeding for initial app setup (e.g., default settings, admin user).
- Configure automatic migrations on app start, with the ability to revert to a previous version.

---

## Phase 4: Missing Features

### 4.1 Database Features

**Priority: High**

- Implement complete `Item`, `User`, and `Settings` models with all properties and relationships.
- Create database migrations to establish the initial schema.

### 4.2 Authentication Features

- Implement user registration, login, and logout functionality.
- Secure sensitive data (e.g., passwords) using modern hashing algorithms.

### 4.3 Settings Management

- Create a settings service to handle reading and writing app settings.
- Implement a settings page in the UI for user configuration.

### 4.4 Container and Search Features

- Develop UI and logic for container management and search functionality.
- Implement due date picker using a third-party calendar control if needed.

---

## Phase 5: Unit Testing Framework

### 5.1 Testing Strategy

**Priority: High**

- Use NUnit as the testing framework for all unit tests.
- Aim for 100% code coverage on all critical and high-severity parts of the application.

### 5.2 Test Automation

- Configure CI/CD pipelines to run tests automatically on each commit and pull request.
- Deploy test reports to a shared location for review.

---

## Phase 6: Performance Optimization

### 6.1 Identify Performance Bottlenecks

**Priority: Medium**

- Use diagnostics tools to identify slow-running queries, memory leaks, and other performance issues.
- Review and optimize the use of async/await, especially in data access and network calls.

---

## Implementation Timeline

| Phase | Start Date | End Date | Dependencies |
|-------|------------|----------|---------------|
| 1. Code Cleanup & Standards | Feb 1, 2026 | Feb 28, 2026 | None |
| 2. Architecture & MVVM Implementation | Mar 1, 2026 | Mar 31, 2026 | Phase 1 |
| 3. Database & Data Layer | Apr 1, 2026 | Apr 30, 2026 | Phase 2 |
| 4. Missing Features | May 1, 2026 | Jun 15, 2026 | Phase 3 |
| 5. Unit Testing Framework | Jun 16, 2026 | Jun 30, 2026 | Phase 4 |
| 6. Performance Optimization | Jul 1, 2026 | Jul 15, 2026 | Phase 5 |

---

## Questions & Assumptions

- **QA Resources:** TM
- **Dev Resources:** TM
- **PO Resources:** TM

### 2.2 Base ViewModel Implementation
