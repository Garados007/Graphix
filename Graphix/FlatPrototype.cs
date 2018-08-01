using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix
{
    public class FlatPrototype
    {
        public List<FlatPrototype> Container { get; private set; }

        public List<Physic.AnimationGroup> Animations { get; private set; }

        public Dictionary<string, IValueWrapper> Parameter { get; private set; }

        public string RenderName { get; set; }

        public string Name { get; set; }

        public FlatPrototype()
        {
            Container = new List<FlatPrototype>();
            Animations = new List<Physic.AnimationGroup>();
            Parameter = new Dictionary<string, IValueWrapper>();
        }
    }
}
