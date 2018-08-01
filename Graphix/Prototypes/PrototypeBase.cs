using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Prototypes
{
    public class PrototypeBase
    {
        public List<PrototypeBase> Container { get; private set; }

        public string ObjectName { get; set; }

        public string Name { get; set; }

        public PrototypeBase BasePrototype { get; set; }

        public Type RenderElement { get; protected set; }
        
        public string RenderName { get; protected set; }

        public List<Physic.AnimationGroup> Animations { get; private set; }

        public Dictionary<string, IValueWrapper> Parameter { get; private set; }

        public PrototypeBase Parent { get; set; }

        public PrototypeBase()
        {
            Container = new List<PrototypeBase>();
            Animations = new List<Physic.AnimationGroup>();
            Parameter = new Dictionary<string, IValueWrapper>();
        }

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

        public FlatPrototype Flatten()
        {
            return Flatten(new PrototypeFlattenerHelper());
        }

        public FlatPrototype Flatten(PrototypeFlattenerHelper helper)
        {
            var flat = new FlatPrototype();
            Flatten(flat, helper);
            //flat.Container.AddRange(Container.ConvertAll((pb) => pb.Flatten()));
            if (flat.RenderName == null) flat.RenderName = typeof(PrototypeBase).FullName;
            flat.Name = Name;
            return flat;
        }

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

        void MoveTargets(PrototypeFlattenerHelper helper, FlatPrototype prot)
        {
            foreach (var a in prot.Animations) a.MoveTargets(helper);
            foreach (var c in prot.Container) MoveTargets(helper, c);
            foreach (var p in prot.Parameter) p.Value.MoveTargets(helper);
        }

        public PrototypeBase GetElement(string name)
        {
            if (Name == name) return this;
            PrototypeBase result;
            foreach (var pb in Container)
                if ((result = pb.GetElement(name)) != null)
                    return result;
            return BasePrototype?.GetElement(name);
        }

        public PrototypeBase GetOwnElement(string name)
        {
            if (Name == name) return this;
            PrototypeBase result;
            foreach (var pb in Container)
                if ((result = pb.GetElement(name)) != null)
                    return result;
            return null;
        }

        public Physic.AnimationGroup GetAnimation(string name)
        {
            foreach (var ag in Animations)
                if (ag.Name == name) return ag;
            return BasePrototype?.GetAnimation(name);
        }

        public Physic.AnimationGroup GetOwnAnimation(string name)
        {
            foreach (var ag in Animations)
                if (ag.Name == name) return ag;
            return null;
        }

        public IValueWrapper GetParameter(string name)
        {
            var par = GetOwnParameter(name);
            if (par != null) return par;
            par = BasePrototype?.getParameter(name);
            if (par != null) return Parameter[name] = par.Clone();
            else return par;
        }

        private IValueWrapper getParameter(string name)
        {
            if (Parameter.ContainsKey(name))
                return Parameter[name];
            return BasePrototype?.getParameter(name);
        }

        public IValueWrapper GetOwnParameter(string name)
        {
            if (Parameter.ContainsKey(name))

                return Parameter[name];
            return null;
        }
    }

    public class PrototypeFlattenerHelper
    {
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

        public void Import(PrototypeFlattenerHelper helper)
        {
            foreach (var conv in helper.Conversion)
                Conversion[conv.Key] = conv.Value;
            foreach (var anim in helper.Animations)
                Animations[anim.Key] = anim.Value;
        }

        public Dictionary<IValueWrapper, IValueWrapper> Conversion { get; private set; }
        public Dictionary<Physic.AnimationGroup, Physic.AnimationGroup> Animations { get; set; }

        public PrototypeFlattenerHelper()
        {
            Conversion = new Dictionary<IValueWrapper, IValueWrapper>(new Equalizer());
            Animations = new Dictionary<Physic.AnimationGroup, Physic.AnimationGroup>();
        }

        public IValueWrapper Convert(IValueWrapper value)
        {
            if (value != null && Conversion.ContainsKey(value))
                value = Conversion[value];
            value?.MoveTargets(this);
            return value;
        }

        public ValueWrapper<T> Convert<T>(ValueWrapper<T> value)
        {
            return (ValueWrapper<T>)Convert((IValueWrapper)value);
        }

        public Physic.AnimationGroup Convert(Physic.AnimationGroup group)
        {
            return Animations.ContainsKey(group) ? Animations[group] : group;
        }
    }
}
