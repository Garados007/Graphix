using System.Xml;
using Graphix.Prototypes;

namespace Graphix.Physic
{
    /// <summary>
    /// Activates an Animation automaticly
    /// </summary>
    public abstract class AnimationActivation
    {
        /// <summary>
        /// Enables this activation. Disabled this activation has no effect.
        /// </summary>
        public ValueWrapper<bool> Enabled { get; set; }

        /// <summary>
        /// Converts this Activation in its XML representation
        /// </summary>
        /// <param name="xml">target xml document</param>
        /// <param name="dict">the exporter dictionary for variable names</param>
        /// <returns>the new XML node</returns>
        public abstract XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict);

        /// <summary>
        /// A List of all used Variables
        /// </summary>
        /// <returns>the used variable list</returns>
        public abstract IValueWrapper[] GetValueWrapper();

        /// <summary>
        /// helper method to add a parameter to this node
        /// </summary>
        /// <param name="xml">the target XML document</param>
        /// <param name="node">the result node</param>
        /// <param name="param">the value</param>
        /// <param name="name">name of the value</param>
        /// <param name="dict">the exporter dictionary for variable names</param>
        protected void AddParamToXml(XmlDocument xml, XmlNode node, IValueWrapper param, string name, PrototypeExporter.Dict dict)
        {
            if (param.Exists)
                node.Attributes.Append(xml.CreateAttribute(name)).Value = PrototypeExporter.GetParamValue(param, dict);
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public abstract void MoveTargets(PrototypeFlattenerHelper helper);

        /// <summary>
        /// Clone this Activation completly
        /// </summary>
        /// <returns>the clone</returns>
        public abstract AnimationActivation Clone();

        /// <summary>
        /// Creates a new Activation
        /// </summary>
        public AnimationActivation()
        {
            Enabled = new ValueWrapper<bool>();
            Enabled.Exists = true;
            Enabled.Value = true;
        }
    }

    /// <summary>
    /// This Activation activates the Animation when the status has been changed
    /// </summary>
    public class StatusChange : AnimationActivation
    {
        /// <summary>
        /// The old status if set
        /// </summary>
        public ValueWrapper<Status> Old { get; set; }

        /// <summary>
        /// The new status if set
        /// </summary>
        public ValueWrapper<Status> New { get; set; }

        /// <summary>
        /// Creates an Activator that activates an animation when the status has been changed
        /// </summary>
        public StatusChange()
        {
            Old = new ValueWrapper<Status>();
            New = new ValueWrapper<Status>();
        }

