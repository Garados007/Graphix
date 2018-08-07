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
    /// This activator activates the animation if a script would start it direct
    /// </summary>
    public class ScriptActivator : AnimationActivation
    {
        /// <summary>
        /// They key that the script refere
        /// </summary>
        public ValueWrapper<string> Key { get; set; }

        /// <summary>
        /// This activator activates the animation if a script would start it direct
        /// </summary>
        public ScriptActivator()
        {
            Key = new ValueWrapper<string>();
        }

        /// <summary>
        /// Converts this Activation in its XML representation
        /// </summary>
        /// <param name="xml">target xml document</param>
        /// <param name="dict">the exporter dictionary for variable names</param>
        /// <returns>the new XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("Script");
            AddParamToXml(xml, node, Enabled, "enable", dict);
            AddParamToXml(xml, node, Key, "key", dict);
            return node;
        }

        /// <summary>
        /// A List of all used Variables
        /// </summary>
        /// <returns>the used variable list</returns>
        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                Enabled, Key
            };
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            Key = helper.Convert(Key);
            Enabled = helper.Convert(Enabled);
        }

        /// <summary>
        /// Clone this Activation completly
        /// </summary>
        /// <returns>the clone</returns>
        public override AnimationActivation Clone()
        {
            var act = new ScriptActivator();
            act.Enabled = (ValueWrapper<bool>)Enabled.Clone();
            act.Key = (ValueWrapper<string>)Key.Clone();
            return act;
        }
    }
}
