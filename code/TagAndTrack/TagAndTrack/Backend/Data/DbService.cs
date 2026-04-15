using SQLite;
using TagAndTrack.Backend.Data.Entities;
using TagAndTrack.Backend.Employees;
using TagAndTrack.Backend.Items;
using TagAndTrack.Backend.Utils;

namespace TagAndTrack.Backend.Data
{
    public static class DbService
    {
        private static SQLiteAsyncConnection? _db;
        private static bool _initialized = false;

        public static async Task InitAsync()
        {
            if (_initialized)
            {
                DebugLogger.Log("DbService.InitAsync() - already initialized, skipping");
                return;
            }

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tagandtrack.db3");
            DebugLogger.Log($"DbService: Creating connection to {dbPath}");
            _db = new SQLiteAsyncConnection(dbPath);

            DebugLogger.Log("DbService: Creating SpecimenEntity table...");
            await _db.CreateTableAsync<SpecimenEntity>();
            DebugLogger.Log("DbService: Creating LoanEntity table...");
            await _db.CreateTableAsync<LoanEntity>();
            DebugLogger.Log("DbService: Creating ContainerEntity table...");
            await _db.CreateTableAsync<ContainerEntity>();
            DebugLogger.Log("DbService: Creating EmployeeEntity table...");
            await _db.CreateTableAsync<EmployeeEntity>();
            DebugLogger.Log("DbService: Creating LoanSpecimenEntity table...");
            await _db.CreateTableAsync<LoanSpecimenEntity>();

            _initialized = true;
            DebugLogger.Log($"DbService: Database initialized successfully at {dbPath}");
        }

        // ===== SPECIMENS =====
        public static async Task<List<SpecimenItem>> GetAllSpecimensAsync()
        {
            var entities = await _db!.Table<SpecimenEntity>().ToListAsync();
            return entities.Select(MapToSpecimen).ToList();
        }

        public static async Task<SpecimenItem?> GetSpecimenByIdAsync(int id)
        {
            var entity = await _db!.Table<SpecimenEntity>().FirstOrDefaultAsync(x => x.Id == id);
            return entity == null ? null : MapToSpecimen(entity);
        }

        public static async Task<int> AddSpecimenAsync(SpecimenItem specimen)
        {
            var entity = new SpecimenEntity
            {
                ArctosId = specimen.ArctosID,
                Name = specimen.Name ?? "",
                Description = specimen.Description ?? "",
                IsPresent = specimen.Status
            };
            await _db!.InsertAsync(entity);
            return entity.Id;
        }

        public static async Task<bool> ArctosIdExistsAsync(string arctosId)
        {
            var existing = await _db!.Table<SpecimenEntity>().FirstOrDefaultAsync(x => x.ArctosId == arctosId);
            return existing != null;
        }

        public static async Task UpdateSpecimenAsync(int id, bool isPresent)
        {
            var entity = await _db!.Table<SpecimenEntity>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                entity.IsPresent = isPresent;
                await _db!.UpdateAsync(entity);
            }
        }

        public static async Task<bool> IsSpecimenInAnyLoanAsync(int specimenId)
        {
            var entities = await _db!.Table<LoanEntity>().ToListAsync();
            return entities.Any(e => SpecimenIdHelper.ContainsSpecimenId(e.SpecimenIds, specimenId));
        }

        public static async Task DeleteSpecimenAsync(int specimenId)
        {
            // Remove from specimen table
            await _db!.DeleteAsync<SpecimenEntity>(specimenId);

            // Remove from any container references
            var containers = await _db!.Table<ContainerEntity>().ToListAsync();
            foreach (var c in containers)
            {
                if (!string.IsNullOrEmpty(c.SpecimenIds))
                {
                    var ids = c.SpecimenIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                    var idStr = specimenId.ToString();
                    if (ids.Remove(idStr))
                    {
                        c.SpecimenIds = string.Join(",", ids);
                        await _db!.UpdateAsync(c);
                    }
                }
            }
        }

        private static SpecimenItem MapToSpecimen(SpecimenEntity e)
        {
            var s = new SpecimenItem(e.ArctosId, e.Name, e.Description);
            SetItemId(s, (ulong)e.Id);
            SetItemArctosId(s, e.ArctosId);
            SetItemStatus(s, e.IsPresent);
            return s;
        }

