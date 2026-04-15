using TagAndTrack.Backend.Data;

namespace TagAndTrack.Backend.Items
{
    /// <summary>
    /// Simplified ItemManager that delegates to DbService.
    /// Kept for backwards compatibility with existing code that uses GetItemByQRID.
    /// </summary>
    public static class ItemManager
    {
        /// <summary>
        /// Gets an item by its QR ID string (e.g., "Specimen:1", "Loan:2", "Container:3").
        /// </summary>
        public static Item? GetItemByQRID(string? qrid)
        {
            if (string.IsNullOrEmpty(qrid)) return null;

            var parts = qrid.Split(':');
            if (parts.Length != 2) return null;

            var type = parts[0];
            if (!int.TryParse(parts[1], out int id)) return null;

            // Synchronously get from database (not ideal but keeps API compatible)
            return type switch
            {
                "Specimen" => Task.Run(async () => await DbService.GetSpecimenByIdAsync(id)).Result,
                "Loan" => Task.Run(async () => await DbService.GetLoanByIdAsync(id)).Result,
                "Container" => Task.Run(async () => await DbService.GetContainerByIdAsync(id)).Result,
                _ => null
            };
        }

        /// <summary>
        /// Gets all items of a specific type.
        /// </summary>
        public static List<Item> GetItemsOfType(Item.ItemType itemType)
        {
            return itemType switch
            {
                Item.ItemType.Specimen => Task.Run(async () => 
                    (await DbService.GetAllSpecimensAsync()).Cast<Item>().ToList()).Result,
                Item.ItemType.Loan => Task.Run(async () => 
                    (await DbService.GetAllLoansAsync()).Cast<Item>().ToList()).Result,
                Item.ItemType.Container => Task.Run(async () => 
                    (await DbService.GetAllContainersAsync()).Cast<Item>().ToList()).Result,
                _ => new List<Item>()
            };
        }
    }
}
