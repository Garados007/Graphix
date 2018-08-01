using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Graphix.Prototypes.Math;

namespace Graphix
{
    /// <summary>
    /// Exports a loaded prototype into an ui XML definition. The definition contains only fully definied 
    /// objects and no real prototypes or cross-references
    /// </summary>
    public class PrototypeExporter
    {
        /// <summary>
        /// Value comparerer
        /// </summary>
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

        /// <summary>
        /// Usage counter
        /// </summary>
        class Counter
        {
            public long Id { get; set; }

            public long Usage { get; set; }
        }

        /// <summary>
        /// Converting information
        /// </summary>
        public class Dict
        {
            /// <summary>
            /// List of all values and their ids
            /// </summary>
            public Dictionary<IValueWrapper, long> ValueWrapper = new Dictionary<IValueWrapper, long>(new IValueWrapperComparer());

            /// <summary>
            /// List of all groups and their ids
            /// </summary>
            public Dictionary<Physic.AnimationGroup, long> Groups = new Dictionary<Physic.AnimationGroup, long>();

            /// <summary>
            /// List of all system values and their names
            /// </summary>
            public Dictionary<IValueWrapper, string> SystemValues = PrototypeLoader.SystemValues.ToDictionary((p) => p.Value, (p) => p.Key, new IValueWrapperComparer());

            /// <summary>
            /// Counter for Animation Groups
            /// </summary>
            public long Counter;
        }

        /// <summary>
        /// List of all objects
        /// </summary>
        public List<FlatPrototype> Objects { get; private set; }

        /// <summary>
        /// List of all status
        /// </summary>
        public Dictionary<string, Status> Status { get; private set; }

        /// <summary>
        /// Exports a loaded prototype into an ui XML definition. The definition contains only fully definied 
        /// objects and no real prototypes or cross-references
        /// </summary>
        public PrototypeExporter()
        {
            Objects = new List<FlatPrototype>();
            Status = new Dictionary<string, Status>();
        }

        /// <summary>
        /// Import the flatten objects from the <see cref="PrototypeLoader"/>
        /// </summary>
        /// <param name="loader">the loader with the ui data</param>
        public void ImportFlatten(PrototypeLoader loader)
        {
            Objects.AddRange(loader.Objects.ToList().ConvertAll((p) => p.Value.Flatten()));
            foreach (var s in loader.Status)
                Status[s.Key] = s.Value;
        }

        /// <summary>
        /// Convert the loaded prototype to a XML document
        /// </summary>
        /// <returns>xml document</returns>
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

        /// <summary>
        /// Save the flattened dom to a ui XML definition file
        /// </summary>
        /// <param name="file"></param>
        public void SaveFlatDom(string file)
        {
            var dir = new System.IO.FileInfo(file).Directory;
            if (!dir.Exists) dir.Create();
            var xml = MakeXmlDom();
            xml.Save(file);
        }

        /// <summary>
        /// Exports all status infos
        /// </summary>
        /// <param name="xml">xml target</param>
        /// <param name="target">current node</param>
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

        /// <summary>
        /// Exports status with root
        /// </summary>
        /// <param name="xml">xml target</param>
        /// <param name="target">current node</param>
        /// <param name="status">current status</param>
        /// <param name="extend">root</param>
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

        /// <summary>
        /// Export all types
        /// </summary>
        /// <param name="xml">xml target</param>
        /// <param name="target">current node</param>
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

        /// <summary>
        /// Exports all objects
        /// </summary>
        /// <param name="xml">xml target</param>
        /// <param name="target">current node</param>
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

        /// <summary>
        /// Filters used values and add them to the dictionary
        /// </summary>
        /// <param name="search">the reference search result</param>
        /// <param name="dict">reference dictionary</param>
        void Filter(Dictionary<IValueWrapper, Counter> search, Dictionary<IValueWrapper, long> dict)
        {
            foreach (var e in search)
                if (e.Value.Usage > 0)
                    dict.Add(e.Key, e.Value.Id);
        }