        // ===== LOANS =====
        public static async Task<List<LoanItem>> GetAllLoansAsync()
        {
            var entities = await _db!.Table<LoanEntity>().ToListAsync();
            var loans = new List<LoanItem>();
            foreach (var e in entities)
            {
                loans.Add(await MapToLoanAsync(e));
            }
            return loans;
        }

        public static async Task<LoanItem?> GetLoanByIdAsync(int id)
        {
            var entity = await _db!.Table<LoanEntity>().FirstOrDefaultAsync(x => x.Id == id);
            return entity == null ? null : await MapToLoanAsync(entity);
        }

        public static async Task<int> AddLoanAsync(LoanItem loan)
        {
            var specimenIds = string.Join(",", loan.Specimens.Select(s => s.ID));
            var entity = new LoanEntity
            {
                ArctosId = loan.ArctosID,
                Name = loan.Name ?? "",
                Description = loan.Description ?? "",
                Borrower = loan.Borrower ?? "",
                Email = loan.Email ?? "",
                DateCheckedOut = loan.DateCheckedOut,
                DateDue = loan.DateDue,
                IsReturned = loan.Status,
                SpecimenIds = specimenIds,
                SignatureData = loan.SignatureImageBytes
            };
            await _db!.InsertAsync(entity);

            // Insert join rows for each specimen
            foreach (var specimen in loan.Specimens)
            {
                var returnDate = loan.GetSpecimenReturnDate(specimen.ID);
                await _db!.InsertAsync(new LoanSpecimenEntity
                {
                    LoanId = entity.Id,
                    SpecimenId = (int)specimen.ID,
                    ReturnDate = returnDate
                });
            }

            return entity.Id;
        }

