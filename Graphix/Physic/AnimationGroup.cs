using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Physic
{
    public class AnimationGroup
    {
        public List<AnimationActivation> Activations { get; private set; }

        public List<AnimationEffect> Effects { get; private set; }

        public string Name { get; set; }

        public ValueWrapper<double> EffectTiming { get; set; }

        public AnimationGroup()
        {
            Activations = new List<AnimationActivation>();
            Effects = new List<AnimationEffect>();
            EffectTiming = new ValueWrapper<double>();
            HookedAnimations = new List<AfterAnimation>();
        }

        internal List<AfterAnimation> HookedAnimations { get; private set; }

        public AnimationGroup Clone()
        {
            var g = new AnimationGroup();
            foreach (var a in Activations) g.Activations.Add(a.Clone());
            foreach (var e in Effects) g.Effects.Add(e.Clone());
            g.Name = Name;
            g.EffectTiming = (ValueWrapper<double>)EffectTiming.Clone();
            return g;
        }

        public void MoveTargets(Prototypes.PrototypeFlattenerHelper helper)
        {
            foreach (var a in Activations) a.MoveTargets(helper);
            foreach (var e in Effects) e.MoveTargets(helper);
            EffectTiming = helper.Convert(EffectTiming);
            EffectTiming.MoveTargets(helper);
        }
    }
}
