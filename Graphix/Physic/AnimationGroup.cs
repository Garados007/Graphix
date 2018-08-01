using System.Collections.Generic;

namespace Graphix.Physic
{
    /// <summary>
    /// Container for a single animations of an object
    /// </summary>
    public class AnimationGroup
    {
        /// <summary>
        /// The list of all activations. If one signals then this animation would
        /// be executed
        /// </summary>
        public List<AnimationActivation> Activations { get; private set; }

        /// <summary>
        /// The list of all effects that are executed sequential (except the flag
        /// async is on).
        /// </summary>
        public List<AnimationEffect> Effects { get; private set; }

        /// <summary>
        /// The name of this <see cref="AnimationGroup"/> that can be later referenced.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The timing multipler for the animation execution. 1 is normal time.
        /// below 1 is slower, higher than 1 is faster
        /// </summary>
        public ValueWrapper<double> EffectTiming { get; set; }

        /// <summary>
        /// Creates a container for a single animation for an object
        /// </summary>
        public AnimationGroup()
        {
            Activations = new List<AnimationActivation>();
            Effects = new List<AnimationEffect>();
            EffectTiming = new ValueWrapper<double>();
            HookedAnimations = new List<AfterAnimation>();
        }

        /// <summary>
        /// A list of all animations that a referenced to this.
        /// </summary>
        internal List<AfterAnimation> HookedAnimations { get; private set; }

        /// <summary>
        /// Clones this animation completly.
        /// </summary>
        /// <returns>The clone</returns>
        public AnimationGroup Clone()
        {
            var g = new AnimationGroup();
            foreach (var a in Activations) g.Activations.Add(a.Clone());
            foreach (var e in Effects) g.Effects.Add(e.Clone());
            g.Name = Name;
            g.EffectTiming = (ValueWrapper<double>)EffectTiming.Clone();
            return g;
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public void MoveTargets(Prototypes.PrototypeFlattenerHelper helper)
        {
            foreach (var a in Activations) a.MoveTargets(helper);
            foreach (var e in Effects) e.MoveTargets(helper);
            EffectTiming = helper.Convert(EffectTiming);
            EffectTiming.MoveTargets(helper);
        }
    }
}