        public static async Task UpdateLoanAsync(int id, bool isReturned)
        {
            var entity = await _db!.Table<LoanEntity>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                entity.IsReturned = isReturned;
                await _db!.UpdateAsync(entity);
            }
        }

        public static async Task CheckInLoanSpecimenAsync(int loanId, int specimenId, DateTime returnDate)
        {
            var joinRow = await _db!.Table<LoanSpecimenEntity>()
                .FirstOrDefaultAsync(x => x.LoanId == loanId && x.SpecimenId == specimenId);
            if (joinRow != null)
            {
                joinRow.ReturnDate = returnDate;
                await _db!.UpdateAsync(joinRow);
            }
        }

        public static async Task<bool> IsSpecimenInAnyActiveLoanAsync(int specimenId, int excludeLoanId)
        {
            var joinRows = await _db!.Table<LoanSpecimenEntity>()
                .Where(x => x.SpecimenId == specimenId && x.ReturnDate == null)
                .ToListAsync();
            return joinRows.Any(x => x.LoanId != excludeLoanId);
        }

        public static async Task<List<LoanSpecimenEntity>> GetAllLoanSpecimenEntitiesAsync()
            => await _db!.Table<LoanSpecimenEntity>().ToListAsync();

        private static async Task<LoanItem> MapToLoanAsync(LoanEntity e)
        {
            var loan = new LoanItem(e.Name, e.Description);
            SetItemId(loan, (ulong)e.Id);
            SetItemArctosId(loan, e.ArctosId);
            SetItemStatus(loan, e.IsReturned);
            loan.SetBorrower(e.Borrower);
            loan.SetEmail(e.Email);
            loan.SetDates(e.DateCheckedOut, e.DateDue);
            loan.SetSignature(e.SignatureData);

            // Load specimens and return dates from join table
            var joinRows = await _db!.Table<LoanSpecimenEntity>()
                .Where(ls => ls.LoanId == e.Id)
                .ToListAsync();

            foreach (var joinRow in joinRows)
            {
                var specimen = await GetSpecimenByIdAsync(joinRow.SpecimenId);
                if (specimen != null)
                {
                    loan.AddSpecimen(specimen);
                    loan.SetSpecimenReturnDate(specimen.ID, joinRow.ReturnDate);
                }
            }

            return loan;
        }

        // ===== CONTAINERS =====
        public static async Task<List<ContainerItem>> GetAllContainersAsync()
        {
            var entities = await _db!.Table<ContainerEntity>().ToListAsync();
            var containers = new List<ContainerItem>();
            foreach (var e in entities)
            {
                containers.Add(await MapToContainerAsync(e));
            }
            return containers;
        }

        public static async Task<ContainerItem?> GetContainerByIdAsync(int id)
        {
            DebugLogger.Log($"DbService.GetContainerByIdAsync called with id={id}");
            var entity = await _db!.Table<ContainerEntity>().FirstOrDefaultAsync(x => x.Id == id);
            DebugLogger.Log($"DbService.GetContainerByIdAsync: entity = {(entity == null ? "null" : entity.Name)}");
            return entity == null ? null : await MapToContainerAsync(entity);
        }

        public static async Task<int> AddContainerAsync(ContainerItem container)
        {
            var specimenIds = string.Join(",", container.Specimens.Select(s => s.ID));
            var entity = new ContainerEntity
            {
                Name = container.Name ?? "",
                Description = container.Description ?? "",
                SpecimenIds = specimenIds
            };
            await _db!.InsertAsync(entity);
            return entity.Id;
        }

        public static async Task UpdateContainerSpecimensAsync(int id, List<SpecimenItem> specimens)
        {
            var entity = await _db!.Table<ContainerEntity>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                entity.SpecimenIds = string.Join(",", specimens.Select(s => s.ID));
                await _db!.UpdateAsync(entity);
            }
        }

        private static async Task<ContainerItem> MapToContainerAsync(ContainerEntity e)
        {
            var container = new ContainerItem(e.Name, e.Description);
            SetItemId(container, (ulong)e.Id);

            if (!string.IsNullOrEmpty(e.SpecimenIds))
            {
                var ids = e.SpecimenIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var idStr in ids)
                {
                    if (int.TryParse(idStr, out int specId))
                    {
                        var specimen = await GetSpecimenByIdAsync(specId);
                        if (specimen != null) container.AddSpecimen(specimen);
                    }
                }
            }
            return container;
        }

        public static async Task<List<ContainerItem>> GetContainersBySpecimenIdAsync(int specimenId)
        {
            var entities = await _db!.Table<ContainerEntity>().ToListAsync();
            var result = new List<ContainerItem>();
            foreach (var e in entities)
            {
                if (SpecimenIdHelper.ContainsSpecimenId(e.SpecimenIds, specimenId))
                {
                    result.Add(await MapToContainerAsync(e));
                }
            }
            return result;
        }

        // ===== LOANS (by specimen) =====
        public static async Task<List<LoanItem>> GetLoansBySpecimenIdAsync(int specimenId)
        {
            var joinRows = await _db!.Table<LoanSpecimenEntity>()
                .Where(ls => ls.SpecimenId == specimenId)
                .ToListAsync();

            var result = new List<LoanItem>();
            var loanIds = joinRows.Select(j => j.LoanId).Distinct();
            foreach (var loanId in loanIds)
            {
                var entity = await _db!.Table<LoanEntity>().FirstOrDefaultAsync(x => x.Id == loanId);
                if (entity != null)
                    result.Add(await MapToLoanAsync(entity));
            }

            return result;
        }

        // ===== EMPLOYEES =====
        public static async Task<List<Employee>> GetAllEmployeesAsync()
        {
            DebugLogger.Log("DbService.GetAllEmployeesAsync() called");
            var entities = await _db!.Table<EmployeeEntity>().ToListAsync();
            DebugLogger.Log($"DbService: Found {entities.Count} employee entities in database");
            var result = entities.Select(e => new Employee(e.Name, e.Id) { LastLogin = e.LastLogin ?? DateTime.MinValue }).ToList();
            DebugLogger.Log($"DbService: Returning {result.Count} Employee objects");
            return result;
        }

        public static async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            var entity = await _db!.Table<EmployeeEntity>().FirstOrDefaultAsync(x => x.Id == id);
            return entity == null ? null : new Employee(entity.Name, entity.Id) { LastLogin = entity.LastLogin ?? DateTime.MinValue };
        }

        public static async Task<int> AddEmployeeAsync(string name)
        {
            var entity = new EmployeeEntity { Name = name };
            await _db!.InsertAsync(entity);
            return entity.Id;
        }

        public static async Task UpdateEmployeeLoginAsync(int id)
        {
            var entity = await _db!.Table<EmployeeEntity>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                entity.LastLogin = DateTime.Now;
                await _db!.UpdateAsync(entity);
            }
        }

        // ===== SEED DATA =====
        public static async Task SeedIfEmptyAsync()
        {
            DebugLogger.Log("DbService.SeedIfEmptyAsync() called");
            
            var specimenCount = await _db!.Table<SpecimenEntity>().CountAsync();
            DebugLogger.Log($"DbService: Specimen count = {specimenCount}");
            
            var employeeCount = await _db!.Table<EmployeeEntity>().CountAsync();
            DebugLogger.Log($"DbService: Employee count = {employeeCount}");
            
            if (specimenCount > 0 || employeeCount > 0)
            {
                DebugLogger.Log("DbService: Database already has data, skipping seed");
                return;
            }

            DebugLogger.Log("DbService: Seeding database with sample data...");

            // 100 Sample specimens based on original debugSpecimens.csv
            var specimenNames = new[] { "Salmon Specimen", "Lion Claw", "Bear Femur", "Heron Egg", "Toad Sample", "Owl Wing", "Beaver Skull", "Fox Pelt", "Eagle Feather", "Wolf Skull" };
            var descriptions = new[] { "Cleaned bone specimen", "Preserved specimen stored in fluid jar", "Egg sample in container", "Fluid jar sample with tag", "Full skeleton display", "Tissue sample for genetics", "Loose bone fragment", "Taxidermy display specimen", "Mounted pelt used for teaching", "Partial remains specimen" };
            
            DebugLogger.Log("DbService: Inserting 100 specimen records...");
            for (int i = 0; i < 100; i++)
            {
                var specimen = new SpecimenEntity
                {
                    Name = specimenNames[i % specimenNames.Length],
                    Description = descriptions[i % descriptions.Length],
                    ArctosId = $"ARC-{i:D6}",
                    IsPresent = (i % 3 != 0) // ~67% present
                };
                await _db.InsertAsync(specimen);
            }
            DebugLogger.Log("DbService: 100 specimens inserted");

            // Sample employees
            var sampleEmployees = new[]
            {
                new EmployeeEntity { Name = "Martin Hundrup" },
                new EmployeeEntity { Name = "Museum Staff 1" },
                new EmployeeEntity { Name = "Museum Staff 2" },
            };

            DebugLogger.Log($"DbService: Inserting {sampleEmployees.Length} employee records...");
            foreach (var e in sampleEmployees)
            {
                await _db.InsertAsync(e);
            }

            // 10 Sample loans with specimens
            DebugLogger.Log("DbService: Inserting 10 loan records...");
            var borrowers = new[] { "University of Alaska", "Smithsonian Museum", "Field Museum Chicago", "Natural History Museum LA", "Harvard Museum" };
            var emails = new[] { "loans@alaska.edu", "loans@si.edu", "loans@fieldmuseum.org", "loans@nhm.org", "loans@harvard.edu" };
            
            for (int i = 0; i < 10; i++)
            {
                // Each loan gets ~5-10 specimens (based on loan index)
                var specimenIds = new List<int>();
                int startSpec = i * 10 + 1;
                int endSpec = startSpec + 5 + (i % 5);
                for (int s = startSpec; s <= endSpec && s <= 100; s++)
                {
                    specimenIds.Add(s);
                }

                var loan = new LoanEntity
                {
                    Name = $"Loan #{i + 1}",
                    Description = $"Research loan for {borrowers[i % borrowers.Length]}",
                    ArctosId = $"LOAN-{i + 1:D4}",
                    Borrower = borrowers[i % borrowers.Length],
                    Email = emails[i % emails.Length],
                    DateCheckedOut = DateTime.Now.AddDays(-30 - i * 10),
                    DateDue = DateTime.Now.AddDays(60 + i * 10),
                    IsReturned = (i >= 8), // Last 2 loans are returned
                    SpecimenIds = string.Join(",", specimenIds)
                };
                await _db.InsertAsync(loan);

                // Insert join rows for each specimen in this loan
                foreach (var specId in specimenIds)
                {
                    var loanSpecimen = new LoanSpecimenEntity
                    {
                        LoanId = loan.Id,
                        SpecimenId = specId,
                        ReturnDate = (i >= 8) ? DateTime.Now.AddDays(-5) : null // returned loans get a return date
                    };
                    await _db.InsertAsync(loanSpecimen);
                }
            }
            DebugLogger.Log("DbService: 10 loans inserted");

            // Sample containers
            DebugLogger.Log("DbService: Inserting sample containers...");
            var containers = new[]
            {
                new ContainerEntity { Name = "Cabinet A-1", Description = "Main storage cabinet", SpecimenIds = "1,2,3,4,5" },
                new ContainerEntity { Name = "Cabinet A-2", Description = "Secondary storage", SpecimenIds = "6,7,8,9,10" },
                new ContainerEntity { Name = "Cabinet B-1", Description = "Overflow storage", SpecimenIds = "11,12,13" },
            };
            foreach (var c in containers)
            {
                await _db.InsertAsync(c);
            }

            DebugLogger.Log("DbService: Database seeded successfully!");
        }

        // ===== RESET =====
        /// <summary>
        /// Drops all tables, reinitializes the schema, and reseeds sample data.
        /// Used by the "Clear Database" button to restore seed state.
        /// </summary>
        public static async Task ResetDatabaseAsync()
        {
            await ClearDatabaseAsync();

#if SEED_DB
            DebugLogger.Log("DbService: Tables recreated. Seeding...");
            await SeedIfEmptyAsync();
#endif

            DebugLogger.Log("DbService: Database reset complete.");
        }

        /// <summary>
        /// Drops all tables and reinitializes empty schema without seeding.
        /// Used before CSV import to ensure a clean slate.
        /// </summary>
        public static async Task ClearDatabaseAsync()
        {
            DebugLogger.Log("DbService.ClearDatabaseAsync() - wiping database...");

            await _db!.DropTableAsync<SpecimenEntity>();
            await _db!.DropTableAsync<LoanEntity>();
            await _db!.DropTableAsync<LoanSpecimenEntity>();
            await _db!.DropTableAsync<ContainerEntity>();
            await _db!.DropTableAsync<EmployeeEntity>();

            DebugLogger.Log("DbService: All tables dropped. Recreating...");

            await _db!.CreateTableAsync<SpecimenEntity>();
            await _db!.CreateTableAsync<LoanEntity>();
            await _db!.CreateTableAsync<LoanSpecimenEntity>();
            await _db!.CreateTableAsync<ContainerEntity>();
            await _db!.CreateTableAsync<EmployeeEntity>();

            DebugLogger.Log("DbService: Database cleared (no seed).");
        }

        // ===== HELPERS =====
        private static readonly System.Reflection.PropertyInfo? IdProp =
            typeof(Item).GetProperty("ID", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        private static readonly System.Reflection.PropertyInfo? ArctosIdProp =
            typeof(Item).GetProperty("ArctosID", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        private static readonly System.Reflection.PropertyInfo? StatusProp =
            typeof(Item).GetProperty("Status", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        private static void SetItemId(Item item, ulong id) => IdProp?.SetValue(item, id);
        private static void SetItemArctosId(Item item, string? arctosId) => ArctosIdProp?.SetValue(item, arctosId);
        private static void SetItemStatus(Item item, bool status) => StatusProp?.SetValue(item, status);

        // ===== RAW ENTITY ACCESS (for CSV import/export) =====
        public static async Task<List<SpecimenEntity>> GetAllSpecimenEntitiesAsync()
            => await _db!.Table<SpecimenEntity>().ToListAsync();

        public static async Task<List<LoanEntity>> GetAllLoanEntitiesAsync()
            => await _db!.Table<LoanEntity>().ToListAsync();

        public static async Task<List<ContainerEntity>> GetAllContainerEntitiesAsync()
            => await _db!.Table<ContainerEntity>().ToListAsync();

        public static async Task<List<EmployeeEntity>> GetAllEmployeeEntitiesAsync()
            => await _db!.Table<EmployeeEntity>().ToListAsync();

        public static async Task InsertRawEntityAsync<T>(T entity) where T : new()
            => await _db!.InsertAsync(entity);
    }
}
