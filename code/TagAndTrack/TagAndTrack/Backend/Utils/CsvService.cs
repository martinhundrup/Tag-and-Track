using System.Text;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Data.Entities;

namespace TagAndTrack.Backend.Utils
{
    public static class CsvService
    {
        // ===== THE GRAND EXPORT =====

#if !UNIT_TESTING
        public static async Task<string> ExportDatabaseAsync()
        {
            var exportDir = Path.Combine(FileSystem.CacheDirectory, "TagAndTrack_Export");
            if (Directory.Exists(exportDir))
                Directory.Delete(exportDir, true);
            Directory.CreateDirectory(exportDir);

            var filePath = Path.Combine(exportDir, "tagandtrack_export.csv");

            await DbService.EnsureAtLeastOneEmployeeAsync();

            var specimens = await DbService.GetAllSpecimenEntitiesAsync();
            var loans = await DbService.GetAllLoanEntitiesAsync();
            var loanSpecimens = await DbService.GetAllLoanSpecimenEntitiesAsync();
            var containers = await DbService.GetAllContainerEntitiesAsync();
            var employees = await DbService.GetAllEmployeeEntitiesAsync();

            DebugLogger.Log($"CsvService: Exporting {specimens.Count} specimens, {loans.Count} loans, {loanSpecimens.Count} loan-specimen links, {containers.Count} containers, {employees.Count} employees");

            var content = BuildExportCsv(specimens, loans, loanSpecimens, containers, employees);
            await File.WriteAllTextAsync(filePath, content);

            DebugLogger.Log($"CsvService: Export complete, file size: {content.Length} bytes");
            return filePath;
        }
#endif

        internal static string BuildExportCsv(
            IEnumerable<SpecimenEntity> specimens,
            IEnumerable<LoanEntity> loans,
            IEnumerable<LoanSpecimenEntity> loanSpecimens,
            IEnumerable<ContainerEntity> containers,
            IEnumerable<EmployeeEntity> employees)
        {
            var sb = new StringBuilder();

            sb.AppendLine("[Specimens]");
            sb.AppendLine("Id,ArctosId,Name,Description,IsPresent");
            foreach (var s in specimens)
                sb.AppendLine($"{s.Id},{CsvEscape(s.ArctosId)},{CsvEscape(s.Name)},{CsvEscape(s.Description)},{s.IsPresent}");

            sb.AppendLine("[Loans]");
            sb.AppendLine("Id,ArctosId,Name,Description,Borrower,Email,DateCheckedOut,DateDue,IsReturned,SpecimenIds,SignatureData");
            foreach (var l in loans)
            {
                string sig = l.SignatureData != null ? Convert.ToBase64String(l.SignatureData) : "";
                sb.AppendLine($"{l.Id},{CsvEscape(l.ArctosId)},{CsvEscape(l.Name)},{CsvEscape(l.Description)},{CsvEscape(l.Borrower)},{CsvEscape(l.Email)},{l.DateCheckedOut:O},{l.DateDue:O},{l.IsReturned},{CsvEscape(l.SpecimenIds)},{CsvEscape(sig)}");
            }

            sb.AppendLine("[LoanSpecimens]");
            sb.AppendLine("Id,LoanId,SpecimenId,ReturnDate");
            foreach (var ls in loanSpecimens)
            {
                string returnDate = ls.ReturnDate.HasValue ? ls.ReturnDate.Value.ToString("O") : "";
                sb.AppendLine($"{ls.Id},{ls.LoanId},{ls.SpecimenId},{returnDate}");
            }

            sb.AppendLine("[Containers]");
            sb.AppendLine("Id,Name,Description,SpecimenIds");
            foreach (var c in containers)
                sb.AppendLine($"{c.Id},{CsvEscape(c.Name)},{CsvEscape(c.Description)},{CsvEscape(c.SpecimenIds)}");

            sb.AppendLine("[Employees]");
            sb.AppendLine("Id,Name,LastLogin");
            foreach (var e in employees)
            {
                string login = e.LastLogin.HasValue ? e.LastLogin.Value.ToString("O") : "";
                sb.AppendLine($"{e.Id},{CsvEscape(e.Name)},{login}");
            }

            return sb.ToString();
        }

        // ===== THE GRAND IMPORT =====

        public static async Task ImportDatabaseAsync(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath);
            DebugLogger.Log($"CsvService.ImportDatabaseAsync(): file length = {content.Length}");
            var sections = ParseSections(content);

            DebugLogger.Log($"CsvService: Parsed {sections.Count} sections from import file");
            foreach (var section in sections.Keys)
            {
                DebugLogger.Log($"  Section '{section}': {sections[section].Count} lines");
            }

