using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Backend.Items
{
    public class ContainerItem : Item
    {
        private readonly List<SpecimenItem> _specimens = new();
        
        public IReadOnlyList<SpecimenItem> Specimens => _specimens;

        public ContainerItem() {
            Type = ItemType.Container;
        }

        public ContainerItem(string name, string description) : this()
        {
            Name = name;
            Description = description;
        }

        public void AddSpecimen(SpecimenItem specimen)
        {
            if (!_specimens.Any(s => s.ID == specimen.ID))
                _specimens.Add(specimen);
        }

        public void RemoveSpecimen(SpecimenItem specimen)
        {
            _specimens.RemoveAll(s => s.ID == specimen.ID);
        }

        public void ClearSpecimens()
        {
            _specimens.Clear();
        }
    }
}
