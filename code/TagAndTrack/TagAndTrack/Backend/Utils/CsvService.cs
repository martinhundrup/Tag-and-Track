using System.Text;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Data.Entities;

namespace TagAndTrack.Backend.Utils
{
    public static class CsvService
    {
        // ===== EXPORT =====

        public static async Task<string> ExportDatabaseAsync()
        {
            var exportDir = Path.Combine(FileSystem.CacheDirectory, "TagAndTrack_Export");
            if (Directory.Exists(exportDir))
                Directory.Delete(exportDir, true);
            Directory.CreateDirectory(exportDir);

            var filePath = Path.Combine(exportDir, "tagandtrack_export.csv");

            var specimens = await DbService.GetAllSpecimenEntitiesAsync();
            var loans = await DbService.GetAllLoanEntitiesAsync();
            var containers = await DbService.GetAllContainerEntitiesAsync();
            var employees = await DbService.GetAllEmployeeEntitiesAsync();

            var content = BuildExportCsv(specimens, loans, containers, employees);
            await File.WriteAllTextAsync(filePath, content);

            return filePath;
        }

        internal static string BuildExportCsv(
            IEnumerable<SpecimenEntity> specimens,
            IEnumerable<LoanEntity> loans,
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

        // ===== IMPORT =====

        public static async Task ImportDatabaseAsync(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath);
            var sections = ParseSections(content);

            await DbService.ResetDatabaseAsync();

            if (sections.TryGetValue("Specimens", out var specLines))
                await ImportSpecimenLines(specLines);
            if (sections.TryGetValue("Loans", out var loanLines))
                await ImportLoanLines(loanLines);
            if (sections.TryGetValue("Containers", out var containerLines))
                await ImportContainerLines(containerLines);
            if (sections.TryGetValue("Employees", out var employeeLines))
                await ImportEmployeeLines(employeeLines);
        }

        internal static Dictionary<string, List<string>> ParseSections(string content)
            => CsvHelper.ParseSections(content);

        private static async Task ImportSpecimenLines(List<string> lines)
        {
            foreach (var line in lines.Skip(1)) // skip header
            {
                var fields = ParseCsvLine(line);
                if (fields.Length < 5) continue;
                var entity = new SpecimenEntity
                {
                    ArctosId = fields[1],
                    Name = fields[2],
                    Description = fields[3],
                    IsPresent = bool.TryParse(fields[4], out bool p) && p
                };
                await DbService.InsertRawEntityAsync(entity);
            }
        }

        private static async Task ImportLoanLines(List<string> lines)
        {
            foreach (var line in lines.Skip(1))
            {
                var fields = ParseCsvLine(line);
                if (fields.Length < 11) continue;
                var entity = new LoanEntity
                {
                    ArctosId = fields[1],
                    Name = fields[2],
                    Description = fields[3],
                    Borrower = fields[4],
                    Email = fields[5],
                    DateCheckedOut = DateTime.TryParse(fields[6], out var dco) ? dco : DateTime.MinValue,
                    DateDue = DateTime.TryParse(fields[7], out var dd) ? dd : DateTime.MinValue,
                    IsReturned = bool.TryParse(fields[8], out bool r) && r,
                    SpecimenIds = fields[9],
                    SignatureData = !string.IsNullOrEmpty(fields[10]) ? Convert.FromBase64String(fields[10]) : null
                };
                await DbService.InsertRawEntityAsync(entity);
            }
        }

        private static async Task ImportContainerLines(List<string> lines)
        {
            foreach (var line in lines.Skip(1))
            {
                var fields = ParseCsvLine(line);
                if (fields.Length < 4) continue;
                var entity = new ContainerEntity
                {
                    Name = fields[1],
                    Description = fields[2],
                    SpecimenIds = fields[3]
                };
                await DbService.InsertRawEntityAsync(entity);
            }
        }

        private static async Task ImportEmployeeLines(List<string> lines)
        {
            foreach (var line in lines.Skip(1))
            {
                var fields = ParseCsvLine(line);
                if (fields.Length < 3) continue;
                var entity = new EmployeeEntity
                {
                    Name = fields[1],
                    LastLogin = DateTime.TryParse(fields[2], out var d) ? d : null
                };
                await DbService.InsertRawEntityAsync(entity);
            }
        }

        // ===== CSV HELPERS =====

        internal static string CsvEscape(string? value) => CsvHelper.Escape(value);

        internal static string[] ParseCsvLine(string line) => CsvHelper.ParseLine(line);
    }
}
