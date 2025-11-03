namespace TagAndTrack.Backend.Items
{
    public class SpecimenItem : Item
    {
        public SpecimenItem()
        {
            Type = ItemType.Specimen;
        }

        public SpecimenItem(string name, string description) : this()
        {
            Name = name;
            Description = description;
        }
    }
}
