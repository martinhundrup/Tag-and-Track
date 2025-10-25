using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Backend.Items
{
    public class ContainerItem : Item
    {
        public ContainerItem() {
            Type = ItemType.Container;
        }

        public ContainerItem(string name, string description) : this()
        {
            Name = name;
            Description = description;
        }
    }
}
