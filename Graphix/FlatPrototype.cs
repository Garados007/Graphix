using System.Collections.Generic;

namespace Graphix
{
    /// <summary>
    /// The flattened <see cref="Prototypes.PrototypeBase"/> that contains only the necessary
    /// informations for the runtime
    /// </summary>
    public class FlatPrototype
    {
        /// <summary>
        /// Contains all the children <see cref="FlatPrototype"/>s.
        /// </summary>
        public List<FlatPrototype> Container { get; private set; }

        /// <summary>
        /// Contains all animations defined for this <see cref="FlatPrototype"/>.
        /// </summary>
        public List<Physic.AnimationGroup> Animations { get; private set; }

        /// <summary>
        /// Contains all parameters for this <see cref="FlatPrototype"/>.
        /// </summary>
        public Dictionary<string, IValueWrapper> Parameter { get; private set; }

        /// <summary>
        /// The Name of the renderer
        /// </summary>
        public string RenderName { get; set; }

        /// <summary>
        /// The public name for this object
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The flattened <see cref="Prototypes.PrototypeBase"/> that contains only the necessary
        /// informations for the runtime
        /// </summary>
        public FlatPrototype()
        {
            Container = new List<FlatPrototype>();
            Animations = new List<Physic.AnimationGroup>();
            Parameter = new Dictionary<string, IValueWrapper>();
        }
    }
}
