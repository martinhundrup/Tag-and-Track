using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TagAndTrack.Backend.Items
{
    public static class ItemManager
    {
        private static readonly List<Item> items = new();

        // Cache reflection once instead of doing it N times
        private static readonly PropertyInfo? IdProp =
            typeof(Item).GetProperty("ID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly PropertyInfo? ArctosIdProp =
            typeof(Item).GetProperty("ArctosID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly PropertyInfo? StatusProp =
            typeof(Item).GetProperty("Status", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public static void LoadAllDebugItems()
        {
            DebugLogger.Log("loading all debug items");
            items.AddRange(LoadDebugSpecimens());
            DebugLogger.Log($"loaded {items.Count} items");
        }

        public static List<Item> LoadDebugSpecimens()
        {
            var result = new List<Item>(EstimateLineCount(DebugItems.specimensCSV));

            using var reader = new StringReader(DebugItems.specimensCSV);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    ReadOnlySpan<char> s = line.AsSpan();

                    // id
                    int p = s.IndexOf(',');
                    if (p < 0) throw new FormatException("Missing field 0");
                    int id = int.Parse(s[..p].Trim());
                    s = s[(p + 1)..];

                    // arctosId
                    p = s.IndexOf(',');
                    if (p < 0) throw new FormatException("Missing field 1");
                    string arctosId = s[..p].Trim().ToString();
                    s = s[(p + 1)..];

                    // name
                    p = s.IndexOf(',');
                    if (p < 0) throw new FormatException("Missing field 2");
                    string name = s[..p].Trim().ToString();
                    s = s[(p + 1)..];

                    // description
                    p = s.IndexOf(',');
                    if (p < 0) throw new FormatException("Missing field 3");
                    string description = s[..p].Trim().ToString();
                    s = s[(p + 1)..];

                    // status
                    // Allow trailing commas/extra fields
                    p = s.IndexOf(',');
                    ReadOnlySpan<char> statusSpan = p >= 0 ? s[..p] : s;
                    bool status = bool.Parse(statusSpan.Trim());

                    var specimen = new SpecimenItem(name, description);

                    // Use cached PropertyInfos (no BaseType hopping, no per-iteration lookup)
                    IdProp?.SetValue(specimen, id);
                    ArctosIdProp?.SetValue(specimen, arctosId);
                    StatusProp?.SetValue(specimen, status);

                    result.Add(specimen);
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"Error parsing line: {line}\n{ex.Message}");
                }
            }

            return result;
        }

        private static int EstimateLineCount(string s)
        {
            int count = 1;
            foreach (char c in s) if (c == '\n') count++;
            return count;
        }

        public static Item? GetItemByQRID(string QRID) =>
            items.FirstOrDefault(i => i.QRID == QRID);

        public static void AddItem(Item item)
        {
            if (items.Any(i => i.ID == item.ID))
                throw new ArgumentException($"Item with ID {item.ID} already exists.");
            items.Add(item);
        }

        // If you must keep returning CSV, at least avoid per-item logs and pre-size the builder
        public static string GetItemsOfType(Item.ItemType itemType)
        {
            var filtered = items.Where(item => item.Type == itemType).ToList();
            // Rough capacity estimate: ~64 chars per row
            var sb = new System.Text.StringBuilder(filtered.Count * 64);

            foreach (var item in filtered)
            {
                sb.Append(item.ID).Append(',')
                  .Append(item.ArctosID).Append(',')
                  .Append(item.Name).Append(',')
                  .Append(item.Description).Append(',')
                  .Append(item.Status).Append('\n');
            }

            return sb.ToString();
        }
    }
}
