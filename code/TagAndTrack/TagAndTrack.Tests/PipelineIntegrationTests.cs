using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Data.Entities;
using TagAndTrack.Backend.Utils;

namespace TagAndTrack.Tests;

/// <summary>
/// Full round-trip pipeline test: builds entities → exports to CSV → imports from CSV
/// → reads back from a real temp SQLite DB → asserts all data matches original.
/// Uses a temp directory that is deleted on dispose; no side effects on production DB.
/// </summary>
public class PipelineIntegrationTests : IAsyncLifetime
{
    private string _tempDir = "";
    private string _dbPath = "";
    private string _csvPath = "";

    // ===== Setup / Teardown =====

    public async Task InitializeAsync()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"TagAndTrack_Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _dbPath = Path.Combine(_tempDir, "test.db3");
        _csvPath = Path.Combine(_tempDir, "export.csv");
        await DbService.InitForTestAsync(_dbPath);
    }

    public async Task DisposeAsync()
    {
        await DbService.DisposeForTestAsync();
        try { Directory.Delete(_tempDir, true); } catch { /* best-effort cleanup */ }
    }

    // ===== Tests =====

    [Fact]
    public async Task FullPipeline_ExportThenImport_AllEntitiesRoundTrip()
    {
        // ── ARRANGE: define source entities with explicit IDs ──────────────────

        var specimens = new List<SpecimenEntity>
        {
            new() { Id = 1, ArctosId = "ARC-0001", Name = "Salmon Skull",   Description = "Large cranium, cleaned",           IsPresent = true  },
            new() { Id = 2, ArctosId = "ARC-0002", Name = "Bear Femur",     Description = "Right femur, adult male",           IsPresent = false },
            new() { Id = 3, ArctosId = "ARC-0003", Name = "Eagle Feather",  Description = "Primary flight feather",            IsPresent = true  },
            new() { Id = 4, ArctosId = "ARC-0004", Name = "Fox Pelt",       Description = "Full pelt, tanned, with \"tags\"",  IsPresent = true  },
            new() { Id = 5, ArctosId = "ARC-0005", Name = "Owl Wing",       Description = "Left wing, dried",                 IsPresent = false },
            new() { Id = 6, ArctosId = "ARC-0006", Name = "Wolf Skull",     Description = "",                                  IsPresent = true  },
        };

        var loans = new List<LoanEntity>
        {
            new()
            {
                Id = 1, ArctosId = "LOAN-0001", Name = "Loan Alpha",
                Description = "First research loan",
                Borrower = "University A", Email = "a@univ.edu",
                DateCheckedOut = new DateTime(2025, 1, 10),
                DateDue        = new DateTime(2025, 6, 10),
                IsReturned     = false,
                SpecimenIds    = "1,2,3",
                SignatureData  = null
            },
            new()
            {
                Id = 2, ArctosId = "LOAN-0002", Name = "Loan Beta",
                Description = "Returned loan, has signature",
                Borrower = "Museum B", Email = "b@museum.org",
                DateCheckedOut = new DateTime(2025, 2,  1),
                DateDue        = new DateTime(2025, 8,  1),
                IsReturned     = true,
                SpecimenIds    = "4,5",
                SignatureData  = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F } // "Hello"
            },
        };

        var loanSpecimens = new List<LoanSpecimenEntity>
        {
            new() { Id = 1, LoanId = 1, SpecimenId = 1, ReturnDate = null },
            new() { Id = 2, LoanId = 1, SpecimenId = 2, ReturnDate = null },
            new() { Id = 3, LoanId = 1, SpecimenId = 3, ReturnDate = null },
            new() { Id = 4, LoanId = 2, SpecimenId = 4, ReturnDate = new DateTime(2025, 7, 15) },
            new() { Id = 5, LoanId = 2, SpecimenId = 5, ReturnDate = new DateTime(2025, 7, 16) },
        };

        var containers = new List<ContainerEntity>
        {
            new() { Id = 1, Name = "Cabinet A",   Description = "Shelf A, row 1",   SpecimenIds = "1,2"   },
            new() { Id = 2, Name = "Cabinet B",   Description = "Overflow storage", SpecimenIds = "3,4,5" },
            new() { Id = 3, Name = "Empty Shelf", Description = "",                 SpecimenIds = ""      },
        };

        var employees = new List<EmployeeEntity>
        {
            new() { Id = 1, Name = "Alice Smith", LastLogin = new DateTime(2025, 3, 1, 12, 0, 0) },
            new() { Id = 2, Name = "Bob Jones",   LastLogin = null },
        };

        // ── ACT: build CSV ─────────────────────────────────────────────────────

        var csv = CsvService.BuildExportCsv(specimens, loans, loanSpecimens, containers, employees);

        // Verify CSV was generated
        Assert.False(string.IsNullOrWhiteSpace(csv), "BuildExportCsv returned empty string");

        // Write to temp file and import into the test database
        await File.WriteAllTextAsync(_csvPath, csv);
        await CsvService.ImportDatabaseAsync(_csvPath);

        // ── ASSERT: read back all entity types and compare ─────────────────────

        var importedSpecimens     = await DbService.GetAllSpecimenEntitiesAsync();
        var importedLoans         = await DbService.GetAllLoanEntitiesAsync();
        var importedLoanSpecimens = await DbService.GetAllLoanSpecimenEntitiesAsync();
        var importedContainers    = await DbService.GetAllContainerEntitiesAsync();
        var importedEmployees     = await DbService.GetAllEmployeeEntitiesAsync();

        // --- Specimens ---
        Assert.Equal(specimens.Count, importedSpecimens.Count);
        foreach (var orig in specimens)
        {
            var imp = importedSpecimens.Single(s => s.Id == orig.Id);
            Assert.Equal(orig.ArctosId,   imp.ArctosId);
            Assert.Equal(orig.Name,        imp.Name);
            Assert.Equal(orig.Description, imp.Description);
            Assert.Equal(orig.IsPresent,   imp.IsPresent);
        }

        // --- Loans ---
        Assert.Equal(loans.Count, importedLoans.Count);
        foreach (var orig in loans)
        {
            var imp = importedLoans.Single(l => l.Id == orig.Id);
            Assert.Equal(orig.ArctosId,    imp.ArctosId);
            Assert.Equal(orig.Name,         imp.Name);
            Assert.Equal(orig.Description,  imp.Description);
            Assert.Equal(orig.Borrower,     imp.Borrower);
            Assert.Equal(orig.Email,        imp.Email);
            Assert.Equal(orig.IsReturned,   imp.IsReturned);
            Assert.Equal(orig.SpecimenIds,  imp.SpecimenIds);
            Assert.Equal(orig.DateCheckedOut, imp.DateCheckedOut, TimeSpan.FromSeconds(1));
            Assert.Equal(orig.DateDue,        imp.DateDue,        TimeSpan.FromSeconds(1));
            if (orig.SignatureData == null)
                Assert.Null(imp.SignatureData);
            else
                Assert.Equal(orig.SignatureData, imp.SignatureData);
        }

        // --- LoanSpecimen join rows ---
        Assert.Equal(loanSpecimens.Count, importedLoanSpecimens.Count);
        foreach (var orig in loanSpecimens)
        {
            var imp = importedLoanSpecimens.Single(ls => ls.Id == orig.Id);
            Assert.Equal(orig.LoanId,     imp.LoanId);
            Assert.Equal(orig.SpecimenId, imp.SpecimenId);
            Assert.Equal(orig.ReturnDate.HasValue, imp.ReturnDate.HasValue);
            if (orig.ReturnDate.HasValue)
                Assert.Equal(orig.ReturnDate.Value, imp.ReturnDate!.Value, TimeSpan.FromSeconds(1));
        }

        // --- Containers ---
        Assert.Equal(containers.Count, importedContainers.Count);
        foreach (var orig in containers)
        {
            var imp = importedContainers.Single(c => c.Id == orig.Id);
            Assert.Equal(orig.Name,        imp.Name);
            Assert.Equal(orig.Description, imp.Description);
            Assert.Equal(orig.SpecimenIds, imp.SpecimenIds);
        }

        // --- Employees ---
        // EnsureAtLeastOneEmployee runs after import, so count is >= original
        Assert.True(importedEmployees.Count >= employees.Count);
        foreach (var orig in employees)
        {
            var imp = importedEmployees.Single(e => e.Id == orig.Id);
            Assert.Equal(orig.Name, imp.Name);
        }
    }

    [Fact]
    public async Task FullPipeline_IdsArePreserved_AfterImport()
    {
        // Verify that entity IDs are preserved (critical for FK relationships)
        var specimens = new List<SpecimenEntity>
        {
            new() { Id = 10, ArctosId = "ARC-1000", Name = "Specimen Ten", Description = "", IsPresent = true },
            new() { Id = 20, ArctosId = "ARC-2000", Name = "Specimen Twenty", Description = "", IsPresent = false },
        };
        var loans = new List<LoanEntity>
        {
            new()
            {
                Id = 5, ArctosId = "LOAN-5", Name = "Test Loan", Description = "",
                Borrower = "Test Borrower", Email = "test@test.com",
                DateCheckedOut = DateTime.UtcNow, DateDue = DateTime.UtcNow.AddDays(30),
                IsReturned = false, SpecimenIds = "10,20", SignatureData = null
            }
        };
        var loanSpecimens = new List<LoanSpecimenEntity>
        {
            new() { Id = 100, LoanId = 5, SpecimenId = 10, ReturnDate = null },
            new() { Id = 101, LoanId = 5, SpecimenId = 20, ReturnDate = null },
        };

        var csv = CsvService.BuildExportCsv(specimens, loans, loanSpecimens, [], []);
        await File.WriteAllTextAsync(_csvPath, csv);
        await CsvService.ImportDatabaseAsync(_csvPath);

        var importedSpecimens     = await DbService.GetAllSpecimenEntitiesAsync();
        var importedLoans         = await DbService.GetAllLoanEntitiesAsync();
        var importedLoanSpecimens = await DbService.GetAllLoanSpecimenEntitiesAsync();

        Assert.Single(importedSpecimens,     s  => s.Id  == 10);
        Assert.Single(importedSpecimens,     s  => s.Id  == 20);
        Assert.Single(importedLoans,         l  => l.Id  == 5);

        var ls1 = Assert.Single(importedLoanSpecimens, ls => ls.Id == 100);
        Assert.Equal(5,  ls1.LoanId);
        Assert.Equal(10, ls1.SpecimenId);

        var ls2 = Assert.Single(importedLoanSpecimens, ls => ls.Id == 101);
        Assert.Equal(5,  ls2.LoanId);
        Assert.Equal(20, ls2.SpecimenId);
    }

    [Fact]
    public async Task FullPipeline_CsvSectionsPresent_ForAllEntityTypes()
    {
        var specimens     = new List<SpecimenEntity> { new() { Id = 1, ArctosId = "X", Name = "N", Description = "D", IsPresent = true } };
        var loans         = new List<LoanEntity>     { new() { Id = 1, ArctosId = "Y", Name = "L", Description = "", Borrower = "B", Email = "", DateCheckedOut = DateTime.UtcNow, DateDue = DateTime.UtcNow, IsReturned = false, SpecimenIds = "1" } };
        var loanSpecimens = new List<LoanSpecimenEntity> { new() { Id = 1, LoanId = 1, SpecimenId = 1 } };
        var containers    = new List<ContainerEntity>    { new() { Id = 1, Name = "C", Description = "", SpecimenIds = "1" } };
        var employees     = new List<EmployeeEntity>     { new() { Id = 1, Name = "E" } };

        var csv = CsvService.BuildExportCsv(specimens, loans, loanSpecimens, containers, employees);

        Assert.Contains("[Specimens]",    csv);
        Assert.Contains("[Loans]",        csv);
        Assert.Contains("[LoanSpecimens]", csv);
        Assert.Contains("[Containers]",   csv);
        Assert.Contains("[Employees]",    csv);
    }

    [Fact]
    public async Task FullPipeline_EmptyDatabase_ExportImportProducesAtLeastOneEmployee()
    {
        // Import an empty export (no employees) and verify EnsureAtLeastOneEmployee fires
        var csv = CsvService.BuildExportCsv([], [], [], [], []);
        await File.WriteAllTextAsync(_csvPath, csv);
        await CsvService.ImportDatabaseAsync(_csvPath);

        var employees = await DbService.GetAllEmployeeEntitiesAsync();
        Assert.NotEmpty(employees);
    }
}
