using Graphix.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Graphix.Physic
{
    /// <summary>
    /// Execute a single script
    /// </summary>
    public class ScriptEffect : AnimationEffect
    {
        /// <summary>
        /// The source code if you want to execute this. This function is normaly set
        /// by the ui xml code.
        /// </summary>
        public ValueWrapper<String> Code { get; set; }

        /// <summary>
        /// That is the function that would be called if this effect would be activated.
        /// This function is normaly set by the js runtime.
        /// </summary>
        public Action<AnimationRuntime, double> Function { get; set; }

        /// <summary>
        /// Execute a single script
        /// </summary>
        public ScriptEffect()
        {
            Code = new ValueWrapper<string>();
        }

        /// <summary>
        /// Clones the added data of this effect
        /// </summary>
        /// <returns>the clone</returns>
        protected override AnimationEffect ProtClone()
        {
            var eff = new ScriptEffect();
            eff.Code = (ValueWrapper<string>)Code.Clone();
            eff.Function = Function;
            return eff;
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            base.MoveTargets(helper);
            Code = helper.Convert(Code);
        }

        /// <summary>
        /// Convert this effect to its XML representation
        /// </summary>
        /// <param name="xml">The target XML document</param>
        /// <param name="dict">The dictionary for variable name support</param>
        /// <returns>The exported XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("Script");
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
            AddParamToXml(xml, node, Code, "code", dict);
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
                Code
               };
        }

        AnimationRuntime runtime;

        /// <summary>
        /// Animate the current effect with the current progress
        /// </summary>
        /// <param name="time">the current progress (between 0 and 1)</param>
        protected override void Animate(double time)
        {
            Function?.Invoke(runtime, time);
        }

        /// <summary>
        /// inform this effect that its execution whould start now
        /// </summary>
        /// <param name="runtime">current animation runtime</param>
        protected override void StartAnimate(AnimationRuntime runtime)
        {
            this.runtime = runtime;
            if (Function == null && Code.Exists)
            {
                var sb = new StringBuilder(Code.Value.Length + 31);
                sb.Append("var f=function(runtime,time){");
                sb.Append(Code.Value);
                sb.Append("};");
                var core = new Script.ScriptCore(runtime);
                core.engine.Execute(sb.ToString());
                var value = core.engine.GetValue("f");
                Function = (r, time) =>
                    value.Invoke(
                        Jint.Native.JsValue.FromObject(core.engine, r),
                        Jint.Native.JsValue.FromObject(core.engine, time)
                    );
            }
        }
    }
}
