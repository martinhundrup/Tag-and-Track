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
            var sb = new StringBuilder();

            var specimens = items.Where(item => item.Type == itemType);

            foreach (var item in specimens)
            {
                sb.Append($"{item.ID},{item.Name},{item.Status}\n");
            }

            return sb.ToString();
        }

        #region DEBUG

        public static void LoadAllDebugItems()
        {
            items.AddRange(LoadDebugSpecimens());
        }
        
        public static List<Item> LoadDebugSpecimens()
        {
            var items = new List<Item>();


            const string line = @"0,ARC-000000,Salmon Specimen,Cleaned bone specimen,true
1,ARC-000001,Lion Claw,Preserved specimen stored in fluid jar,true
2,ARC-000002,Bear Femur,Egg sample in container,false
3,ARC-000003,Heron Egg,Fluid jar sample with tag,true
4,ARC-000004,Toad Sample,Full skeleton display,false
5,ARC-000005,Owl Wing,Tissue sample for genetics,true
6,ARC-000006,Beaver Skull,Cleaned bone specimen,true
7,ARC-000007,Lion Claw,Cleaned bone specimen,true
8,ARC-000008,Toad Sample,Loose bone fragment,false
9,ARC-000009,Fox Pelt,Cleaned bone specimen,false";

                var parts = line.Split(',');

            try
            {
                int id = int.Parse(parts[0]);
                string arctosId = parts[1];
                string name = parts[2];
                string description = parts[3];
                bool status = bool.Parse(parts[4]);

                var specimen = new SpecimenItem(name, description);
                specimen.GetType().BaseType
                    ?.GetProperty("ID")?.SetValue(specimen, id);
                specimen.GetType().BaseType
                    ?.GetProperty("ArctosID")?.SetValue(specimen, arctosId);
                specimen.GetType().BaseType
                    ?.GetProperty("Status")?.SetValue(specimen, status);

                items.Add(specimen);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing line: {line}\n{ex.Message}");
            }

            return items;
            }

        #endregion
    }
}