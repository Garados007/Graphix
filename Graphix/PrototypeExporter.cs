using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Graphix.Prototypes.Math;

namespace Graphix
{
    public class PrototypeExporter
    {
        class IValueWrapperComparer : IEqualityComparer<IValueWrapper>
        {
            public bool Equals(IValueWrapper x, IValueWrapper y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(IValueWrapper obj)
            {
                return ((object)obj).GetHashCode();
            }
        }

        class Counter
        {
            public long Id { get; set; }

            public long Usage { get; set; }
        }

        public class Dict
        {
            public Dictionary<IValueWrapper, long> ValueWrapper = new Dictionary<IValueWrapper, long>(new IValueWrapperComparer());

            public Dictionary<Physic.AnimationGroup, long> Groups = new Dictionary<Physic.AnimationGroup, long>();

            public Dictionary<IValueWrapper, string> SystemValues = PrototypeLoader.SystemValues.ToDictionary((p) => p.Value, (p) => p.Key, new IValueWrapperComparer());

            public long Counter;
        }

        public List<FlatPrototype> Objects { get; private set; }

        public Dictionary<string, Status> Status { get; private set; }

        public PrototypeExporter()
        {
            Objects = new List<FlatPrototype>();
            Status = new Dictionary<string, Status>();
        }

        public void ImportFlatten(PrototypeLoader loader)
        {
            Objects.AddRange(loader.Objects.ToList().ConvertAll((p) => p.Value.Flatten()));
            foreach (var s in loader.Status)
                Status[s.Key] = s.Value;
        }

