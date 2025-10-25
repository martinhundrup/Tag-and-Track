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
    }
}