using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Graphix.Prototypes;

namespace Graphix.Physic
{
    public abstract class AnimationActivation
    {
        public ValueWrapper<bool> Enabled { get; set; }

        public abstract XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict);

        public abstract IValueWrapper[] GetValueWrapper();

        protected void AddParamToXml(XmlDocument xml, XmlNode node, IValueWrapper param, string name, PrototypeExporter.Dict dict)
        {
            if (param.Exists)
                node.Attributes.Append(xml.CreateAttribute(name)).Value = PrototypeExporter.GetParamValue(param, dict);
        }

        public abstract void MoveTargets(PrototypeFlattenerHelper helper);

        public abstract AnimationActivation Clone();

        public AnimationActivation()
        {
            Enabled = new ValueWrapper<bool>();
            Enabled.Exists = true;
            Enabled.Value = true;
        }
    }

    public class StatusChange : AnimationActivation
    {
        public ValueWrapper<Status> Old { get; set; }

        public ValueWrapper<Status> New { get; set; }

        public StatusChange()
        {
            Old = new ValueWrapper<Status>();
            New = new ValueWrapper<Status>();
        }

        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("StatusChange");
            AddParamToXml(xml, node, Enabled, "enable", dict);
            AddParamToXml(xml, node, Old, "old", dict);
            AddParamToXml(xml, node, New, "new", dict);
            return node;
        }

        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                Enabled, Old, New
            };
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            Old = helper.Convert(Old);
            New = helper.Convert(New);
            Enabled = helper.Convert(Enabled);
        }

        public override AnimationActivation Clone()
        {
            var act = new StatusChange();
            act.Enabled = (ValueWrapper<bool>)Enabled.Clone();
            act.Old = (ValueWrapper<Status>)Old.Clone();
            act.New = (ValueWrapper<Status>)New.Clone();
            return act;
        }
    }

    public class AfterAnimation : AnimationActivation
    {
        public AnimationGroup Effect { get; set; }

        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("AfterAnimation");
            AddParamToXml(xml, node, Enabled, "enable", dict);
            node.Attributes.Append(xml.CreateAttribute("effect-id")).Value = "#" + dict.Groups[Effect].ToString();
            return node;
        }

        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[] 
            {
                Enabled
            };
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            Effect = helper.Convert(Effect);
            Enabled = helper.Convert(Enabled);
        }

        public override AnimationActivation Clone()
        {
            var act = new AfterAnimation();
            act.Enabled = (ValueWrapper<bool>)Enabled.Clone();
            act.Effect = Effect;
            return act;
        }
    }

    public class ClickAnimation : AnimationActivation
    {
        public override XmlNode ToXml(XmlDocument xml, PrototypeExporter.Dict dict)
        {
            var node = xml.CreateElement("Click");
            AddParamToXml(xml, node, Enabled, "enable", dict);
            return node;
        }
        public override IValueWrapper[] GetValueWrapper()
        {
            return new IValueWrapper[]
            {
                Enabled
            };
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            Enabled = helper.Convert(Enabled);
        }

        public override AnimationActivation Clone()
        {
            var act = new ClickAnimation();
            act.Enabled = (ValueWrapper<bool>)Enabled.Clone();
            return act;
        }
    }
}
