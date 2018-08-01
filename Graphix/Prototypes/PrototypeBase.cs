using System;
using System.Collections.Generic;
using System.Linq;

namespace Graphix.Prototypes
{
    /// <summary>
    /// This is the base class of all prototypes of the Graphix Library. Each Prototype
    /// can used as a prototype for other prototypes or as an object in the ui.
    /// </summary>
    public class PrototypeBase
    {
        /// <summary>
        /// A collection of all children
        /// </summary>
        public List<PrototypeBase> Container { get; private set; }

        /// <summary>
        /// The Object name that is assigned to this prototype to reference from other
        /// prototypes.
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// The name that is assigned to this object in the global scope.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The reference to the base prototype where all its data is inherited
        /// </summary>
        public PrototypeBase BasePrototype { get; set; }

        /// <summary>
        /// The type reference to the renderer for this prototype
        /// </summary>
        public Type RenderElement { get; protected set; }
        
        /// <summary>
        /// The full qualified name of the type stored in <see cref="RenderElement"/>.
        /// </summary>
        public string RenderName { get; protected set; }

        /// <summary>
        /// A list of all animations this element has
        /// </summary>
        public List<Physic.AnimationGroup> Animations { get; private set; }

        /// <summary>
        /// A list of all parameters this element has
        /// </summary>
        public Dictionary<string, IValueWrapper> Parameter { get; private set; }

        /// <summary>
        /// The parent prototype in the object tree
        /// </summary>
        public PrototypeBase Parent { get; set; }

        /// <summary>
        /// Created a new basic prototype
        /// </summary>
        public PrototypeBase()
        {
            Container = new List<PrototypeBase>();
            Animations = new List<Physic.AnimationGroup>();
            Parameter = new Dictionary<string, IValueWrapper>();
        }

        /// <summary>
        /// Creates a new element that inherits from this and clone the visual
        /// settings
        /// </summary>
        /// <returns>the new clone</returns>
        public PrototypeBase Clone()
        {
            var target = new PrototypeBase();
            //foreach (var pb in Container)
            //    target.Container.Add(pb.Clone());
            target.Name = Name;
            target.BasePrototype = this;
            target.RenderElement = RenderElement;
            return target;
        }

        /// <summary>
        /// Flatten the whole inheritance tree to a single prototype. After this all changes to
        /// the inheritance tree are ignored to the previous flatten result.
        /// This process is responsible for a major speed gain in rendering and animation.
        /// </summary>
        /// <returns>The flattened result</returns>
        public FlatPrototype Flatten()
        {
            return Flatten(new PrototypeFlattenerHelper());
        }

        /// <summary>
        /// Flatten this whole inheritance tree to a single prototype. After this all changes to
        /// the inheritance tree are ignored to the previous flatten result.
        /// All the current informations are stored in the helper.
        /// </summary>
        /// <param name="helper">
        ///     in this helper the information about this objectare stored
        /// </param>
        /// <returns>The flattened result</returns>
        public FlatPrototype Flatten(PrototypeFlattenerHelper helper)
        {
            var flat = new FlatPrototype();
            Flatten(flat, helper);
            //flat.Container.AddRange(Container.ConvertAll((pb) => pb.Flatten()));
            if (flat.RenderName == null) flat.RenderName = typeof(PrototypeBase).FullName;
            flat.Name = Name;
            return flat;
        }

        /// <summary>
        /// flatten this prototype
        /// </summary>
        /// <param name="flat">target of informations</param>
        /// <param name="helper">information that helps this flattening</param>
        private void Flatten(FlatPrototype flat, PrototypeFlattenerHelper helper)
        {
            if (BasePrototype != null) BasePrototype.Flatten(flat, helper);
            flat.Container.AddRange(Container.ConvertAll((p) =>
            {
                var h = new PrototypeFlattenerHelper();
                var f = p.Flatten(h);
                helper.Import(h);
                return f;
            }));
            foreach (var p in flat.Parameter.ToArray())
                if (Parameter.ContainsKey(p.Key))
                {
                    var par = p.Value;
                    var current = Parameter[p.Key];
                    var newPar = current.Clone();
                    helper.Conversion[par] = newPar;
                    helper.Conversion[current] = newPar;
                    flat.Parameter[p.Key] = newPar;
                }
            foreach (var p in Parameter)
                if (!flat.Parameter.ContainsKey(p.Key))
                {
                    var par = p.Value.Clone();
                    helper.Conversion[p.Value] = par;
                    flat.Parameter[p.Key] = par;
                }
            foreach (var a in Animations)
            {
                var anim = a.Clone();
                helper.Animations[a] = anim;
                flat.Animations.Add(anim);
            }
            MoveTargets(helper, flat);

            
            if (RenderName != null) flat.RenderName = RenderName;
        }

        /// <summary>
        /// Move all reference targets of the flatten result to its variables
        /// </summary>
        /// <param name="helper">helper informations</param>
        /// <param name="prot">flattened result</param>
        void MoveTargets(PrototypeFlattenerHelper helper, FlatPrototype prot)
        {
            foreach (var a in prot.Animations) a.MoveTargets(helper);
            foreach (var c in prot.Container) MoveTargets(helper, c);
            foreach (var p in prot.Parameter) p.Value.MoveTargets(helper);
        }

        /// <summary>
        /// Find a single Element in its container or its base prototype containers by
        /// its given name
        /// </summary>
        /// <param name="name">the name of the object</param>
        /// <returns>The found object or null if not found</returns>
        public PrototypeBase GetElement(string name)
        {
            if (Name == name) return this;
            PrototypeBase result;
            foreach (var pb in Container)
                if ((result = pb.GetElement(name)) != null)
                    return result;
            return BasePrototype?.GetElement(name);
        }

