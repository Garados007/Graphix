using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Graphix.Prototypes;
using Color = System.Drawing.Color;

namespace Graphix.Physic
{
    public abstract class AnimationEffect
    {
        public ValueWrapper<double> TimeStart { get; set; }

        public ValueWrapper<double> TimeOffset { get; set; }

        public ValueWrapper<double> TimeFinish { get; set; }

        public ValueWrapper<double> TimeDuration { get; set; }

        public ValueWrapper<bool> Reverse { get; set; }

        public ValueWrapper<RepeatMode> Repeat { get; set; }

        public ValueWrapper<AnimationMode> Mode { get; set; }

        public ValueWrapper<bool> Async { get; set; }

        public ValueWrapper<bool> Enable { get; set; }

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

        public abstract XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict);

        public abstract IValueWrapper[] GetValueWrapper();

        protected void AddParamToXml(XmlDocument xml, XmlNode node, IValueWrapper param, string name, PrototypeExporter.Dict dict)
        {
            if (param.Exists)
                node.Attributes.Append(xml.CreateAttribute(name)).Value = PrototypeExporter.GetParamValue(param, dict);
        }

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

        protected abstract void Animate(double time);

        protected abstract void StartAnimate(AnimationRuntime runtime); //store start value if neccesary

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

        protected abstract AnimationEffect ProtClone();
    }

    public struct RepeatMode
    {
        public uint Times { get; set; }

        public bool Infinite { get => Times == uint.MaxValue; set => Times = uint.MaxValue; }

        public bool None { get => Times == 0; set => Times = 0; }

        public RepeatMode(uint times)
        {
            Times = times;
        }

        public override string ToString()
        {
            return None ? "none" : Infinite ? "infinite" : Times.ToString();
        }
    }

    public class ADouble : AnimationEffect
    {
        public ValueWrapper<double> Target { get; set; }

        public ValueWrapper<double> ValueStart { get; set; }

        public ValueWrapper<double> ValueChange { get; set; }

        public ValueWrapper<double> ValueFinish { get; set; }

        public ADouble()
        {
            Target = new ValueWrapper<double>();
            ValueStart = new ValueWrapper<double>() { Exists = false };
            ValueChange = new ValueWrapper<double>() { Exists = false };
            ValueFinish = new ValueWrapper<double>() { Exists = false };
        }

        protected override AnimationEffect ProtClone()
        {
            var eff = new ADouble();
            eff.Target = Target;
            eff.ValueStart = (ValueWrapper<double>)ValueStart.Clone();
            eff.ValueChange = (ValueWrapper<double>)ValueChange.Clone();
            eff.ValueFinish = (ValueWrapper<double>)ValueFinish.Clone();
            return eff;
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            ValueStart = helper.Convert(ValueStart);
            ValueChange = helper.Convert(ValueChange);
            ValueFinish = helper.Convert(ValueFinish);
        }

        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("ADouble");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Target, "target-id", dict);
            AddParamToXml(xml, node, ValueStart, "value-start", dict);
            AddParamToXml(xml, node, ValueChange, "value-change", dict);
            AddParamToXml(xml, node, ValueFinish, "value-finish", dict);
        }

        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Target, ValueStart, ValueChange, ValueFinish
            };
        }

        double startValue;

        protected override void Animate(double time)
        {
            double finish;
            if (ValueFinish.Exists) finish = ValueFinish.Value;
            else if (ValueChange.Exists) finish = startValue + ValueChange.Value;
            else finish = startValue;
            Target.Value = startValue + (finish - startValue) * PerformMode(time, Mode);
        }

        protected override void StartAnimate(AnimationRuntime runtime)
        {
            if (ValueStart.Exists)
                startValue = ValueStart.Value;
            else startValue = Target.Value;
        }
    }

    public class AInt : AnimationEffect
    {
        public ValueWrapper<int> Target { get; set; }

        public ValueWrapper<int> ValueStart { get; set; }

        public ValueWrapper<int> ValueChange { get; set; }

        public ValueWrapper<int> ValueFinish { get; set; }

        public AInt()
        {
            Target = new ValueWrapper<int>();
            ValueStart = new ValueWrapper<int>() { Exists = false };
            ValueChange = new ValueWrapper<int>() { Exists = false };
            ValueFinish = new ValueWrapper<int>() { Exists = false };
        }

        protected override AnimationEffect ProtClone()
        {
            var eff = new AInt();
            eff.Target = Target;
            eff.ValueStart = (ValueWrapper<int>)ValueStart.Clone();
            eff.ValueChange = (ValueWrapper<int>)ValueChange.Clone();
            eff.ValueFinish = (ValueWrapper<int>)ValueFinish.Clone();
            return eff;
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            ValueStart = helper.Convert(ValueStart);
            ValueChange = helper.Convert(ValueChange);
            ValueFinish = helper.Convert(ValueFinish);
        }

        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("AInt");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Target, "target-id", dict);
            AddParamToXml(xml, node, ValueStart, "value-start", dict);
            AddParamToXml(xml, node, ValueChange, "value-change", dict);
            AddParamToXml(xml, node, ValueFinish, "value-finish", dict);
        }

        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Target, ValueStart, ValueChange, ValueFinish
            };
        }

        double startValue;

        protected override void Animate(double time)
        {
            double finish;
            if (ValueFinish.Exists) finish = ValueFinish.Value;
            else if (ValueChange.Exists) finish = startValue + ValueChange.Value;
            else finish = startValue;
            Target.Value = (int)(startValue + (finish - startValue) * PerformMode(time, Mode));
        }

        protected override void StartAnimate(AnimationRuntime runtime)
        {
            if (ValueStart.Exists)
                startValue = ValueStart.Value;
            else startValue = Target.Value;
        }
    }

    public class AColor : AnimationEffect
    {
        public ValueWrapper<Color> Target { get; set; }

        public ValueWrapper<Color> ValueStart { get; set; }

        public ValueWrapper<Color> ValueFinish { get; set; }

        public AColor()
        {
            Target = new ValueWrapper<Color>();
            ValueStart = new ValueWrapper<Color>() { Exists = false };
            ValueFinish = new ValueWrapper<Color>() { Exists = false };
        }

        protected override AnimationEffect ProtClone()
        {
            var eff = new AColor();
            eff.Target = Target;
            eff.ValueStart = (ValueWrapper<Color>)ValueStart.Clone();
            eff.ValueFinish = (ValueWrapper<Color>)ValueFinish.Clone();
            return eff;
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            ValueStart = helper.Convert(ValueStart);
            ValueFinish = helper.Convert(ValueFinish);
        }

        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("AColor");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Target, "target-id", dict);
            AddParamToXml(xml, node, ValueStart, "value-start", dict);
            AddParamToXml(xml, node, ValueFinish, "value-finish", dict);
        }

        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Target, ValueStart,ValueFinish
            };
        }

        Color startValue;

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

        protected override void StartAnimate(AnimationRuntime runtime)
        {
            if (ValueStart.Exists)
                startValue = ValueStart.Value;
            else startValue = Target.Value;
        }
    }


    public class AScreenPos : AnimationEffect
    {
        public ValueWrapper<ScreenPos> Target { get; set; }

        public ValueWrapper<ScreenPos> ValueStart { get; set; }

        public ValueWrapper<ScreenPos> ValueChange { get; set; }

        public ValueWrapper<ScreenPos> ValueFinish { get; set; }

        public AScreenPos()
        {
            Target = new ValueWrapper<ScreenPos>();
            ValueStart = new ValueWrapper<ScreenPos>() { Exists = false };
            ValueChange = new ValueWrapper<ScreenPos>() { Exists = false };
            ValueFinish = new ValueWrapper<ScreenPos>() { Exists = false };
        }

        protected override AnimationEffect ProtClone()
        {
            var eff = new AScreenPos();
            eff.Target = Target;
            eff.ValueStart = (ValueWrapper<ScreenPos>)ValueStart.Clone();
            eff.ValueChange = (ValueWrapper<ScreenPos>)ValueChange.Clone();
            eff.ValueFinish = (ValueWrapper<ScreenPos>)ValueFinish.Clone();
            return eff;
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            ValueStart = helper.Convert(ValueStart);
            ValueChange = helper.Convert(ValueChange);
            ValueFinish = helper.Convert(ValueFinish);
        }

        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("AScreenPos");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Target, "target-id", dict);
            AddParamToXml(xml, node, ValueStart, "value-start", dict);
            AddParamToXml(xml, node, ValueChange, "value-change", dict);
            AddParamToXml(xml, node, ValueFinish, "value-finish", dict);
        }

        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Target, ValueStart, ValueChange, ValueFinish
            };
        }

        ScreenPos startValue;

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

        protected override void StartAnimate(AnimationRuntime runtime)
        {
            if (ValueStart.Exists)
                startValue = ValueStart.Value;
            else startValue = Target.Value;
        }
    }

    public class ABool : AnimationEffect
    {
        public ValueWrapper<bool> Target { get; set; }

        public ValueWrapper<bool> ValueStart { get; set; }
        
        public ValueWrapper<bool> ValueFinish { get; set; }

        public ABool()
        {
            Target = new ValueWrapper<bool>();
            ValueStart = new ValueWrapper<bool>() { Exists = false };
            ValueFinish = new ValueWrapper<bool>() { Exists = false };
        }

        protected override AnimationEffect ProtClone()
        {
            var eff = new ABool();
            eff.Target = Target;
            eff.ValueStart = (ValueWrapper<bool>)ValueStart.Clone();
            eff.ValueFinish = (ValueWrapper<bool>)ValueFinish.Clone();
            return eff;
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            ValueStart = helper.Convert(ValueStart);
            ValueFinish = helper.Convert(ValueFinish);
        }

        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("ABool");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Target, "target-id", dict);
            AddParamToXml(xml, node, ValueStart, "value-start", dict);
            AddParamToXml(xml, node, ValueFinish, "value-finish", dict);
        }

        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Target, ValueStart, ValueFinish
            };
        }

        protected override void Animate(double time)
        {
            var finish = ValueFinish.Exists ? ValueFinish.Value : startValue;
            Target.Value = PerformMode(time, Mode) >= 0.5 ? finish : startValue;
        }

        bool startValue;
        protected override void StartAnimate(AnimationRuntime runtime)
        {
            if (ValueStart.Exists)
                startValue = ValueStart.Value;
            else startValue = Target.Value;
        }
    }

    public class AString : AnimationEffect
    {
        public ValueWrapper<string> Target { get; set; }

        public ValueWrapper<string> ValueStart { get; set; }

        public ValueWrapper<string> ValueFinish { get; set; }

        public ValueWrapper<bool> Flip { get; set; }

        public AString()
        {
            Target = new ValueWrapper<string>();
            ValueStart = new ValueWrapper<string>() { Exists = false };
            ValueFinish = new ValueWrapper<string>() { Exists = false };
            Flip = new ValueWrapper<bool>();
        }

        protected override AnimationEffect ProtClone()
        {
            var eff = new AString();
            eff.Target = Target;
            eff.ValueStart = (ValueWrapper<string>)ValueStart.Clone();
            eff.ValueFinish = (ValueWrapper<string>)ValueFinish.Clone();
            eff.Flip = (ValueWrapper<bool>)Flip.Clone();
            return eff;
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            ValueStart = helper.Convert(ValueStart);
            ValueFinish = helper.Convert(ValueFinish);
            Flip = helper.Convert(Flip);
        }

        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("AString");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Target, "target-id", dict);
            AddParamToXml(xml, node, ValueStart, "value-start", dict);
            AddParamToXml(xml, node, ValueFinish, "value-finish", dict);
            AddParamToXml(xml, node, Flip, "flip", dict);
        }

        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Target, ValueStart, ValueFinish, Flip
            };
        }

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
        protected override void StartAnimate(AnimationRuntime runtime)
        {
            if (ValueStart.Exists)
                startValue = ValueStart.Value;
            else startValue = Target.Value;
        }
    }

    public class Call : AnimationEffect
    {
        public AnimationGroup Target { get; set; }

        public ValueWrapper<double> Timing { get; set; }

        public Call()
        {
            Timing = new ValueWrapper<double>();
        }

        protected override AnimationEffect ProtClone()
        {
            var eff = new Call();
            eff.Target = Target;
            eff.Timing = (ValueWrapper<double>)Timing.Clone();
            return eff;
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Target = helper.Convert(Target);
            Timing = helper.Convert(Timing);
        }

        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("Call");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            node.Attributes.Append(xml.CreateAttribute("target-id")).Value = "#" + dict.Groups[Target].ToString();
            AddParamToXml(xml, node, Async, "async", dict);
            AddParamToXml(xml, node, Timing, "timing", dict);
        }

        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Enable,
                Async, Timing
            };
        }

        protected override void Animate(double time)
        {
        }

        protected override void StartAnimate(AnimationRuntime runtime)
        {
            runtime.ExecuteAnimation(Target, Timing.Value);
        }
    }

    public class SetState : AnimationEffect
    {
        public ValueWrapper<Status> State { get; set; }

        public SetState()
        {
            State = new ValueWrapper<Status>();
        }

        protected override AnimationEffect ProtClone()
        {
            var eff = new SetState();
            eff.State = (ValueWrapper<Status>)State.Clone();
            return eff;
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            State = helper.Convert(State);
        }

        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("SetState");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, State, "state", dict);
        }

        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                State
            };
        }

        protected override void Animate(double time)
        {
            
        }

        protected override void StartAnimate(AnimationRuntime runtime)
        {
            runtime.CurrentStatus = State.Value;
        }
    }

    public class AnimAction : AnimationEffect
    {
        public ValueWrapper<String> Name { get; set; }

        public AnimAction()
        {
            Name = new ValueWrapper<string>();
        }

        protected override AnimationEffect ProtClone()
        {
            var eff = new AnimAction();
            eff.Name = (ValueWrapper<string>)Name.Clone();
            return eff;
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Name = helper.Convert(Name);
        }

        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("AnimAction");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, Name, "name", dict);
        }

        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
               {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                Name
               };
        }

        protected override void Animate(double time)
        {
        }

        protected override void StartAnimate(AnimationRuntime runtime)
        {
            var name = Name.Value;
            if (actions.ContainsKey(name))
                new Task(actions[name]).Start();
        }

        static Dictionary<string, Action> actions = new Dictionary<string, Action>();

        public static void AddAction(string name, Action action)
        {
            actions[name] = action;
        }
    }

    public class PlaySound : AnimationEffect
    {
        public ValueWrapper<String> File { get; set; }

        public ValueWrapper<double> Volume { get; set; }

        public PlaySound()
        {
            File = new ValueWrapper<string>();
            Volume = new ValueWrapper<double>();
            Volume.Value = 1;
        }

        protected override AnimationEffect ProtClone()
        {
            var eff = new PlaySound();
            eff.File = (ValueWrapper<string>)File.Clone();
            eff.Volume = (ValueWrapper<double>)Volume.Clone();
            return eff;
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            File = helper.Convert(File);
            Volume = helper.Convert(Volume);
        }

        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("Sound");
            AddParamsToXml(xml, node, dict);
            return node;
        }

        protected override void AddParamsToXml(XmlDocument xml, XmlNode node, PrototypeExporter.Dict dict)
        {
            base.AddParamsToXml(xml, node, dict);
            AddParamToXml(xml, node, File, "file", dict);
            AddParamToXml(xml, node, Volume, "volume", dict);
        }

        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
               {
                TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
                File, Volume
               };
        }

        protected override void Animate(double time)
        {
        }

        protected override void StartAnimate(AnimationRuntime runtime)
        {
            var name = File.Value;
            if (runtime.SoundPlayer != null)
                new Task(() => runtime.SoundPlayer.PlaySound(File.Value, Volume.Value)).Start();
        }
    }
}
