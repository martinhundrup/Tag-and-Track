using System.Text;

namespace TagAndTrack.Backend.Items
{
    public static class ItemManager
    {
        private static List<Item> items = new List<Item>();

        public static Item? GetItemByQRID(string QRID)
        {
            return items.FirstOrDefault(i => i.QRID == QRID);
        }

        public static void AddItem(Item item)
        {
            if (!items.Any(i => i.ID == item.ID))
            {
                items.Add(item);
            }
            else
            {
                throw new ArgumentException($"Item with ID {item.ID} already exists.");
            }
        }

        public static string GetItemsOfType(Item.ItemType itemType)
        {
            DebugLogger.Log("GetItemsOfType");
            var sb = new StringBuilder();

            var specimens = items.Where(item => item.Type == itemType);

            foreach (var item in specimens)
            {
                DebugLogger.Log("processing an item");
                sb.Append($"{item.ID},{item.ArctosID},{item.Name},{item.Description},{item.Status}\n");
            }

            DebugLogger.Log("all item processing completed");
            return sb.ToString();
        }

        #region DEBUG

        public static void LoadAllDebugItems()
        {
            DebugLogger.Log("loading all debug items");
            items.AddRange(LoadDebugSpecimens());
        }

        public static List<Item> LoadDebugSpecimens()
        {
            var items = new List<Item>();

            // Split the CSV string into individual lines
            var lines = DebugItems.specimensCSV
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                try
                {
                    var parts = line.Split(',', StringSplitOptions.None);
                    if (parts.Length < 5)
                    {
                        DebugLogger.Log($"Skipping malformed line: {line}");
                        continue;
                    }

                    int id = int.Parse(parts[0].Trim());
                    string arctosId = parts[1].Trim();
                    string name = parts[2].Trim();
                    string description = parts[3].Trim();
                    bool status = bool.Parse(parts[4].Trim());

                    var specimen = new SpecimenItem(name, description);

                    specimen.GetType().BaseType?.GetProperty("ID")?.SetValue(specimen, id);
                    specimen.GetType().BaseType?.GetProperty("ArctosID")?.SetValue(specimen, arctosId);
                    specimen.GetType().BaseType?.GetProperty("Status")?.SetValue(specimen, status);

                    items.Add(specimen);
                    DebugLogger.Log($"Added specimen {id} ({name})");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"Error parsing line: {line}\n{ex}");
                }
            }

            return items;
        }


        #endregion
    }
}