        /// <summary>
        /// Find all used types if renderer in the prototype
        /// </summary>
        /// <param name="flat">prototype</param>
        /// <param name="types">current list to extend</param>
        void FindTypes(FlatPrototype flat, List<string> types)
        {
            if (!types.Contains(flat.RenderName))
                types.Add(flat.RenderName);
            foreach (var s in flat.Container)
                FindTypes(s, types);
        }

        /// <summary>
        /// lookup for all referencing stuff in the prototype
        /// </summary>
        /// <param name="flat">current prototype</param>
        /// <param name="dict">reference dictionary for values</param>
        /// <param name="d">reference dictionary</param>
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

        /// <summary>
        /// Lookup for variable usage in a parameter
        /// </summary>
        /// <param name="p">current value</param>
        /// <param name="dict">reference dictionary for values</param>
        /// <param name="d">reference dictionary</param>
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

        /// <summary>
        /// Lookup for variable usage in math parameter
        /// </summary>
        /// <param name="p">current value</param>
        /// <param name="dict">reference dictionary for values</param>
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

        /// <summary>
        /// Create the object node
        /// </summary>
        /// <param name="xml">xml target</param>
        /// <param name="flat">current prototype</param>
        /// <param name="dict">reference dictionary</param>
        /// <returns></returns>
        XmlNode CreateObjectNode(XmlDocument xml, FlatPrototype flat, Dict dict)
        {
            var node = xml.CreateElement(flat.RenderName);
            CreateContainer(xml, node, flat, dict);
            CreateAnimations(xml, node, flat, dict);
            CreateParameter(xml, node, flat, dict);

            return node;
        }

        /// <summary>
        /// Create the container of an object
        /// </summary>
        /// <param name="xml">xml target</param>
        /// <param name="target">current node</param>
        /// <param name="flat">current prototype</param>
        /// <param name="dict">reference dictionary</param>
        void CreateContainer(XmlDocument xml, XmlNode target, FlatPrototype flat, Dict dict)
        {
            if (flat.Container.Count == 0) return;
            var node = target.AppendChild(xml.CreateElement("Container"));
            foreach (var sub in flat.Container)
                node.AppendChild(CreateObjectNode(xml, sub, dict));
        }

        /// <summary>
        /// Creates the animations of an object
        /// </summary>
        /// <param name="xml">xml target</param>
        /// <param name="target">current node</param>
        /// <param name="flat">current prototype</param>
        /// <param name="dict">reference dictionary</param>
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

        /// <summary>
        /// Creates the parameter of an object
        /// </summary>
        /// <param name="xml">xml target</param>
        /// <param name="target">current node</param>
        /// <param name="flat">current prototype</param>
        /// <param name="dict">reference dictionary</param>
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

        /// <summary>
        /// creates the math parameter
        /// </summary>
        /// <param name="xml">xml target</param>
        /// <param name="target">target node</param>
        /// <param name="value">current value</param>
        /// <param name="dict">reference dictionary</param>
        /// <returns>current node</returns>
        XmlNode CreateMathParameter(XmlDocument xml, XmlNode target, IValueWrapper value, Dict dict)
        {
            var node = target.AppendChild(xml.CreateElement("Math"));
            var type = value.GetType();
            node.Attributes.Append(xml.CreateAttribute("type")).Value = (string)type.GetProperty("ValueType").GetValue(value);
            CreateMathSubParameter(xml, node, (IValueWrapper)type.GetProperty("ValueSource").GetValue(value), dict);
            return node;
        }

        /// <summary>
        /// creates the math dependent parameter
        /// </summary>
        /// <param name="xml">xml target</param>
        /// <param name="target">target node</param>
        /// <param name="value">current value</param>
        /// <param name="dict">reference dictionary</param>
        /// <returns>current node</returns>
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

        /// <summary>
        /// loads the value of a parameter
        /// </summary>
        /// <param name="v">current value</param>
        /// <param name="dict">reference dictionary</param>
        /// <param name="enableDirect">enable direct referencing</param>
        /// <returns>string representation of parameter</returns>
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
