using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Graphix.Prototypes;
using Color = System.Drawing.Color;

namespace Graphix.Physic
{
    /// <summary>
    /// Defines the Effect that should be done when its Animation is called
    /// </summary>
    public abstract class AnimationEffect
    {
        /// <summary>
        /// The start time when this effect is called. Its the time since the
        /// complete start of this animation sequence
        /// </summary>
        public ValueWrapper<double> TimeStart { get; set; }

        /// <summary>
        /// The offset time when this effect is called. It's the time since the
        /// execution of the previous effect
        /// </summary>
        public ValueWrapper<double> TimeOffset { get; set; }

        /// <summary>
        /// The time when this effect should be finished. It's the time since the
        /// complete start of this animation sequence.
        /// </summary>
        public ValueWrapper<double> TimeFinish { get; set; }

        /// <summary>
        /// The time this effect should durate. Its the time since the start of
        /// this effect.
        /// </summary>
        public ValueWrapper<double> TimeDuration { get; set; }

        /// <summary>
        /// Determine if this effect should be executed in reverse direction.
        /// </summary>
        public ValueWrapper<bool> Reverse { get; set; }

        /// <summary>
        /// The amount of how often this effect should be executed
        /// </summary>
        public ValueWrapper<RepeatMode> Repeat { get; set; }

        /// <summary>
        /// The timing curve for this effect
        /// </summary>
        public ValueWrapper<AnimationMode> Mode { get; set; }

        /// <summary>
        /// Run this effect asynchron from the the other effects. 
        /// </summary>
        public ValueWrapper<bool> Async { get; set; }

        /// <summary>
        /// Enable this effect. If this effect is not enabled the execution whould be skipped to the next effect.
        /// </summary>
        public ValueWrapper<bool> Enable { get; set; }

        /// <summary>
        /// Creates a new animation effect
        /// </summary>
        public AnimationEffect()
        {
            TimeStart = new ValueWrapper<double>() { Exists = false };
            TimeOffset = new ValueWrapper<double>() { Exists = false };
            TimeFinish = new ValueWrapper<double>() { Exists = false };
            TimeDuration = new ValueWrapper<double>() { Exists = false };
            Reverse = new ValueWrapper<bool>();
            Repeat = new ValueWrapper<RepeatMode>();
            Mode = new ValueWrapper<AnimationMode>();
            Async = new ValueWrapper<bool>();
            Enable = new ValueWrapper<bool>() { Value = true };
        }

        /// <summary>
        /// Convert this effect to its XML representation
        /// </summary>
        /// <param name="xml">The target XML document</param>
        /// <param name="dict">The dictionary for variable name support</param>
        /// <returns>The exported XML node</returns>
        public abstract XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict);

        /// <summary>
        /// Get a list of all used variables
        /// </summary>
        /// <returns>list of used variables</returns>
        public abstract IValueWrapper[] GetValueWrapper();

        /// <summary>
        /// Util function to add a parameter to the XML node
        /// </summary>
        /// <param name="xml">the target XML document</param>
        /// <param name="node">the current XML node</param>
        /// <param name="param">value to export</param>
        /// <param name="name">name of value</param>
        /// <param name="dict">Dictionary for variable name support</param>
        protected void AddParamToXml(XmlDocument xml, XmlNode node, IValueWrapper param, string name, PrototypeExporter.Dict dict)
        {
            if (param.Exists)
                node.Attributes.Append(xml.CreateAttribute(name)).Value = PrototypeExporter.GetParamValue(param, dict);
        }