            await DbService.ClearDatabaseAsync();

            if (sections.TryGetValue("Specimens", out var specLines))
            {
                DebugLogger.Log($"Importing {specLines.Count - 1} specimens...");
                await ImportSpecimenLines(specLines);
            }
            if (sections.TryGetValue("Loans", out var loanLines))
            {
                DebugLogger.Log($"Importing {loanLines.Count - 1} loans...");
                await ImportLoanLines(loanLines);
            }
            if (sections.TryGetValue("LoanSpecimens", out var loanSpecimenLines))
            {
                DebugLogger.Log($"Importing {loanSpecimenLines.Count - 1} loan-specimen links...");
                await ImportLoanSpecimenLines(loanSpecimenLines);
            }
            if (sections.TryGetValue("Containers", out var containerLines))
            {
                DebugLogger.Log($"Importing {containerLines.Count - 1} containers...");
                await ImportContainerLines(containerLines);
            }
            if (sections.TryGetValue("Employees", out var employeeLines))
            {
                DebugLogger.Log($"Importing {employeeLines.Count - 1} employees...");
                await ImportEmployeeLines(employeeLines);
            }

            await DbService.EnsureAtLeastOneEmployeeAsync();

            var specimenCount = (await DbService.GetAllSpecimenEntitiesAsync()).Count;
            var loanCount = (await DbService.GetAllLoanEntitiesAsync()).Count;
            var loanSpecimenCount = (await DbService.GetAllLoanSpecimenEntitiesAsync()).Count;
            var containerCount = (await DbService.GetAllContainerEntitiesAsync()).Count;
            var employeeCount = (await DbService.GetAllEmployeeEntitiesAsync()).Count;

            DebugLogger.Log($"CsvService: Post-import DB counts -> Specimens={specimenCount}, Loans={loanCount}, LoanSpecimens={loanSpecimenCount}, Containers={containerCount}, Employees={employeeCount}");

            var loans = await DbService.GetAllLoanEntitiesAsync();
            var joins = await DbService.GetAllLoanSpecimenEntitiesAsync();
            foreach (var loan in loans)
            {
                var linked = joins.Count(j => j.LoanId == loan.Id);
                DebugLogger.Log($"CsvService: Loan ID={loan.Id} Name='{loan.Name}' has {linked} linked specimen rows");
            }

            DebugLogger.Log("CsvService: Import complete");
        }

        internal static Dictionary<string, List<string>> ParseSections(string content)
            => CsvHelper.ParseSections(content);

        private static async Task ImportSpecimenLines(List<string> lines)
        {
            int imported = 0, skipped = 0;
            int lineNo = 1;
            var entities = new List<SpecimenEntity>();
            foreach (var line in lines.Skip(1)) // skip header
            {
                lineNo++;
                if (string.IsNullOrWhiteSpace(line)) continue;
                var fields = ParseCsvLine(line);
                if (fields.Length < 5)
                {
                    DebugLogger.Log($"Specimen line skipped (not enough fields): {line.Substring(0, Math.Min(50, line.Length))}...");
                    skipped++;
                    continue;
                }

                try
                {
                    entities.Add(new SpecimenEntity
                    {
                        Id = int.TryParse(fields[0], out var id) ? id : 0,
                        ArctosId = fields[1],
                        Name = fields[2],
                        Description = fields[3],
                        IsPresent = bool.TryParse(fields[4], out bool p) && p
                    });
                    imported++;
                }
                catch (Exception ex)
                {
                    skipped++;
                    DebugLogger.Log($"Specimen import error at section line {lineNo}: {ex.GetType().Name}: {ex.Message}");
                    DebugLogger.Log($"Specimen import failing row prefix: {line.Substring(0, Math.Min(180, line.Length))}");
                }
            }
            DebugLogger.Log($"Specimen import: parsed {imported} valid rows, {skipped} skipped. Inserting in batch...");
            await DbService.InsertRawEntitiesAsync(entities);
            DebugLogger.Log($"Specimens imported: {imported}, skipped: {skipped}");
        }

