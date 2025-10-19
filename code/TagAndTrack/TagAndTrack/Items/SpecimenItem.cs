using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Items
{
    internal class SpecimenItem : Item
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