        /// <summary>
        /// Add its own parameters to the xml node
        /// </summary>
        /// <param name="xml">the target XML document</param>
        /// <param name="node">the current XML node</param>
        /// <param name="dict">Dictionary for variable name support</param>
        protected virtual void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            AddParamToXml(xml, node, TimeStart, "time-start", dict);
            AddParamToXml(xml, node, TimeOffset, "time-offset", dict);
            AddParamToXml(xml, node, TimeFinish, "time-finish", dict);
            AddParamToXml(xml, node, TimeDuration, "time-duration", dict);
            AddParamToXml(xml, node, Reverse, "reverse", dict);
            AddParamToXml(xml, node, Repeat, "repeat", dict);
            AddParamToXml(xml, node, Async, "async", dict);
            AddParamToXml(xml, node, Mode, "mode", dict);
            AddParamToXml(xml, node, Enable, "enable", dict);
        }

        /// <summary>
        /// Apply the timing function to the progress
        /// </summary>
        /// <param name="value">The progress of execution (between 0 and 1)</param>
        /// <param name="mode">the timing function</param>
        /// <returns>The modified progress value</returns>
        public static double PerformMode(double value, AnimationMode mode)
        {
            if (value <= 0) return 0;
            if (value >= 1) return 1;
            switch (mode)
            {
                case AnimationMode.SwingIn: return value * value;
                case AnimationMode.SwingOut: return value * (2 - value);
                case AnimationMode.Swing: return value < 0.5 ? 2 * value * value : 2 * value * (2 - value) - 1;
                case AnimationMode.Focus:
                    {
                        var x = 2 * value - 1;
                        return 0.5 * x * x * x + 0.5;
                    }
                case AnimationMode.Jump: return value < 0.5 ? 0 : 1;

                case AnimationMode.Linear:
                default: return value;
            }
        }

        /// <summary>
        /// Check if the effect could start
        /// </summary>
        /// <param name="data">runtime information</param>
        public virtual void CanStartAnimation(AnimationRuntimeData data)
        {
            if (TimeStart.Exists)
            {
                if (TimeStart.Value <= data.CurrentTime)
                {
                    data.CanAnimate = true;
                    data.StartTime = TimeStart.Value;
                    if (Enable) StartAnimate(data.Runtime);
                }
                else data.CanAnimate = false;
            }
            else if (TimeOffset.Exists)
            {
                if (TimeOffset.Value <= data.CurrentTime - data.LastTileTime)
                {
                    data.CanAnimate = true;
                    data.StartTime = data.LastTileTime + TimeOffset.Value;
                    if (Enable) StartAnimate(data.Runtime);
                }
                else data.CanAnimate = false;
            }
            else
            {
                data.CanAnimate = true;
                data.StartTime = data.CurrentTime;
                if (Enable) StartAnimate(data.Runtime);
            }
        }

        /// <summary>
        /// Animate the current effect
        /// </summary>
        /// <param name="data">runtime data</param>
        public void Animate(AnimationRuntimeData data)
        {
            var time = data.CurrentTime - data.StartTime;
            double final;
            if (TimeFinish.Exists)
            {
                if (TimeFinish.Value <= data.CurrentTime)
                {
                    data.LastTileTime = TimeFinish.Value;
                    data.CanAnimate = false;
                }
                final = TimeFinish.Value - TimeStart.Value;
            }
            else if (TimeDuration.Exists)
            {
                if (TimeDuration.Value <= data.CurrentTime - data.StartTime)
                {
                    data.LastTileTime = data.StartTime + TimeDuration.Value;
                    data.CanAnimate = false;
                }
                final = TimeDuration.Value;
            }
            else
            {
                data.LastTileTime = data.StartTime;
                data.CanAnimate = false;
                final = 0;
            }
            if (Enable) Animate(final == 0 ? 1 : time / final);
        }

        /// <summary>
        /// Animate the current effect with the current progress
        /// </summary>
        /// <param name="time">the current progress (between 0 and 1)</param>
        protected abstract void Animate(double time);

        /// <summary>
        /// inform this effect that its execution whould start now
        /// </summary>
        /// <param name="runtime">current animation runtime</param>
        protected abstract void StartAnimate(AnimationRuntime runtime); //store start value if neccesary

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public virtual void MoveTargets(PrototypeFlattenerHelper helper)
        {
            TimeStart = helper.Convert(TimeStart);
            TimeOffset = helper.Convert(TimeOffset);
            TimeFinish = helper.Convert(TimeFinish);
            TimeDuration = helper.Convert(TimeDuration);
            Reverse = helper.Convert(Reverse);
            Mode = helper.Convert(Mode);
            Async = helper.Convert(Async);
            Enable = helper.Convert(Enable);
        }

        /// <summary>
        /// Clones this effect completly.
        /// </summary>
        /// <returns>The clone</returns>
        public AnimationEffect Clone()
        {
            var eff = ProtClone();
            eff.TimeStart = (ValueWrapper<double>)TimeStart.Clone();
            eff.TimeOffset = (ValueWrapper<double>)TimeOffset.Clone();
            eff.TimeFinish = (ValueWrapper<double>)TimeFinish.Clone();
            eff.TimeDuration = (ValueWrapper<double>)TimeDuration.Clone();
            eff.Reverse = (ValueWrapper<bool>)Reverse.Clone();
            eff.Mode = (ValueWrapper<AnimationMode>)Mode.Clone();
            eff.Async = (ValueWrapper<bool>)Async.Clone();
            eff.Enable = (ValueWrapper<bool>)Enable.Clone();
            return eff;
        }

        /// <summary>
        /// Clones the added data of this effect
        /// </summary>
        /// <returns>the clone</returns>
        protected abstract AnimationEffect ProtClone();
    }

    /// <summary>
    /// Define how often an effect should be repeated
    /// </summary>
    public struct RepeatMode
    {
        /// <summary>
        /// the number of times
        /// </summary>
        public uint Times { get; set; }

        /// <summary>
        /// The effect should be infinity repeated
        /// </summary>
        public bool Infinite { get => Times == uint.MaxValue; set => Times = uint.MaxValue; }

        /// <summary>
        /// This effect should never be repeated
        /// </summary>
        public bool None { get => Times == 0; set => Times = 0; }

        /// <summary>
        /// Create RepeatMode, it defines how often an effect should be repeated
        /// </summary>
        /// <param name="times">
        /// the amount of times this effect should be repeated (<see cref="uint.MaxValue"/> means 
        /// infinite repeating)
        /// </param>
        public RepeatMode(uint times)
        {
            Times = times;
        }

        /// <summary>
        /// converts the Value to a string
        /// </summary>
        /// <returns>the resulted string</returns>
        public override string ToString()
        {
            return None ? "none" : Infinite ? "infinite" : Times.ToString();
        }
    }

    /// <summary>
    /// Animates a double value
    /// </summary>
    public class ADouble : AnimationEffect
    {
        /// <summary>
        /// The variable that should be changed
        /// </summary>
        public ValueWrapper<double> Target { get; set; }

        /// <summary>
        /// The value of the variable at the start of this animation
        /// </summary>
        public ValueWrapper<double> ValueStart { get; set; }

        /// <summary>
        /// The amount this variable should be changed
        /// </summary>
        public ValueWrapper<double> ValueChange { get; set; }

        /// <summary>
        /// The value of the variable at the end of this animation
        /// </summary>
        public ValueWrapper<double> ValueFinish { get; set; }

        /// <summary>
        /// Animates a double value
        /// </summary>
        public ADouble()
        {
            Target = new ValueWrapper<double>();
            ValueStart = new ValueWrapper<double>() { Exists = false };
            ValueChange = new ValueWrapper<double>() { Exists = false };
            ValueFinish = new ValueWrapper<double>() { Exists = false };
        }

        /// <summary>
        /// Clones the added data of this effect
        /// </summary>
        /// <returns>the clone</returns>
        protected override AnimationEffect ProtClone()
        {
            var eff = new ADouble();
            eff.Target = Target;
            eff.ValueStart = (ValueWrapper<double>)ValueStart.Clone();
            eff.ValueChange = (ValueWrapper<double>)ValueChange.Clone();
            eff.ValueFinish = (ValueWrapper<double>)ValueFinish.Clone();
            return eff;
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            ValueStart = helper.Convert(ValueStart);
            ValueChange = helper.Convert(ValueChange);
            ValueFinish = helper.Convert(ValueFinish);
        }

        /// <summary>
        /// Convert this effect to its XML representation
        /// </summary>
        /// <param name="xml">The target XML document</param>
        /// <param name="dict">The dictionary for variable name support</param>
        /// <returns>The exported XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("ADouble");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        /// <summary>
        /// Add its own parameters to the xml node
        /// </summary>
        /// <param name="xml">the target XML document</param>
        /// <param name="node">the current XML node</param>
        /// <param name="dict">Dictionary for variable name support</param>
        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Target, "target-id", dict);
            AddParamToXml(xml, node, ValueStart, "value-start", dict);
            AddParamToXml(xml, node, ValueChange, "value-change", dict);
            AddParamToXml(xml, node, ValueFinish, "value-finish", dict);
        }

        /// <summary>
        /// Get a list of all used variables
        /// </summary>
        /// <returns>list of used variables</returns>
        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Target, ValueStart, ValueChange, ValueFinish
            };
        }

        double startValue;

        /// <summary>
        /// Animate the current effect with the current progress
        /// </summary>
        /// <param name="time">the current progress (between 0 and 1)</param>
        protected override void Animate(double time)
        {
            double finish;
            if (ValueFinish.Exists) finish = ValueFinish.Value;
            else if (ValueChange.Exists) finish = startValue + ValueChange.Value;
            else finish = startValue;
            Target.Value = startValue + (finish - startValue) * PerformMode(time, Mode);
        }

        /// <summary>
        /// inform this effect that its execution whould start now
        /// </summary>
        /// <param name="runtime">current animation runtime</param>
        protected override void StartAnimate(AnimationRuntime runtime)
        {
            if (ValueStart.Exists)
                startValue = ValueStart.Value;
            else startValue = Target.Value;
        }
    }

    /// <summary>
    /// Animate an int value
    /// </summary>
    public class AInt : AnimationEffect
    {
        /// <summary>
        /// The variable that should be changed
        /// </summary>
        public ValueWrapper<int> Target { get; set; }

        /// <summary>
        /// The value of the variable at the start of this animation
        /// </summary>
        public ValueWrapper<int> ValueStart { get; set; }

        /// <summary>
        /// The amount this variable should be changed
        /// </summary>
        public ValueWrapper<int> ValueChange { get; set; }

        /// <summary>
        /// The value of the variable at the end of this animation
        /// </summary>
        public ValueWrapper<int> ValueFinish { get; set; }

        /// <summary>
        /// Animate an int value
        /// </summary>
        public AInt()
        {
            Target = new ValueWrapper<int>();
            ValueStart = new ValueWrapper<int>() { Exists = false };
            ValueChange = new ValueWrapper<int>() { Exists = false };
            ValueFinish = new ValueWrapper<int>() { Exists = false };
        }

        /// <summary>
        /// Clones the added data of this effect
        /// </summary>
        /// <returns>the clone</returns>
        protected override AnimationEffect ProtClone()
        {
            var eff = new AInt();
            eff.Target = Target;
            eff.ValueStart = (ValueWrapper<int>)ValueStart.Clone();
            eff.ValueChange = (ValueWrapper<int>)ValueChange.Clone();
            eff.ValueFinish = (ValueWrapper<int>)ValueFinish.Clone();
            return eff;
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            ValueStart = helper.Convert(ValueStart);
            ValueChange = helper.Convert(ValueChange);
            ValueFinish = helper.Convert(ValueFinish);
        }

        /// <summary>
        /// Convert this effect to its XML representation
        /// </summary>
        /// <param name="xml">The target XML document</param>
        /// <param name="dict">The dictionary for variable name support</param>
        /// <returns>The exported XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("AInt");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        /// <summary>
        /// Add its own parameters to the xml node
        /// </summary>
        /// <param name="xml">the target XML document</param>
        /// <param name="node">the current XML node</param>
        /// <param name="dict">Dictionary for variable name support</param>
        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Target, "target-id", dict);
            AddParamToXml(xml, node, ValueStart, "value-start", dict);
            AddParamToXml(xml, node, ValueChange, "value-change", dict);
            AddParamToXml(xml, node, ValueFinish, "value-finish", dict);
        }

        /// <summary>
        /// Get a list of all used variables
        /// </summary>
        /// <returns>list of used variables</returns>
        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Target, ValueStart, ValueChange, ValueFinish
            };
        }

        double startValue;

        /// <summary>
        /// Animate the current effect with the current progress
        /// </summary>
        /// <param name="time">the current progress (between 0 and 1)</param>
        protected override void Animate(double time)
        {
            double finish;
            if (ValueFinish.Exists) finish = ValueFinish.Value;
            else if (ValueChange.Exists) finish = startValue + ValueChange.Value;
            else finish = startValue;
            Target.Value = (int)(startValue + (finish - startValue) * PerformMode(time, Mode));
        }

        /// <summary>
        /// inform this effect that its execution whould start now
        /// </summary>
        /// <param name="runtime">current animation runtime</param>
        protected override void StartAnimate(AnimationRuntime runtime)
        {
            if (ValueStart.Exists)
                startValue = ValueStart.Value;
            else startValue = Target.Value;
        }
    }

    /// <summary>
    /// Animate a color
    /// </summary>
    public class AColor : AnimationEffect
    {
        /// <summary>
        /// The variable that should be changed
        /// </summary>
        public ValueWrapper<Color> Target { get; set; }

        /// <summary>
        /// The value of the variable at the start of this animation
        /// </summary>
        public ValueWrapper<Color> ValueStart { get; set; }

        /// <summary>
        /// The value of the variable at the end of this animation
        /// </summary>
        public ValueWrapper<Color> ValueFinish { get; set; }

        /// <summary>
        /// Animate a color
        /// </summary>
        public AColor()
        {
            Target = new ValueWrapper<Color>();
            ValueStart = new ValueWrapper<Color>() { Exists = false };
            ValueFinish = new ValueWrapper<Color>() { Exists = false };
        }

        /// <summary>
        /// Clones the added data of this effect
        /// </summary>
        /// <returns>the clone</returns>
        protected override AnimationEffect ProtClone()
        {
            var eff = new AColor();
            eff.Target = Target;
            eff.ValueStart = (ValueWrapper<Color>)ValueStart.Clone();
            eff.ValueFinish = (ValueWrapper<Color>)ValueFinish.Clone();
            return eff;
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            ValueStart = helper.Convert(ValueStart);
            ValueFinish = helper.Convert(ValueFinish);
        }

        /// <summary>
        /// Convert this effect to its XML representation
        /// </summary>
        /// <param name="xml">The target XML document</param>
        /// <param name="dict">The dictionary for variable name support</param>
        /// <returns>The exported XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("AColor");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        /// <summary>
        /// Add its own parameters to the xml node
        /// </summary>
        /// <param name="xml">the target XML document</param>
        /// <param name="node">the current XML node</param>
        /// <param name="dict">Dictionary for variable name support</param>
        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Target, "target-id", dict);
            AddParamToXml(xml, node, ValueStart, "value-start", dict);
            AddParamToXml(xml, node, ValueFinish, "value-finish", dict);
        }

        /// <summary>
        /// Get a list of all used variables
        /// </summary>
        /// <returns>list of used variables</returns>
        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Target, ValueStart,ValueFinish
            };
        }

        Color startValue;

        /// <summary>
        /// Animate the current effect with the current progress
        /// </summary>
        /// <param name="time">the current progress (between 0 and 1)</param>
        protected override void Animate(double time)
        {
            Color finish;
            if (ValueFinish.Exists) finish = ValueFinish.Value;
            else finish = startValue;
            var t = PerformMode(time, Mode);
            Target.Value = Color.FromArgb(
                (int)Math.Max(0, Math.Min(255, startValue.A + (finish.A - startValue.A) * t)),
                (int)Math.Max(0, Math.Min(255, startValue.R + (finish.R - startValue.R) * t)),
                (int)Math.Max(0, Math.Min(255, startValue.G + (finish.G - startValue.G) * t)),
                (int)Math.Max(0, Math.Min(255, startValue.B + (finish.B - startValue.B) * t))
            );
        }

        /// <summary>
        /// inform this effect that its execution whould start now
        /// </summary>
        /// <param name="runtime">current animation runtime</param>
        protected override void StartAnimate(AnimationRuntime runtime)
        {
            if (ValueStart.Exists)
                startValue = ValueStart.Value;
            else startValue = Target.Value;
        }
    }

    /// <summary>
    /// Animate a position on the screen
    /// </summary>
    public class AScreenPos : AnimationEffect
    {
        /// <summary>
        /// The variable that should be changed
        /// </summary>
        public ValueWrapper<ScreenPos> Target { get; set; }

        /// <summary>
        /// The value of the variable at the start of this animation
        /// </summary>
        public ValueWrapper<ScreenPos> ValueStart { get; set; }

        /// <summary>
        /// The amount this variable should be changed
        /// </summary>
        public ValueWrapper<ScreenPos> ValueChange { get; set; }

        /// <summary>
        /// The value of the variable at the end of this animation
        /// </summary>
        public ValueWrapper<ScreenPos> ValueFinish { get; set; }

        /// <summary>
        /// Animate a position on the screen
        /// </summary>
        public AScreenPos()
        {
            Target = new ValueWrapper<ScreenPos>();
            ValueStart = new ValueWrapper<ScreenPos>() { Exists = false };
            ValueChange = new ValueWrapper<ScreenPos>() { Exists = false };
            ValueFinish = new ValueWrapper<ScreenPos>() { Exists = false };
        }

        /// <summary>
        /// Clones the added data of this effect
        /// </summary>
        /// <returns>the clone</returns>
        protected override AnimationEffect ProtClone()
        {
            var eff = new AScreenPos();
            eff.Target = Target;
            eff.ValueStart = (ValueWrapper<ScreenPos>)ValueStart.Clone();
            eff.ValueChange = (ValueWrapper<ScreenPos>)ValueChange.Clone();
            eff.ValueFinish = (ValueWrapper<ScreenPos>)ValueFinish.Clone();
            return eff;
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            ValueStart = helper.Convert(ValueStart);
            ValueChange = helper.Convert(ValueChange);
            ValueFinish = helper.Convert(ValueFinish);
        }

        /// <summary>
        /// Convert this effect to its XML representation
        /// </summary>
        /// <param name="xml">The target XML document</param>
        /// <param name="dict">The dictionary for variable name support</param>
        /// <returns>The exported XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("AScreenPos");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        /// <summary>
        /// Add its own parameters to the xml node
        /// </summary>
        /// <param name="xml">the target XML document</param>
        /// <param name="node">the current XML node</param>
        /// <param name="dict">Dictionary for variable name support</param>
        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Target, "target-id", dict);
            AddParamToXml(xml, node, ValueStart, "value-start", dict);
            AddParamToXml(xml, node, ValueChange, "value-change", dict);
            AddParamToXml(xml, node, ValueFinish, "value-finish", dict);
        }

        /// <summary>
        /// Get a list of all used variables
        /// </summary>
        /// <returns>list of used variables</returns>
        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Target, ValueStart, ValueChange, ValueFinish
            };
        }

        ScreenPos startValue;

        /// <summary>
        /// Animate the current effect with the current progress
        /// </summary>
        /// <param name="time">the current progress (between 0 and 1)</param>
        protected override void Animate(double time)
        {
            ScreenPos finish;
            if (ValueFinish.Exists) finish = ValueFinish.Value;
            else if (ValueChange.Exists)
            {
                if (startValue.PosType == ValueChange.Value.PosType)
                    finish = new ScreenPos(startValue.Value + ValueChange.Value.Value, startValue.PosType);
                else finish = startValue;
            }
            else finish = startValue;
            if (finish.PosType != startValue.PosType)
                Target.Value = finish;
            else Target.Value = new ScreenPos(startValue.Value + (finish.Value - startValue.Value) * PerformMode(time, Mode), startValue.PosType);
        }

        /// <summary>
        /// inform this effect that its execution whould start now
        /// </summary>
        /// <param name="runtime">current animation runtime</param>
        protected override void StartAnimate(AnimationRuntime runtime)
        {
            if (ValueStart.Exists)
                startValue = ValueStart.Value;
            else startValue = Target.Value;
        }
    }

    /// <summary>
    /// Animate a boolean value
    /// </summary>
    public class ABool : AnimationEffect
    {
        /// <summary>
        /// The variable that should be changed
        /// </summary>
        public ValueWrapper<bool> Target { get; set; }

        /// <summary>
        /// The value of the variable at the start of this animation
        /// </summary>
        public ValueWrapper<bool> ValueStart { get; set; }

        /// <summary>
        /// The value of the variable at the end of this animation
        /// </summary>
        public ValueWrapper<bool> ValueFinish { get; set; }

        /// <summary>
        /// Animate a boolean value
        /// </summary>
        public ABool()
        {
            Target = new ValueWrapper<bool>();
            ValueStart = new ValueWrapper<bool>() { Exists = false };
            ValueFinish = new ValueWrapper<bool>() { Exists = false };
        }

        /// <summary>
        /// Clones the added data of this effect
        /// </summary>
        /// <returns>the clone</returns>
        protected override AnimationEffect ProtClone()
        {
            var eff = new ABool();
            eff.Target = Target;
            eff.ValueStart = (ValueWrapper<bool>)ValueStart.Clone();
            eff.ValueFinish = (ValueWrapper<bool>)ValueFinish.Clone();
            return eff;
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            ValueStart = helper.Convert(ValueStart);
            ValueFinish = helper.Convert(ValueFinish);
        }

        /// <summary>
        /// Convert this effect to its XML representation
        /// </summary>
        /// <param name="xml">The target XML document</param>
        /// <param name="dict">The dictionary for variable name support</param>
        /// <returns>The exported XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("ABool");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        /// <summary>
        /// Add its own parameters to the xml node
        /// </summary>
        /// <param name="xml">the target XML document</param>
        /// <param name="node">the current XML node</param>
        /// <param name="dict">Dictionary for variable name support</param>
        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Target, "target-id", dict);
            AddParamToXml(xml, node, ValueStart, "value-start", dict);
            AddParamToXml(xml, node, ValueFinish, "value-finish", dict);
        }

        /// <summary>
        /// Get a list of all used variables
        /// </summary>
        /// <returns>list of used variables</returns>
        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Target, ValueStart, ValueFinish
            };
        }

        /// <summary>
        /// Animate the current effect with the current progress
        /// </summary>
        /// <param name="time">the current progress (between 0 and 1)</param>
        protected override void Animate(double time)
        {
            var finish = ValueFinish.Exists ? ValueFinish.Value : startValue;
            Target.Value = PerformMode(time, Mode) >= 0.5 ? finish : startValue;
        }

        bool startValue;
        /// <summary>
        /// inform this effect that its execution whould start now
        /// </summary>
        /// <param name="runtime">current animation runtime</param>
        protected override void StartAnimate(AnimationRuntime runtime)
        {
            if (ValueStart.Exists)
                startValue = ValueStart.Value;
            else startValue = Target.Value;
        }
    }

    /// <summary>
    /// Animate a string
    /// </summary>
    public class AString : AnimationEffect
    {
        /// <summary>
        /// The variable that should be changed
        /// </summary>
        public ValueWrapper<string> Target { get; set; }

        /// <summary>
        /// The value of the variable at the start of this animation
        /// </summary>
        public ValueWrapper<string> ValueStart { get; set; }

        /// <summary>
        /// The value of the variable at the end of this animation
        /// </summary>
        public ValueWrapper<string> ValueFinish { get; set; }

        /// <summary>
        /// If flipped the string was changed from right to left. Normaly it
        /// would be replaced from left to right
        /// </summary>
        public ValueWrapper<bool> Flip { get; set; }

        /// <summary>
        /// Animate a string
        /// </summary>
        public AString()
        {
            Target = new ValueWrapper<string>();
            ValueStart = new ValueWrapper<string>() { Exists = false };
            ValueFinish = new ValueWrapper<string>() { Exists = false };
            Flip = new ValueWrapper<bool>();
        }

        /// <summary>
        /// Clones the added data of this effect
        /// </summary>
        /// <returns>the clone</returns>
        protected override AnimationEffect ProtClone()
        {
            var eff = new AString();
            eff.Target = Target;
            eff.ValueStart = (ValueWrapper<string>)ValueStart.Clone();
            eff.ValueFinish = (ValueWrapper<string>)ValueFinish.Clone();
            eff.Flip = (ValueWrapper<bool>)Flip.Clone();
            return eff;
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            ValueStart = helper.Convert(ValueStart);
            ValueFinish = helper.Convert(ValueFinish);
            Flip = helper.Convert(Flip);
        }

        /// <summary>
        /// Convert this effect to its XML representation
        /// </summary>
        /// <param name="xml">The target XML document</param>
        /// <param name="dict">The dictionary for variable name support</param>
        /// <returns>The exported XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("AString");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        /// <summary>
        /// Add its own parameters to the xml node
        /// </summary>
        /// <param name="xml">the target XML document</param>
        /// <param name="node">the current XML node</param>
        /// <param name="dict">Dictionary for variable name support</param>
        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Target, "target-id", dict);
            AddParamToXml(xml, node, ValueStart, "value-start", dict);
            AddParamToXml(xml, node, ValueFinish, "value-finish", dict);
            AddParamToXml(xml, node, Flip, "flip", dict);
        }

        /// <summary>
        /// Get a list of all used variables
        /// </summary>
        /// <returns>list of used variables</returns>
        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Target, ValueStart, ValueFinish, Flip
            };
        }

        /// <summary>
        /// Animate the current effect with the current progress
        /// </summary>
        /// <param name="time">the current progress (between 0 and 1)</param>
        protected override void Animate(double time)
        {
            var finish = ValueFinish.Exists ? ValueFinish.Value : startValue;
            time = PerformMode(time, Mode);
            if (time >= 1) Target.Value = finish;
            else if (time <= 0) Target.Value = startValue;
            else
            {
                var len = Math.Max(startValue.Length, finish.Length);
                var sb = new StringBuilder(len);
                var pos = (int)(time * len);
                for (int i = 0; i < pos; ++i)
                    sb.Append(Flip.Value ? 
                        i < startValue.Length ? startValue[i] : ' ' : 
                        i < finish.Length ? finish[i] : ' ');
                for (int i = pos; i < len; ++i)
                    sb.Append(Flip.Value ?
                        i< finish.Length ? finish[i] : ' ' :
                        i < startValue.Length ? startValue[i] : ' ');
                Target.Value = sb.ToString();
            }
        }

        string startValue;
        /// <summary>
        /// inform this effect that its execution whould start now
        /// </summary>
        /// <param name="runtime">current animation runtime</param>
        protected override void StartAnimate(AnimationRuntime runtime)
        {
            if (ValueStart.Exists)
                startValue = ValueStart.Value;
            else startValue = Target.Value;
        }
    }

    /// <summary>
    /// Call an another animation
    /// </summary>
    public class Call : AnimationEffect
    {
        /// <summary>
        /// The animation that should be called
        /// </summary>
        public AnimationGroup Target { get; set; }

        /// <summary>
        /// The timing multipler this animation should be called
        /// </summary>
        public ValueWrapper<double> Timing { get; set; }

        /// <summary>
        /// Call an another animation
        /// </summary>
        public Call()
        {
            Timing = new ValueWrapper<double>();
        }

        /// <summary>
        /// Clones the added data of this effect
        /// </summary>
        /// <returns>the clone</returns>
        protected override AnimationEffect ProtClone()
        {
            var eff = new Call();
            eff.Target = Target;
            eff.Timing = (ValueWrapper<double>)Timing.Clone();
            return eff;
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            Timing = helper.Convert(Timing);
        }

        /// <summary>
        /// Convert this effect to its XML representation
        /// </summary>
        /// <param name="xml">The target XML document</param>
        /// <param name="dict">The dictionary for variable name support</param>
        /// <returns>The exported XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("Call");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        /// <summary>
        /// Add its own parameters to the xml node
        /// </summary>
        /// <param name="xml">the target XML document</param>
        /// <param name="node">the current XML node</param>
        /// <param name="dict">Dictionary for variable name support</param>
        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            node.Attributes.Append(xml.CreateAttribute("target-id")).Value = "#" + dict.Groups[Target].ToString();
            AddParamToXml(xml, node, Async, "async", dict);
            AddParamToXml(xml, node, Timing, "timing", dict);
        }

        /// <summary>
        /// Get a list of all used variables
        /// </summary>
        /// <returns>list of used variables</returns>
        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Enable,
                Async, Timing
            };
        }

        /// <summary>
        /// Animate the current effect with the current progress
        /// </summary>
        /// <param name="time">the current progress (between 0 and 1)</param>
        protected override void Animate(double time)
        {
        }

        /// <summary>
        /// inform this effect that its execution whould start now
        /// </summary>
        /// <param name="runtime">current animation runtime</param>
        protected override void StartAnimate(AnimationRuntime runtime)
        {
            runtime.ExecuteAnimation(Target, Timing.Value);
        }
    }

    /// <summary>
    /// Set the current phase of the ui
    /// </summary>
    public class SetState : AnimationEffect
    {
        /// <summary>
        /// The target state of the ui
        /// </summary>
        public ValueWrapper<Status> State { get; set; }

        /// <summary>
        /// Set the current phase of the ui
        /// </summary>
        public SetState()
        {
            State = new ValueWrapper<Status>();
        }

        /// <summary>
        /// Clones the added data of this effect
        /// </summary>
        /// <returns>the clone</returns>
        protected override AnimationEffect ProtClone()
        {
            var eff = new SetState();
            eff.State = (ValueWrapper<Status>)State.Clone();
            return eff;
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            State = helper.Convert(State);
        }

        /// <summary>
        /// Convert this effect to its XML representation
        /// </summary>
        /// <param name="xml">The target XML document</param>
        /// <param name="dict">The dictionary for variable name support</param>
        /// <returns>The exported XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("SetState");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        /// <summary>
        /// Add its own parameters to the xml node
        /// </summary>
        /// <param name="xml">the target XML document</param>
        /// <param name="node">the current XML node</param>
        /// <param name="dict">Dictionary for variable name support</param>
        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, State, "state", dict);
        }

        /// <summary>
        /// Get a list of all used variables
        /// </summary>
        /// <returns>list of used variables</returns>
        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                State
            };
        }

        /// <summary>
        /// Animate the current effect with the current progress
        /// </summary>
        /// <param name="time">the current progress (between 0 and 1)</param>
        protected override void Animate(double time)
        {
            
        }

        /// <summary>
        /// inform this effect that its execution whould start now
        /// </summary>
        /// <param name="runtime">current animation runtime</param>
        protected override void StartAnimate(AnimationRuntime runtime)
        {
            runtime.CurrentStatus = State.Value;
        }
    }

    /// <summary>
    /// Execute a registered action
    /// </summary>
    public class AnimAction : AnimationEffect
    {
        /// <summary>
        /// The name of the animation that should be called if the effect starts
        /// </summary>
        public ValueWrapper<String> Name { get; set; }

        /// <summary>
        /// Execute a registered action
        /// </summary>
        public AnimAction()
        {
            Name = new ValueWrapper<string>();
        }

        /// <summary>
        /// Clones the added data of this effect
        /// </summary>
        /// <returns>the clone</returns>
        protected override AnimationEffect ProtClone()
        {
            var eff = new AnimAction();
            eff.Name = (ValueWrapper<string>)Name.Clone();
            return eff;
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Name = helper.Convert(Name);
        }

        /// <summary>
        /// Convert this effect to its XML representation
        /// </summary>
        /// <param name="xml">The target XML document</param>
        /// <param name="dict">The dictionary for variable name support</param>
        /// <returns>The exported XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("AnimAction");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        /// <summary>
        /// Add its own parameters to the xml node
        /// </summary>
        /// <param name="xml">the target XML document</param>
        /// <param name="node">the current XML node</param>
        /// <param name="dict">Dictionary for variable name support</param>
        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Name, "name", dict);
        }

        /// <summary>
        /// Get a list of all used variables
        /// </summary>
        /// <returns>list of used variables</returns>
        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
               {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Name
               };
        }

        /// <summary>
        /// Animate the current effect with the current progress
        /// </summary>
        /// <param name="time">the current progress (between 0 and 1)</param>
        protected override void Animate(double time)
        {
        }

        /// <summary>
        /// inform this effect that its execution whould start now
        /// </summary>
        /// <param name="runtime">current animation runtime</param>
        protected override void StartAnimate(AnimationRuntime runtime)
        {
            var name = Name.Value;
            if (actions.ContainsKey(name))
                new Task(actions[name]).Start();
        }

        /// <summary>
        /// List of all registred actions
        /// </summary>
        static Dictionary<string, Action> actions = new Dictionary<string, Action>();

        /// <summary>
        /// Registers an action that the ui can call if something happens
        /// </summary>
        /// <param name="name">the name of the action</param>
        /// <param name="action">the method that would be called</param>
        public static void AddAction(string name, Action action)
        {
            actions[name] = action;
        }
    }

    /// <summary>
    /// Play a sound file
    /// </summary>
    public class PlaySound : AnimationEffect
    {
        /// <summary>
        /// The path of the specific file
        /// </summary>
        public ValueWrapper<String> File { get; set; }

        /// <summary>
        /// the volume of this sound (between 0 and 1)
        /// </summary>
        public ValueWrapper<double> Volume { get; set; }

        /// <summary>
        /// Play a sound file
        /// </summary>
        public PlaySound()
        {
            File = new ValueWrapper<string>();
            Volume = new ValueWrapper<double>();
            Volume.Value = 1;
        }

        /// <summary>
        /// Clones the added data of this effect
        /// </summary>
        /// <returns>the clone</returns>
        protected override AnimationEffect ProtClone()
        {
            var eff = new PlaySound();
            eff.File = (ValueWrapper<string>)File.Clone();
            eff.Volume = (ValueWrapper<double>)Volume.Clone();
            return eff;
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            File = helper.Convert(File);
            Volume = helper.Convert(Volume);
        }

        /// <summary>
        /// Convert this effect to its XML representation
        /// </summary>
        /// <param name="xml">The target XML document</param>
        /// <param name="dict">The dictionary for variable name support</param>
        /// <returns>The exported XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("Sound");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        /// <summary>
        /// Add its own parameters to the xml node
        /// </summary>
        /// <param name="xml">the target XML document</param>
        /// <param name="node">the current XML node</param>
        /// <param name="dict">Dictionary for variable name support</param>
        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, File, "file", dict);
            AddParamToXml(xml, node, Volume, "volume", dict);
        }

        /// <summary>
        /// Get a list of all used variables
        /// </summary>
        /// <returns>list of used variables</returns>
        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
               {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                File, Volume
               };
        }

        /// <summary>
        /// Animate the current effect with the current progress
        /// </summary>
        /// <param name="time">the current progress (between 0 and 1)</param>
        protected override void Animate(double time)
        {
        }

        /// <summary>
        /// inform this effect that its execution whould start now
        /// </summary>
        /// <param name="runtime">current animation runtime</param>
        protected override void StartAnimate(AnimationRuntime runtime)
        {
            var name = File.Value;
            if (runtime.SoundPlayer != null)
                new Task(() => runtime.SoundPlayer.PlaySound(File.Value, Volume.Value)).Start();
        }
    }
}