        public XmlDocument MakeXmlDom()
        {
            var xml = new XmlDocument();
            xml.AppendChild(xml.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
            var node = xml.AppendChild(xml.CreateElement("Objects"));

            ExportStatus(xml, node);
            ExportTypes(xml, node);
            ExportObjects(xml, node);

            return xml;
        }

        public void SaveFlatDom(string file)
        {
            var dir = new System.IO.FileInfo(file).Directory;
            if (!dir.Exists) dir.Create();
            var xml = MakeXmlDom();
            xml.Save(file);
        }

        void ExportStatus(XmlDocument xml, XmlNode target)
        {
            var node = target.AppendChild(xml.CreateElement("Status"));
            foreach (var s in Status)
            {
                if (s.Value.SubStatus.Count > 0)
                    ExportStatus(xml, target, s.Value, s.Value.Name);
                var val = node.AppendChild(xml.CreateElement("Value"));
                val.InnerText = s.Value.Name;
            }
        }

        void ExportStatus(XmlDocument xml, XmlNode target, Status status, string extend)
        {
            var node = target.AppendChild(xml.CreateElement("Status"));
            node.Attributes.Append(xml.CreateAttribute("extends")).Value = extend;
            //extend = extend + "|" + status.Name;
            foreach (var sub in status.SubStatus)
            {
                if (sub.SubStatus.Count > 0)
                    ExportStatus(xml, target, sub, extend + "|" + sub.Name);
                var val = node.AppendChild(xml.CreateElement("Value"));
                val.InnerText = sub.Name;
            }
        }

        void ExportTypes(XmlDocument xml, XmlNode target)
        {
            var types = new List<string>();
            foreach (var flat in Objects) FindTypes(flat, types);
            foreach (var type in types)
            {
                var node = target.AppendChild(xml.CreateElement("Prototype"));
                node.Attributes.Append(xml.CreateAttribute("object-name")).Value = type;
                node.Attributes.Append(xml.CreateAttribute("dotnet")).Value = type;
            }
        }

        void ExportObjects(XmlDocument xml, XmlNode target)
        {
            var dict = new Dict();
            var search = new Dictionary<IValueWrapper, Counter>(new IValueWrapperComparer());
            foreach (var obj in Objects)
                CreateLookup(obj, search, dict);
            Filter(search, dict.ValueWrapper);
            foreach (var obj in Objects)
            {
                var node = CreateObjectNode(xml, obj, dict);
                node.Attributes.Append(xml.CreateAttribute("name")).Value = obj.Name;
                target.AppendChild(node);
            }
        }

        void Filter(Dictionary<IValueWrapper, Counter> search, Dictionary<IValueWrapper, long> dict)
        {
            foreach (var e in search)
                if (e.Value.Usage > 0)
                    dict.Add(e.Key, e.Value.Id);
        }

        void FindTypes(FlatPrototype flat, List<string> types)
        {
            if (!types.Contains(flat.RenderName))
                types.Add(flat.RenderName);
            foreach (var s in flat.Container)
                FindTypes(s, types);
        }

        void CreateLookup(FlatPrototype flat, Dictionary<IValueWrapper, Counter> dict, Dict d)
        {
            foreach (var sub in flat.Container) CreateLookup(sub, dict, d);
            foreach (var param in flat.Parameter)
                LookupParam(param.Value, dict, d);
            foreach (var group in flat.Animations)
            {
                d.Groups.Add(group, d.Counter++);
                LookupParam(group.EffectTiming, dict, d);
                foreach (var act in group.Activations)
                    foreach (var p in act.GetValueWrapper())
                        LookupParam(p, dict, d);
                foreach (var eff in group.Effects)
                    foreach (var p in eff.GetValueWrapper())
                        LookupParam(p, dict, d);
            }
        }

        void LookupParam(IValueWrapper p, Dictionary<IValueWrapper, Counter> dict, Dict d)
        {
            if (p.Name == "Math")
            {
                var type = p.GetType();
                LookupMathParam((IValueWrapper)type.GetProperty("ValueSource").GetValue(p), dict);
            }
            for (; p != null; p = p.RemoteSource)
                if (!d.SystemValues.ContainsKey(p))
                {
                    if (!dict.ContainsKey(p))
                        dict.Add(p, new Counter() { Id = d.Counter++ });
                    else dict[p].Usage++;
                }
        }

        void LookupMathParam(IValueWrapper p, Dictionary<IValueWrapper, Counter> dict)
        {
            if (p is Calc)
            {
                foreach (var v in ((Calc)p).ValueList)
                    LookupMathParam(v, dict);
            }
            else if (p is If)
            {
                var val = (If)p;
                LookupMathParam(val.Condition, dict);
                LookupMathParam(val.True, dict);
                LookupMathParam(val.False, dict);
            }
            else if (p is Check)
            {
                var check = (Check)p;
                LookupMathParam(check.Value1, dict);
                LookupMathParam(check.Value2, dict);
            }
            else
            {
                for (; p != null; p = p.RemoteSource)
                    if (dict.ContainsKey(p))
                        dict[p].Usage++;
            }
        }

        XmlNode CreateObjectNode(XmlDocument xml, FlatPrototype flat, Dict dict)
        {
            var node = xml.CreateElement(flat.RenderName);
            CreateContainer(xml, node, flat, dict);
            CreateAnimations(xml, node, flat, dict);
            CreateParameter(xml, node, flat, dict);

            return node;
        }

        void CreateContainer(XmlDocument xml, XmlNode target, FlatPrototype flat, Dict dict)
        {
            if (flat.Container.Count == 0) return;
            var node = target.AppendChild(xml.CreateElement("Container"));
            foreach (var sub in flat.Container)
                node.AppendChild(CreateObjectNode(xml, sub, dict));
        }

        void CreateAnimations(XmlDocument xml, XmlNode target, FlatPrototype flat, Dict dict)
        {
            if (flat.Animations.Count == 0) return;
            target = target.AppendChild(xml.CreateElement("Animation"));
            foreach (var group in flat.Animations)
            {
                var node = target.AppendChild(xml.CreateElement("Group"));
                //node.Attributes.Append(xml.CreateAttribute("name")).Value = group.Name;
                node.Attributes.Append(xml.CreateAttribute("id")).Value = "#" + dict.Groups[group].ToString();
                if (group.Activations.Count > 0)
                {
                    var sub = node.AppendChild(xml.CreateElement("Activation"));
                    foreach (var effect in group.Activations)
                    {
                        sub.AppendChild(effect.ToXml(xml, dict));
                    }
                }
                if (group.Effects.Count > 0)
                {
                    var sub = node.AppendChild(xml.CreateElement("Effects"));
                    sub.Attributes.Append(xml.CreateAttribute("timing")).Value = GetParamValue(group.EffectTiming, dict);
                    foreach (var effect in group.Effects)
                    {
                        sub.AppendChild(effect.ToXml(xml, dict));
                    }
                }
            }
        }

        void CreateParameter(XmlDocument xml, XmlNode target, FlatPrototype flat, Dict dict)
        {
            if (flat.Parameter.Count == 0) return;
            target = target.AppendChild(xml.CreateElement("Parameter"));
            foreach (var param in flat.Parameter)
            {
                XmlNode node;
                if (param.Value.Name == "Math")
                {
                    node = CreateMathParameter(xml, target, param.Value, dict);
                    node.Attributes.Append(xml.CreateAttribute("name")).Value = param.Key;
                }
                else
                {
                    var ptype = param.Value.GetType().FullName;
                    var name = PrototypeLoader.ParameterTypes.ToList().Find((p) => p.Value.FullName == ptype).Key;
                    node = target.AppendChild(xml.CreateElement(name));
                    node.Attributes.Append(xml.CreateAttribute("name")).Value = param.Key;
                    var value = GetParamValue(param.Value, dict, false);
                    if (value != null) node.Attributes.Append(xml.CreateAttribute("value")).Value = value;
                }
                if (dict.ValueWrapper.ContainsKey(param.Value))
                    node.Attributes.Append(xml.CreateAttribute("id")).Value = dict.ValueWrapper[param.Value].ToString();
            }
        }

        XmlNode CreateMathParameter(XmlDocument xml, XmlNode target, IValueWrapper value, Dict dict)
        {
            var node = target.AppendChild(xml.CreateElement("Math"));
            var type = value.GetType();
            node.Attributes.Append(xml.CreateAttribute("type")).Value = (string)type.GetProperty("ValueType").GetValue(value);
            CreateMathSubParameter(xml, node, (IValueWrapper)type.GetProperty("ValueSource").GetValue(value), dict);
            return node;
        }

        void CreateMathSubParameter(XmlDocument xml, XmlNode target, IValueWrapper value, Dict dict)
        {
            if (value is Calc)
            {
                var calc = (Calc)value;
                var node = target.AppendChild(xml.CreateElement("Calc"));
                node.Attributes.Append(xml.CreateAttribute("method")).Value = calc.Method.ToString();
                node.Attributes.Append(xml.CreateAttribute("type")).Value = calc.Type.ToString();
                if (calc.Precompile)
                    node.Attributes.Append(xml.CreateAttribute("precompile")).Value = calc.Precompile.ToString();
                foreach (var sub in calc.ValueList)
                    CreateMathSubParameter(xml, node, sub, dict);
            }
            else if (value is If)
            {
                var val = (If)value;
                var node = target.AppendChild(xml.CreateElement("If"));
                if (val.Precompile)
                    node.Attributes.Append(xml.CreateAttribute("precompile")).Value = val.Precompile.ToString();
                CreateMathSubParameter(xml, node.AppendChild(xml.CreateElement("Condition")), val.Condition, dict);
                CreateMathSubParameter(xml, node.AppendChild(xml.CreateElement("True")), val.True, dict);
                CreateMathSubParameter(xml, node.AppendChild(xml.CreateElement("False")), val.False, dict);
            }
            else if (value is Check)
            {
                var check = (Check)value;
                var node = target.AppendChild(xml.CreateElement("Check"));
                node.Attributes.Append(xml.CreateAttribute("method")).Value = check.Mode.ToString();
                if (check.Precompile)
                    node.Attributes.Append(xml.CreateAttribute("precompile")).Value = check.Precompile.ToString();
                CreateMathSubParameter(xml, node, check.Value1, dict);
                CreateMathSubParameter(xml, node, check.Value2, dict);
            }
            else
            {
                var ptype = value.GetType().FullName;
                var name = PrototypeLoader.ParameterTypes.ToList().Find((p) => p.Value.FullName == ptype).Key;
                var node = target.AppendChild(xml.CreateElement(name));
                var val = GetParamValue(value, dict, false);
                if (val.StartsWith("#") || val.StartsWith("$"))
                    node.Attributes.Append(xml.CreateAttribute("ref")).Value = val;
                else node.Attributes.Append(xml.CreateAttribute("value")).Value = val;
            }
        }

        public static string GetParamValue(IValueWrapper v, Dict dict, bool enableDirect = true)
        {
            if (!enableDirect)
            {
                if (v.RemoteSource != null && dict.ValueWrapper.ContainsKey(v.RemoteSource)) return "#" + dict.ValueWrapper[v.RemoteSource];
            }
            else if (dict.ValueWrapper.ContainsKey(v)) return "#" + dict.ValueWrapper[v];
            if (dict.SystemValues.ContainsKey(v)) return "$" + dict.SystemValues[v];
            //var sv = PrototypeLoader.SystemValues.ToList().FindIndex((p) => p.Value == v);
            //if (sv != -1) return "$" + PrototypeLoader.SystemValues.ElementAt(sv).Key;
            return v.Value?.ToString();
        }
    }
}