        private static async Task ImportLoanLines(List<string> lines)
        {
            int imported = 0, skipped = 0;
            var entities = new List<LoanEntity>();
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue; // skip blank lines
                var fields = ParseCsvLine(line);
                if (fields.Length < 11)
                {
                    DebugLogger.Log($"Loan line skipped (not enough fields): {line.Substring(0, Math.Min(50, line.Length))}...");
                    skipped++;
                    continue;
                }
                try
                {
                    byte[]? sigData = null;
                    if (!string.IsNullOrEmpty(fields[10]))
                    {
                        try { sigData = Convert.FromBase64String(fields[10]); }
                        catch { DebugLogger.Log($"Failed to parse Base64 signature for loan {fields[2]}"); }
                    }
                    var entity = new LoanEntity
                    {
                        Id = int.TryParse(fields[0], out var id) ? id : 0,
                        ArctosId = fields[1],
                        Name = fields[2],
                        Description = fields[3],
                        Borrower = fields[4],
                        Email = fields[5],
                        DateCheckedOut = DateTime.TryParse(fields[6], out var dco) ? dco : DateTime.MinValue,
                        DateDue = DateTime.TryParse(fields[7], out var dd) ? dd : DateTime.MinValue,
                        IsReturned = bool.TryParse(fields[8], out bool r) && r,
                        SpecimenIds = fields[9],
                        SignatureData = sigData
                    };

                    DebugLogger.Log($"CsvService: Parsed loan row -> Id={entity.Id}, Name='{entity.Name}', IsReturned={entity.IsReturned}, SpecimenIds='{entity.SpecimenIds}', SignatureBytes={(entity.SignatureData?.Length ?? 0)}");
                    entities.Add(entity);
                    imported++;
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"Error parsing loan: {ex.Message}");
                    skipped++;
                }
            }
            await DbService.InsertRawEntitiesAsync(entities);
            DebugLogger.Log($"Loans imported: {imported}, skipped: {skipped}");
        }

        private static async Task ImportLoanSpecimenLines(List<string> lines)
        {
            int imported = 0, skipped = 0;
            var entities = new List<LoanSpecimenEntity>();
            foreach (var line in lines.Skip(1)) // skip header
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var fields = ParseCsvLine(line);
                if (fields.Length < 4)
                {
                    skipped++;
                    DebugLogger.Log($"CsvService: LoanSpecimen line skipped (not enough fields): {line}");
                    continue;
                }

                var entity = new LoanSpecimenEntity
                {
                    Id = int.TryParse(fields[0], out int id) ? id : 0,
                    LoanId = int.TryParse(fields[1], out int lid) ? lid : 0,
                    SpecimenId = int.TryParse(fields[2], out int sid) ? sid : 0,
                    ReturnDate = DateTime.TryParse(fields[3], out var rd) ? rd : null
                };

                DebugLogger.Log($"CsvService: Parsed loan-specimen row -> Id={entity.Id}, LoanId={entity.LoanId}, SpecimenId={entity.SpecimenId}, ReturnDate={(entity.ReturnDate.HasValue ? entity.ReturnDate.Value.ToString("O") : "null")}");

                if (entity.LoanId > 0 && entity.SpecimenId > 0)
                {
                    entities.Add(entity);
                    imported++;
                }
                else
                {
                    skipped++;
                    DebugLogger.Log($"CsvService: LoanSpecimen skipped invalid IDs -> LoanId={entity.LoanId}, SpecimenId={entity.SpecimenId}");
                }
            }

            await DbService.InsertRawEntitiesAsync(entities);
            DebugLogger.Log($"LoanSpecimens imported: {imported}, skipped: {skipped}");
        }

        private static async Task ImportContainerLines(List<string> lines)
        {
            var entities = new List<ContainerEntity>();
            foreach (var line in lines.Skip(1))
            {
                var fields = ParseCsvLine(line);
                if (fields.Length < 4) continue;
                entities.Add(new ContainerEntity
                {
                    Id = int.TryParse(fields[0], out var id) ? id : 0,
                    Name = fields[1],
                    Description = fields[2],
                    SpecimenIds = fields[3]
                });
            }
            await DbService.InsertRawEntitiesAsync(entities);
        }

        private static async Task ImportEmployeeLines(List<string> lines)
        {
            int imported = 0, skipped = 0;
            var entities = new List<EmployeeEntity>();
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue; // skip blank lines
                var fields = ParseCsvLine(line);
                if (fields.Length < 3)
                {
                    DebugLogger.Log($"Employee line skipped (not enough fields): {line}");
                    skipped++;
                    continue;
                }
                entities.Add(new EmployeeEntity
                {
                    Id = int.TryParse(fields[0], out var id) ? id : 0,
                    Name = fields[1],
                    LastLogin = DateTime.TryParse(fields[2], out var d) ? d : null
                });
                imported++;
            }
            await DbService.InsertRawEntitiesAsync(entities);
            DebugLogger.Log($"Employees imported: {imported}, skipped: {skipped}");
        }

        // ===== FAITHFUL CSV HELPERS =====

        internal static string CsvEscape(string? value) => CsvHelper.Escape(value);

        internal static string[] ParseCsvLine(string line) => CsvHelper.ParseLine(line);
    }
}