        /// <summary>
        /// Find an element in the container of only this prototype. The container of
        /// the base prototypes are ignored.
        /// </summary>
        /// <param name="name">the name of the object</param>
        /// <returns>The found object or null if not found</returns>
        public PrototypeBase GetOwnElement(string name)
        {
            if (Name == name) return this;
            PrototypeBase result;
            foreach (var pb in Container)
                if ((result = pb.GetElement(name)) != null)
                    return result;
            return null;
        }

        /// <summary>
        /// Search a single animation of this or base elements by its name
        /// </summary>
        /// <param name="name">the name of the animation</param>
        /// <returns>The Animation or null if not found</returns>
        public Physic.AnimationGroup GetAnimation(string name)
        {
            foreach (var ag in Animations)
                if (ag.Name == name) return ag;
            return BasePrototype?.GetAnimation(name);
        }

        /// <summary>
        /// Search a single animation in only this prototype
        /// </summary>
        /// <param name="name">the name of the animation</param>
        /// <returns>The Animation or null if not found</returns>
        public Physic.AnimationGroup GetOwnAnimation(string name)
        {
            foreach (var ag in Animations)
                if (ag.Name == name) return ag;
            return null;
        }

        /// <summary>
        /// Search a single Parameter in this or base elements by its name
        /// </summary>
        /// <param name="name">the name of the parameter</param>
        /// <returns>The Parameter or null if not found</returns>
        public IValueWrapper GetParameter(string name)
        {
            var par = GetOwnParameter(name);
            if (par != null) return par;
            par = BasePrototype?.getParameter(name);
            if (par != null) return Parameter[name] = par.Clone();
            else return par;
        }

        /// <summary>
        /// search for a parameter
        /// </summary>
        /// <param name="name">the name of this parameter</param>
        /// <returns>found parameter</returns>
        private IValueWrapper getParameter(string name)
        {
            if (Parameter.ContainsKey(name))
                return Parameter[name];
            return BasePrototype?.getParameter(name);
        }

        /// <summary>
        /// Search a single Parameter in this element by its name
        /// </summary>
        /// <param name="name">the name of the parameter</param>
        /// <returns>The Parameter or null if not found</returns>
        public IValueWrapper GetOwnParameter(string name)
        {
            if (Parameter.ContainsKey(name))

                return Parameter[name];
            return null;
        }
    }

    /// <summary>
    /// This class helps to flatten the <see cref="PrototypeBase"/>
    /// </summary>
    public class PrototypeFlattenerHelper
    {
        /// <summary>
        /// Compare <see cref="IValueWrapper"/>
        /// </summary>
        class Equalizer : IEqualityComparer<IValueWrapper>
        {
            public bool Equals(IValueWrapper x, IValueWrapper y)
            {
                return object.ReferenceEquals(x, y);
            }

            public int GetHashCode(IValueWrapper obj)
            {
                return obj.GetHashCode();
            }
        }

        /// <summary>
        /// Import the data of another <see cref="PrototypeFlattenerHelper"/>
        /// </summary>
        /// <param name="helper">the other helper</param>
        public void Import(PrototypeFlattenerHelper helper)
        {
            foreach (var conv in helper.Conversion)
                Conversion[conv.Key] = conv.Value;
            foreach (var anim in helper.Animations)
                Animations[anim.Key] = anim.Value;
        }

        /// <summary>
        /// A list of all values. The key are the value objects in the original prototype. 
        /// The value of this dictionary are value objects in the flattened result
        /// </summary>
        public Dictionary<IValueWrapper, IValueWrapper> Conversion { get; private set; }
        /// <summary>
        /// A list of all animations. The Key are the animations in the original prototype.
        /// The values are the animations of the flattened result.
        /// </summary>
        public Dictionary<Physic.AnimationGroup, Physic.AnimationGroup> Animations { get; set; }

        /// <summary>
        /// Create a new helper to flatten <see cref="PrototypeBase"/>
        /// </summary>
        public PrototypeFlattenerHelper()
        {
            Conversion = new Dictionary<IValueWrapper, IValueWrapper>(new Equalizer());
            Animations = new Dictionary<Physic.AnimationGroup, Physic.AnimationGroup>();
        }

        /// <summary>
        /// Converts a <see cref="IValueWrapper"/> using its <see cref="Conversion"/> list.
        /// </summary>
        /// <param name="value">the value from the prototype</param>
        /// <returns>the value for the flattened result</returns>
        public IValueWrapper Convert(IValueWrapper value)
        {
            if (value != null && Conversion.ContainsKey(value))
                value = Conversion[value];
            value?.MoveTargets(this);
            return value;
        }

        /// <summary>
        /// Convert a specific <see cref="ValueWrapper{T}"/> using the 
        /// <see cref="Convert(IValueWrapper)"/> method.
        /// </summary>
        /// <typeparam name="T">The type of the value wrapper</typeparam>
        /// <param name="value">the value from the prototype</param>
        /// <returns>the value in the flattened result</returns>
        public ValueWrapper<T> Convert<T>(ValueWrapper<T> value)
        {
            return (ValueWrapper<T>)Convert((IValueWrapper)value);
        }

        /// <summary>
        /// Converts a <see cref="Physic.AnimationGroup"/> using its
        /// <see cref="Animations"/> list.
        /// </summary>
        /// <param name="group">the animation from the prototype</param>
        /// <returns>the animation for the flattened result</returns>
        public Physic.AnimationGroup Convert(Physic.AnimationGroup group)
        {
            return Animations.ContainsKey(group) ? Animations[group] : group;
        }
    }
}