        /// <summary>
        /// Converts this Activation in its XML representation
        /// </summary>
        /// <param name="xml">target xml document</param>
        /// <param name="dict">the exporter dictionary for variable names</param>
        /// <returns>the new XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("StatusChange");
            AddParamToXml(xml, node, Enabled, "enable", dict);
            AddParamToXml(xml, node, Old, "old", dict);
            AddParamToXml(xml, node, New, "new", dict);
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
                Enabled, Old, New
            };
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            Old = helper.Convert(Old);
            New = helper.Convert(New);
            Enabled = helper.Convert(Enabled);
        }

        /// <summary>
        /// Clone this Activation completly
        /// </summary>
        /// <returns>the clone</returns>
        public override AnimationActivation Clone()
        {
            var act = new StatusChange();
            act.Enabled = (ValueWrapper<bool>)Enabled.Clone();
            act.Old = (ValueWrapper<Status>)Old.Clone();
            act.New = (ValueWrapper<Status>)New.Clone();
            return act;
        }
    }

    /// <summary>
    /// This Activator actives an animation when an another animation has been finished
    /// </summary>
    public class AfterAnimation : AnimationActivation
    {
        /// <summary>
        /// The other animation after that this animation should be called
        /// </summary>
        public AnimationGroup Effect { get; set; }

        /// <summary>
        /// Converts this Activation in its XML representation
        /// </summary>
        /// <param name="xml">target xml document</param>
        /// <param name="dict">the exporter dictionary for variable names</param>
        /// <returns>the new XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("AfterAnimation");
            AddParamToXml(xml, node, Enabled, "enable", dict);
            node.Attributes.Append(xml.CreateAttribute("effect-id")).Value = "#" + dict.Groups[Effect].ToString();
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
                Enabled
            };
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            Effect = helper.Convert(Effect);
            Enabled = helper.Convert(Enabled);
        }

        /// <summary>
        /// Clone this Activation completly
        /// </summary>
        /// <returns>the clone</returns>
        public override AnimationActivation Clone()
        {
            var act = new AfterAnimation();
            act.Enabled = (ValueWrapper<bool>)Enabled.Clone();
            act.Effect = Effect;
            return act;
        }
    }

    /// <summary>
    /// An activator that actives an animation after its block was clicked
    /// </summary>
    public class ClickAnimation : AnimationActivation
    {
        /// <summary>
        /// The button that was clicked
        /// </summary>
        public ValueWrapper<ClickButton> Button { get; set; }

        /// <summary>
        /// An activator that actives an animation after its block was clicked
        /// </summary>
        public ClickAnimation()
        {
            Button = new ValueWrapper<ClickButton>();
        }

        /// <summary>
        /// Converts this Activation in its XML representation
        /// </summary>
        /// <param name="xml">target xml document</param>
        /// <param name="dict">the exporter dictionary for variable names</param>
        /// <returns>the new XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("Click");
            AddParamToXml(xml, node, Enabled, "enable", dict);
            AddParamToXml(xml, node, Button, "button", dict);
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
                Enabled, Button
            };
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            Enabled = helper.Convert(Enabled);
            Button = helper.Convert(Button);
        }

        /// <summary>
        /// Clone this Activation completly
        /// </summary>
        /// <returns>the clone</returns>
        public override AnimationActivation Clone()
        {
            var act = new ClickAnimation();
            act.Enabled = (ValueWrapper<bool>)Enabled.Clone();
            act.Button = (ValueWrapper<ClickButton>)Button.Clone();
            return act;
        }
    }

    /// <summary>
    /// Define the mouse button that was clicked
    /// </summary>
    public enum ClickButton
    {
        /// <summary>
        /// The left mouse button was clicked
        /// </summary>
        Left,
        /// <summary>
        /// The right mouse button was clicked
        /// </summary>
        Right,
        /// <summary>
        /// The middle mouse button was clicked
        /// </summary>
        Middle,
        /// <summary>
        /// An unknown mouse button was clicked
        /// </summary>
        Unknown
    }

    /// <summary>
    /// An activator that actives an animation after a button on the keyboard was clicked
    /// </summary>
    public class KeyDownActivation : AnimationActivation
    {
        /// <summary>
        /// The button that was clicked
        /// </summary>
        public ValueWrapper<Keys> Key { get; set; }

        /// <summary>
        /// An activator that actives an animation after its block was clicked
        /// </summary>
        public KeyDownActivation()
        {
            Key = new ValueWrapper<Keys>();
        }

        /// <summary>
        /// Converts this Activation in its XML representation
        /// </summary>
        /// <param name="xml">target xml document</param>
        /// <param name="dict">the exporter dictionary for variable names</param>
        /// <returns>the new XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("KeyDown");
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
            Enabled = helper.Convert(Enabled);
            Key = helper.Convert(Key);
        }

        /// <summary>
        /// Clone this Activation completly
        /// </summary>
        /// <returns>the clone</returns>
        public override AnimationActivation Clone()
        {
            var act = new KeyDownActivation();
            act.Enabled = (ValueWrapper<bool>)Enabled.Clone();
            act.Key = (ValueWrapper<Keys>)Key.Clone();
            return act;
        }
    }

    /// <summary>
    /// An activator that actives an animation after a button on the keyboard was released
    /// </summary>
    public class KeyUpActivation : AnimationActivation
    {
        /// <summary>
        /// The button that was released
        /// </summary>
        public ValueWrapper<Keys> Key { get; set; }

        /// <summary>
        /// An activator that actives an animation after its block was released
        /// </summary>
        public KeyUpActivation()
        {
            Key = new ValueWrapper<Keys>();
        }

        /// <summary>
        /// Converts this Activation in its XML representation
        /// </summary>
        /// <param name="xml">target xml document</param>
        /// <param name="dict">the exporter dictionary for variable names</param>
        /// <returns>the new XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("KeyUp");
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
            Enabled = helper.Convert(Enabled);
            Key = helper.Convert(Key);
        }

        /// <summary>
        /// Clone this Activation completly
        /// </summary>
        /// <returns>the clone</returns>
        public override AnimationActivation Clone()
        {
            var act = new KeyUpActivation();
            act.Enabled = (ValueWrapper<bool>)Enabled.Clone();
            act.Key = (ValueWrapper<Keys>)Key.Clone();
            return act;
        }
    }

    /// <summary>
    /// An activator that actives an animation after a char was typed on the keyboard
    /// </summary>
    public class KeyPressActivation : AnimationActivation
    {
        /// <summary>
        /// The button that was clicked
        /// </summary>
        public ValueWrapper<string> Char { get; set; }

        /// <summary>
        /// An activator that actives an animation after a char was typed on the keyboard
        /// </summary>
        public KeyPressActivation()
        {
            Char = new ValueWrapper<string>();
        }

        /// <summary>
        /// Converts this Activation in its XML representation
        /// </summary>
        /// <param name="xml">target xml document</param>
        /// <param name="dict">the exporter dictionary for variable names</param>
        /// <returns>the new XML node</returns>
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("KeyPress");
            AddParamToXml(xml, node, Enabled, "enable", dict);
            AddParamToXml(xml, node, Char, "char", dict);
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
                Enabled, Char
            };
        }

        /// <summary>
        /// Move the targets of the used values
        /// </summary>
        /// <param name="helper">The flatten helper</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            Enabled = helper.Convert(Enabled);
            Char = helper.Convert(Char);
        }

        /// <summary>
        /// Clone this Activation completly
        /// </summary>
        /// <returns>the clone</returns>
        public override AnimationActivation Clone()
        {
            var act = new KeyPressActivation();
            act.Enabled = (ValueWrapper<bool>)Enabled.Clone();
            act.Char = (ValueWrapper<string>)Char.Clone();
            return act;
        }
    }
}
