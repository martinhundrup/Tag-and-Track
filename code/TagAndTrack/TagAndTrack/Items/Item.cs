namespace TagAndTrack.Items
{

    internal abstract class Item
    {
        public enum ItemType
        {
            None,           // for errors
            Specimen,
            Loan,           // contains multiple specimens and loan info
            Container,      // for shelves and stuff
        }

        public Item()
        {
            Type = Item.ItemType.None;
        }

        public Item(string name, string description) : this()
        {
            Name = name;
            Description = description;
        }

        public ItemType Type // avoids using reflection
        {
            get;
            protected set;
        }

        public int ID // internal ID to tag and track
        {
            get;
            protected set;
        }

        public string? ArctosID // ArctosDB ID
        {
            get;
            protected set;
        }

        public string? Name // display name
        {
            get;
            protected set;
        }

        public string? Description // brief description of the item
        {
            get;
            protected set;
        }

        public string? QRID // qr code ID
        {
            get
            {
                return $"{Type.ToString()}:{ID}"; // ex: "Specimen:4920382"
            }
        }
    }
}
