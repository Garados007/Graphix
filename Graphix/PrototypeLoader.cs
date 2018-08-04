using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using Graphix.Prototypes;
using Graphix.Prototypes.Math;
using Graphix.Physic;
using Color = System.Drawing.Color;
using KnownColor = System.Drawing.KnownColor;

namespace Graphix
{
    /// <summary>
    /// Loader for the ui from the ui XML definition
    /// </summary>
    public class PrototypeLoader
    {
        #region Static Lib

        static List<Type> dotnetPrototypes = new List<Type>();
        /// <summary>
        /// List of all core Prototypes
        /// </summary>
        public static Type[] DotNetPrototypes => dotnetPrototypes.ToArray();

        /// <summary>
        /// Register a new core <see cref="PrototypeBase"/>
        /// </summary>
        /// <typeparam name="T">the type of the prototype</typeparam>
        public static void AddDotNetPrototype<T>() where T:PrototypeBase
        {
            var type = typeof(T);
            if (!dotnetPrototypes.Contains(type))
                dotnetPrototypes.Add(type);
        }

        static Dictionary<string, Type> parameterTypes = new Dictionary<string, Type>();
        static Dictionary<string, Delegate> parameterConverter = new Dictionary<string, Delegate>();
        public static Dictionary<string, Type> ParameterTypes => parameterTypes.ToDictionary((e) => e.Key, (e) => e.Value);
        static Dictionary<string, Type> mathParameterTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Register a parameter type that can used as the type of any value in the ui XML definition
        /// </summary>
        /// <typeparam name="T">the type of parameter</typeparam>
        /// <param name="name">name of this type using in the XML definition</param>
        /// <param name="converter">converter that can convert string to value type</param>
        public static void AddParameterType<T>(string name, Func<PrototypeLoader, PrototypeBase, string, T> converter) 
        {
            if (!parameterTypes.ContainsKey(name))
            {
                parameterTypes.Add(name, typeof(ValueWrapper<T>));
                mathParameterTypes.Add(name, typeof(MathValue<T>));
                parameterConverter.Add(name, converter);
            }
        }

        /// <summary>
        /// Public system values that can references everywhere in the ui
        /// </summary>
        public static Dictionary<string, IValueWrapper> SystemValues { get; private set; }

        private static Dictionary<string, Tuple<Type, Delegate>> activators = new Dictionary<string, Tuple<Type, Delegate>>();
        /// <summary>
        /// Register a <see cref="AnimationActivation"/> to use in ui XML definition
        /// </summary>
        /// <typeparam name="T">the type of the activator</typeparam>
        /// <param name="name">the name of this activator that can be used in ui XML definition</param>
        /// <param name="dataFiller">method that can fill this activator with data from the node</param>
        public static void AddActivator<T>(string name, Action<PrototypeLoader, PrototypeBase, T, XmlNode> dataFiller) where T:AnimationActivation
        {
            if (!activators.ContainsKey(name))
                activators.Add(name, new Tuple<Type, Delegate>(typeof(T), dataFiller));
        }

        private static Dictionary<string, Tuple<Type, Delegate>> effects = new Dictionary<string, Tuple<Type, Delegate>>();
        /// <summary>
        /// Register a <see cref="AnimationEffect"/> to use in ui XML definition
        /// </summary>
        /// <typeparam name="T">the type of the effect</typeparam>
        /// <param name="name">the name of this effect that can be used in ui XML definition</param>
        /// <param name="dataFiller">method that can fill this effect with data from the node</param>
        public static void AddEffect<T>(string name, Action<PrototypeLoader, PrototypeBase, T, XmlNode> dataFiller) where T:AnimationEffect
        {
            if (!effects.ContainsKey(name))
                effects.Add(name, new Tuple<Type, Delegate>(typeof(T), dataFiller));
        }

        /// <summary>
        /// Initialize static values
        /// </summary>
        static PrototypeLoader()
        {
            #region SystemValues

            SystemValues = new Dictionary<string, IValueWrapper>();

            #endregion
            #region .Net Prototype

            AddDotNetPrototype<PrototypeBase>();
            AddDotNetPrototype<DisplayBase>();
            AddDotNetPrototype<RenderingBase>();
            AddDotNetPrototype<Image>();
            AddDotNetPrototype<Text>();
            AddDotNetPrototype<Line>();
            AddDotNetPrototype<Rect>();
            AddDotNetPrototype<Ellipse>();
            AddDotNetPrototype<AnimImage>();

            #endregion
            #region Parameter Type

            AddParameterType("String", (pl, pb, t) => t);
            AddParameterType("Double", (pl, pb, t) => Double.Parse(t.Replace(',','.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture));
            AddParameterType("Int", (pl, pb, t) => int.Parse(t));
            AddParameterType("Bool", (pl, pb, t) => bool.Parse(t));
            AddParameterType("Repeat", (pl, pb, t) => new RepeatMode(t == "none" ? 0 : t == "infinite" ? uint.MaxValue : uint.Parse(t)));
            AddParameterType("Status", (pl, pb, t) => pl.FindStatus(t.Split('|')));
            AddParameterType("ScreenPos", (pl, pb, t) =>
            {
                if (t.EndsWith("%")) return new ScreenPos(double.Parse(t.Remove(t.Length - 1).Replace(',', '.'), 
                    System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture), PosType.Relative);
                if (t.EndsWith("%w")) return new ScreenPos(double.Parse(t.Remove(t.Length - 2).Replace(',','.'),
                    System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture), PosType.RelativeWidth);
                if (t.EndsWith("%h")) return new ScreenPos(double.Parse(t.Remove(t.Length - 2).Replace(',', '.'), 
                    System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture), PosType.RelativeHeight);
                if (t.EndsWith("v")) return new ScreenPos(double.Parse(t.Remove(t.Length - 1).Replace(',', '.'), 
                    System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture), PosType.Screen);
                if (t.EndsWith("vw")) return new ScreenPos(double.Parse(t.Remove(t.Length - 2).Replace(',','.'),
                    System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture), PosType.ScreenWidth);
                if (t.EndsWith("vh")) return new ScreenPos(double.Parse(t.Remove(t.Length - 2).Replace(',', '.'),
                    System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture), PosType.ScreenHeight);
                return new ScreenPos(double.Parse(t), PosType.Absolute);
            });
            AddParameterType("Align", (pl, pb, t) => (Align)Enum.Parse(typeof(Align), t));
            AddParameterType("Valign", (pl, pb, t) => (Valign)Enum.Parse(typeof(Valign), t));
            AddParameterType("AnimMode", (pl, pb, t) => (AnimationMode)Enum.Parse(typeof(AnimationMode), t));
            AddParameterType("Color", (pl, pb, t) =>
            {
                if (t == null) return Color.Black;
                KnownColor color;
                if (Enum.TryParse(t, true, out color))
                    return Color.FromKnownColor(color);
                else
                {
                    var p = t.Split(',');
                    int r = 0, g = 0, b = 0, a = 255;
                    bool error = false;
                    switch (p.Length)
                    {
                        case 1:
                            if (!int.TryParse(p[0], out r)) error = true;
                            else g = b = r;
                            break;
                        case 2:
                            if (!int.TryParse(p[0], out r) || !int.TryParse(p[1], out a)) error = true;
                            else g = b = r;
                            break;
                        case 3:
                            if (!int.TryParse(p[0], out r) || !int.TryParse(p[1], out g) || !int.TryParse(p[2], out b)) error = true;
                            break;
                        case 4:
                            if (!int.TryParse(p[0], out r) || !int.TryParse(p[1], out g) || !int.TryParse(p[2], out b) || !int.TryParse(p[3], out a)) error = true;
                            break;
                        default: error = true; break;
                    }
                    return error ? Color.Black : Color.FromArgb(a, r, g, b);
                }
            });
            AddParameterType("ClickButton", (pl, pb, t) =>
            {
                if (Enum.TryParse(t, true, out ClickButton button))
                    return button;
                else return ClickButton.Left;
            });
            AddParameterType("Key", (pl, pb, t) => (Keys)Enum.Parse(typeof(Keys), t));

            #endregion
            #region Activator

            AddActivator<StatusChange>("StatusChange", (pl, pb, a, node) =>
            {
                pl.SetParameter(node.Attributes["old"]?.Value, a.Old, pb, false, parameterConverter["Status"]);
                pl.SetParameter(node.Attributes["new"]?.Value, a.New, pb, false, parameterConverter["Status"]);
                pl.SetParameter(node.Attributes["enable"]?.Value, a.Enabled, pb, false, parameterConverter["Bool"]);
            });
            AddActivator<AfterAnimation>("AfterAnimation", (pl, pb, a, node) =>
            {
                if (node.Attributes["effect-id"] != null)
                    a.Effect = pl.groupList[node.Attributes["effect-id"].Value];
                else
                {
                    var prot = pb.GetElement(node.Attributes["target"].Value);
                    a.Effect = prot.GetAnimation(node.Attributes["effect"].Value);
                }
                pl.SetParameter(node.Attributes["enable"]?.Value, a.Enabled, pb, false, parameterConverter["Bool"]);
            });
            AddActivator<ClickAnimation>("Click", (pl, pb, a, node) =>
            {
                pl.SetParameter(node.Attributes["enable"]?.Value, a.Enabled, pb, false, parameterConverter["Bool"]);
                pl.SetParameter(node.Attributes["button"]?.Value, a.Button, pb, false, parameterConverter["ClickButton"]);
            });
            AddActivator<KeyDownActivation>("KeyDown", (pl, pb, a, node) =>
            {
                pl.SetParameter(node.Attributes["enable"]?.Value, a.Enabled, pb, false, parameterConverter["Bool"]);
                pl.SetParameter(node.Attributes["key"]?.Value, a.Key, pb, false, parameterConverter["Key"]);
            });
            AddActivator<KeyUpActivation>("KeyUp", (pl, pb, a, node) =>
            {
                pl.SetParameter(node.Attributes["enable"]?.Value, a.Enabled, pb, false, parameterConverter["Bool"]);
                pl.SetParameter(node.Attributes["key"]?.Value, a.Key, pb, false, parameterConverter["Key"]);
            });
            AddActivator<KeyPressActivation>("KeyPress", (pl, pb, a, node) =>
            {
                pl.SetParameter(node.Attributes["enable"]?.Value, a.Enabled, pb, false, parameterConverter["Bool"]);
                pl.SetParameter(node.Attributes["char"]?.Value, a.Char, pb, false, parameterConverter["Key"]);
            });
            AddActivator<ChannelActivation>("Channel", (pl, pb, a, node) =>
            {
                pl.SetParameter(node.Attributes["enable"]?.Value, a.Enabled, pb, false, parameterConverter["Bool"]);
                pl.SetParameter(node.Attributes["old"]?.Value, a.Old, pb, false, parameterConverter["String"]);
            });

            #endregion
            #region Effect

            var effectBase = new Action<PrototypeLoader, PrototypeBase, AnimationEffect, XmlNode>((pl, pb, e, node) =>
            {
                pl.SetParameter(node.Attributes["repeat"]?.Value, e.Repeat, pb, false, parameterConverter["Repeat"]);
                pl.SetParameter(node.Attributes["reverse"]?.Value, e.Reverse, pb, false, parameterConverter["Bool"]);
                pl.SetParameter(node.Attributes["time-start"]?.Value, e.TimeStart, pb, false, parameterConverter["Double"]);
                pl.SetParameter(node.Attributes["time-offset"]?.Value, e.TimeOffset, pb, false, parameterConverter["Double"]);
                pl.SetParameter(node.Attributes["time-duration"]?.Value, e.TimeDuration, pb, false, parameterConverter["Double"]);
                pl.SetParameter(node.Attributes["time-finish"]?.Value, e.TimeFinish, pb, false, parameterConverter["Double"]);
                pl.SetParameter(node.Attributes["mode"]?.Value, e.Mode, pb, false, parameterConverter["AnimMode"]);
                pl.SetParameter(node.Attributes["enable"]?.Value, e.Enable, pb, false, parameterConverter["Bool"]);
                pl.SetParameter(node.Attributes["async"]?.Value, e.Async, pb, false, parameterConverter["Bool"]);
            });
            var fetchTarget = new Func<PrototypeLoader, PrototypeBase, XmlNode, IValueWrapper>((pl, pb, node) =>
            {
                if (node.Attributes["target-id"] != null)
                {
                    var id = node.Attributes["target-id"].Value;
                    if (id.StartsWith("#")) id = id.Substring(1);
                    return pl.idList[id];
                }
                else return pb.GetParameter(node.Attributes["param"].Value);
            });
            AddEffect<ADouble>("ADouble", (pl, pb, e, node) =>
            {
                effectBase(pl, pb, e, node);
                e.Target = (ValueWrapper<double>)fetchTarget(pl, pb, node);
                pl.SetParameter(node.Attributes["value-start"]?.Value, e.ValueStart, pb, false, parameterConverter["Double"]);
                pl.SetParameter(node.Attributes["value-change"]?.Value, e.ValueChange, pb, false, parameterConverter["Double"]);
                pl.SetParameter(node.Attributes["value-finish"]?.Value, e.ValueFinish, pb, false, parameterConverter["Double"]);
            });
            AddEffect<AInt>("AInt", (pl, pb, e, node) =>
            {
                effectBase(pl, pb, e, node);
                e.Target = (ValueWrapper<int>)fetchTarget(pl, pb, node);
                pl.SetParameter(node.Attributes["value-start"]?.Value, e.ValueStart, pb, false, parameterConverter["Int"]);
                pl.SetParameter(node.Attributes["value-change"]?.Value, e.ValueChange, pb, false, parameterConverter["Int"]);
                pl.SetParameter(node.Attributes["value-finish"]?.Value, e.ValueFinish, pb, false, parameterConverter["Int"]);
            });
            AddEffect<AColor>("AColor", (pl, pb, e, node) =>
            {
                effectBase(pl, pb, e, node);
                e.Target = (ValueWrapper<Color>)fetchTarget(pl, pb, node);
                pl.SetParameter(node.Attributes["value-start"]?.Value, e.ValueStart, pb, false, parameterConverter["Color"]);
                pl.SetParameter(node.Attributes["value-finish"]?.Value, e.ValueFinish, pb, false, parameterConverter["Color"]);
            });
            AddEffect<AScreenPos>("AScreenPos", (pl, pb, e, node) =>
            {
                effectBase(pl, pb, e, node);
                e.Target = (ValueWrapper<ScreenPos>)fetchTarget(pl, pb, node);
                pl.SetParameter(node.Attributes["value-start"]?.Value, e.ValueStart, pb, false, parameterConverter["ScreenPos"]);
                pl.SetParameter(node.Attributes["value-change"]?.Value, e.ValueChange, pb, false, parameterConverter["ScreenPos"]);
                pl.SetParameter(node.Attributes["value-finish"]?.Value, e.ValueFinish, pb, false, parameterConverter["ScreenPos"]);
            });
            AddEffect<ABool>("ABool", (pl, pb, e, node) =>
            {
                effectBase(pl, pb, e, node);
                e.Target = (ValueWrapper<bool>)fetchTarget(pl, pb, node);
                pl.SetParameter(node.Attributes["value-start"]?.Value, e.ValueStart, pb, false, parameterConverter["Bool"]);
                pl.SetParameter(node.Attributes["value-finish"]?.Value, e.ValueFinish, pb, false, parameterConverter["Bool"]);
            });
            AddEffect<AString>("AString", (pl, pb, e, node) =>
            {
                effectBase(pl, pb, e, node);
                e.Target = (ValueWrapper<string>)fetchTarget(pl, pb, node);
                pl.SetParameter(node.Attributes["value-start"]?.Value, e.ValueStart, pb, false, parameterConverter["String"]);
                pl.SetParameter(node.Attributes["value-finish"]?.Value, e.ValueFinish, pb, false, parameterConverter["String"]);
                pl.SetParameter(node.Attributes["flip"]?.Value, e.Flip, pb, false, parameterConverter["Bool"]);
            });
            AddEffect<Call>("Call", (pl, pb, e, node) =>
            {
                effectBase(pl, pb, e, node);
                if (node.Attributes["target-id"] != null) e.Target = pl.groupList[node.Attributes["target-id"].Value];
                else e.Target = pb.GetAnimation(node.Attributes["effect"].Value);
                //pl.SetParameter(node.Attributes["async"]?.Value, e.Async, pb, false, parameterConverter["Bool"]);
                pl.SetParameter(node.Attributes["timing"]?.Value, e.Timing, pb, false, parameterConverter["Double"]);
            });
            AddEffect<SetState>("SetState", (pl, pb, e, node) =>
            {
                effectBase(pl, pb, e, node);
                pl.SetParameter(node.Attributes["state"]?.Value, e.State, pb, false, parameterConverter["Status"]);
            });
            AddEffect<AnimAction>("Action", (pl, pb, e, node) =>
            {
                effectBase(pl, pb, e, node);
                pl.SetParameter(node.Attributes["name"]?.Value, e.Name, pb, false, parameterConverter["String"]);
            });
            AddEffect<PlaySound>("Sound", (pl, pb, e, node) =>
            {
                effectBase(pl, pb, e, node);
                pl.SetParameter(node.Attributes["file"]?.Value, e.File, pb, false, parameterConverter["String"]);
                pl.SetParameter(node.Attributes["volume"]?.Value, e.Volume, pb, false, parameterConverter["Double"]);
            });
            AddEffect<CloseEffect>("Close", (pl, pb, e, node) =>
            {
                effectBase(pl, pb, e, node);
            });
            AddEffect<ChannelEffect>("Channel", (pl, pb, e, node) =>
            {
                effectBase(pl, pb, e, node);
                pl.SetParameter(node.Attributes["name"]?.Value, e.Name, pb, false, parameterConverter["String"]);
            });

            #endregion
        }

        #endregion

        #region Data Container

        /// <summary>
        /// All loaded Prototypes
        /// </summary>
        public Dictionary<string, PrototypeBase> Prototypes { get; private set; }

        /// <summary>
        /// All loaded global objects
        /// </summary>
        public Dictionary<string, PrototypeBase> Objects { get; private set; }

        /// <summary>
        /// All loaded status
        /// </summary>
        public Dictionary<string, Status> Status { get; private set; }

        /// <summary>
        /// All loaded file names
        /// </summary>
        public List<string> LoadedFiles { get; private set; }

        #endregion

        #region Public Stuff

        /// <summary>
        /// Create a Loader for the ui
        /// </summary>
        public PrototypeLoader()
        {
            Prototypes = new Dictionary<string, PrototypeBase>();
            Objects = new Dictionary<string, PrototypeBase>();
            Status = new Dictionary<string, Status>();
            LoadedFiles = new List<string>();
            Prototypes.Add("PrototypeBase", new PrototypeBase());
        }

        /// <summary>
        /// Find a loaded status from its root key
        /// </summary>
        /// <param name="parts">keys to status</param>
        /// <returns>found status</returns>
        public Status FindStatus(string[] parts)
        {
            if (parts.Length == 0) return null;
            Status st;
            foreach (var s in Status)
                if (s.Key == parts[0] && (st = s.Value.Find(parts)) != null)
                    return st;
            return null;
        }

        /// <summary>
        /// Filter the loaded Data of this <see cref="PrototypeLoader"/>
        /// </summary>
        /// <param name="whitelistPrototypes">enabled Prototypes (if defined every need to be here)</param>
        /// <param name="whitelistObjects">enabled Objects (if defined every need to be here)</param>
        /// <param name="whitelistStatus">enabled Status (if defined every need to be here)</param>
        /// <param name="blacklistPrototypes">disabled prototypes (only if no whitelist)</param>
        /// <param name="blacklistObjects">disabled objects (only if no whitelist)</param>
        /// <param name="blacklistStatus">disabled status (only if no whitelist)</param>
        /// <returns>the new <see cref="PrototypeLoader"/> with the filtered data</returns>
        public PrototypeLoader Filter(string[] whitelistPrototypes, string[] whitelistObjects, string[] whitelistStatus,
            string[] blacklistPrototypes, string[] blacklistObjects, string[] blacklistStatus)
        {
            var pl = new PrototypeLoader();
            foreach (var p in Prototypes)
            {
                if (whitelistPrototypes.Length > 0 ? whitelistPrototypes.Contains(p.Key) : !blacklistPrototypes.Contains(p.Key))
                    pl.Prototypes.Add(p.Key, p.Value);
            }
            foreach (var o in Objects)
            {
                if (whitelistObjects.Length > 0 ? whitelistObjects.Contains(o.Key) : !blacklistObjects.Contains(o.Key))
                    pl.Objects.Add(o.Key, o.Value);
            }
            foreach (var s in Status)
            {
                if (whitelistStatus.Length > 0 ? whitelistStatus.Contains(s.Key) : !blacklistStatus.Contains(s.Key))
                    pl.Status.Add(s.Key, s.Value);
            }
            //ignore file list because data is filtered
            return pl;
        }

        /// <summary>
        /// Include the loaded Data from another <see cref="PrototypeLoader"/>
        /// </summary>
        /// <param name="other">the other loader with data</param>
        /// <param name="overwriteData">overwrite existing data (otherwise keep old data)</param>
        public void Include(PrototypeLoader other, bool overwriteData = false)
        {
            foreach (var p in other.Prototypes)
            {
                if (Prototypes.ContainsKey(p.Key))
                {
                    if (overwriteData && p.Value.GetType() != typeof(PrototypeBase)) Prototypes[p.Key] = p.Value;
                }
                else Prototypes.Add(p.Key, p.Value);
            }
            foreach (var o in other.Objects)
            {
                if (Objects.ContainsKey(o.Key))
                {
                    if (overwriteData) Objects[o.Key] = o.Value;
                }
                else Objects.Add(o.Key, o.Value);
            }
            foreach (var s in other.Status)
            {
                if (Status.ContainsKey(s.Key))
                {
                    if (overwriteData) Status[s.Key] = s.Value;
                }
                else Status.Add(s.Key, s.Value);
            }
            foreach (var file in other.LoadedFiles)
            {
                if (!LoadedFiles.Contains(file))
                    LoadedFiles.Add(file);
            }
        }

        /// <summary>
        /// Search for the real path of the references file name
        /// </summary>
        /// <param name="name">reference file name</param>
        /// <returns>real path</returns>
        private string GetLibPath(string name)
        {
            var paths = new string[]
            {
                "{0}",
                "{0}.xml",
                "lib\\{0}.xml",
                "lib\\{0}",
                "ui\\{0}.xml",
                "ui\\{0}",
                "visuals\\{0}.xml",
                "visuals\\{0}",
                "ui-temp\\{0}.xml",
                "ui-temp\\{0}"
            };
            foreach (var path in paths)
            {
                var file = string.Format(path, name);
                if (File.Exists(file)) return file;
            }
            return null;
        }

        Dictionary<string, IValueWrapper> idList = new Dictionary<string, IValueWrapper>();
        Dictionary<string, AnimationGroup> groupList = new Dictionary<string, AnimationGroup>();

        /// <summary>
        /// Load the data from a specific ui XML file
        /// </summary>
        /// <param name="file">the real file name (absolute or relative)</param>
        public void Load(string file)
        {
            file = new FileInfo(file).FullName;
            LoadedFiles.Add(file);
            var settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            var reader = XmlReader.Create(file, settings);
            var xml = new XmlDocument();
            xml.Load(reader);
            reader.Dispose();
            var node = xml["Objects"];
            //ClearComments(node);
            foreach (XmlNode sn in node.ChildNodes)
            {
                switch (sn.Name)
                {
                    case "Imports": ManageImports(sn); break;
                    case "Status": ManageStatus(sn); break;
                    case "Prototype":
                        if (sn.Attributes["dotnet"] != null) ManagePrototypeImport(sn);
                        else if (sn.Attributes["extends"] != null) ManagePrototypeExtension(sn);
                        else ManagePrototypeDeclaration(sn);
                        break;
                    case "Animation": ManageAnimation(sn); break;
                    default: ManageObjectCreation(sn); break;
                }
            }
            idList.Clear();
            groupList.Clear();
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// remove comments from the XML tree
        /// </summary>
        /// <param name="node"></param>
        void ClearComments(XmlNode node)
        {
            foreach (XmlNode sub in node)
            {
                if (sub.NodeType == XmlNodeType.Comment)
                    node.RemoveChild(sub);
                else ClearComments(sub);
            }
        }

        /// <summary>
        /// Load all imports from an import section
        /// </summary>
        /// <param name="node">XML node</param>
        void ManageImports(XmlNode node)
        {
            foreach (XmlNode entry in node.ChildNodes)
            {
                if (entry.Name == "File")
                {
                    var path = GetLibPath(entry.Attributes["name"].Value);
                    if (path == null)
                    {
                        if (entry.Attributes["require"] != null && !bool.Parse(entry.Attributes["require"].Value))
                            continue;
                        throw new FileNotFoundException("include file not found: " + entry.Attributes["name"].Value);
                    }
                    path = new FileInfo(path).FullName;
                    if (LoadedFiles.Contains(path)) continue;
                    if (entry.ChildNodes.Count > 0)
                    {
                        var pl = new PrototypeLoader();
                        pl.Load(path);
                        var wp = new List<string>();
                        var wo = new List<string>();
                        var ws = new List<string>();
                        var bp = new List<string>();
                        var bo = new List<string>();
                        var bs = new List<string>();
                        foreach (XmlNode rule in entry.ChildNodes)
                        {
                            switch (rule.Name)
                            {
                                case "IncludeProp": wp.Add(rule.Attributes["name"].Value); break;
                                case "IncludeObj": wo.Add(rule.Attributes["name"].Value); break;
                                case "IncludeStat": ws.Add(rule.Attributes["name"].Value); break;
                                case "ExcludeProp": bp.Add(rule.Attributes["name"].Value); break;
                                case "ExcludeObj": bo.Add(rule.Attributes["name"].Value); break;
                                case "ExcludeStat": bs.Add(rule.Attributes["name"].Value); break;
                                default: throw new NotImplementedException("import rule " + rule.Name + " not supported");
                            }
                        }
                        pl = pl.Filter(wp.ToArray(), wo.ToArray(), ws.ToArray(), bp.ToArray(), bo.ToArray(), bs.ToArray());
                        Include(pl);
                    }
                    else
                    {
                        var pl = new PrototypeLoader();
                        pl.Prototypes = Prototypes;
                        pl.Objects = Objects;
                        pl.Status = Status;
                        pl.LoadedFiles = LoadedFiles;
                        pl.Load(path);
                    }
                }
                else throw new NotImplementedException("the import mode " + entry.Name + " is not implemented");
            }
        }
        
        /// <summary>
        /// Load all status from an status section
        /// </summary>
        /// <param name="node">XML node</param>
        void ManageStatus(XmlNode node)
        {
            Status Parent = null;
            if (node.Attributes["extends"] != null)
            {
                var bname = node.Attributes["extends"].Value.Split('|');
                if (!Status.ContainsKey(bname[0])) throw new KeyNotFoundException("state '" + node.Attributes["extends"].Value + "' not found");
                Parent = Status[bname[0]].Find(bname) ?? throw new KeyNotFoundException("state '" + node.Attributes["extends"].Value + "' not found");
            }
            foreach (XmlNode v in node.ChildNodes)
            {
                if (v.Name != "Value") throw new NotImplementedException("status value type " + v.Name + " not supported");
                var st = new Status(v.InnerText);
                if ((st.Parent = Parent) == null)
                    Status.Add(st.Name, st);
                else Parent.SubStatus.Add(st);
            }
        }

        /// <summary>
        /// Load prototype imports
        /// </summary>
        /// <param name="node">XML node</param>
        void ManagePrototypeImport(XmlNode node)
        {
            var typename = node.Attributes["dotnet"].Value;
            var type = dotnetPrototypes.Find((t) => t.FullName == typename);
            if (type == null) throw new KeyNotFoundException("Dotnet type '" + typename + "' not registered");
            var prot = (PrototypeBase)Activator.CreateInstance(type);
            prot.ObjectName = node.Attributes["object-name"].Value;
            if (Prototypes.ContainsKey(prot.ObjectName))
                Prototypes[prot.ObjectName] = prot;
            else Prototypes.Add(prot.ObjectName, prot);
            foreach (var param in prot.Parameter)
            {
                var ptype = param.Value.GetType();
                param.Value.Name = parameterTypes.First((kvp) => kvp.Value == ptype).Key;
            }
        }

        /// <summary>
        /// Load prototype declaration
        /// </summary>
        /// <param name="node">XML node</param>
        void ManagePrototypeDeclaration(XmlNode node)
        {
            PrototypeBase prot;
            if (node.Attributes["base"] != null)
            {
                var name = node.Attributes["base"].Value;
                if (!Prototypes.ContainsKey(name)) throw new KeyNotFoundException("Prototype '" + name + "' is not defined");
                prot = Prototypes[name].Clone();
            }
            else prot = Prototypes["PrototypeBase"].Clone();

            SetupParameter(node, prot);
            SetupContainer(node, prot);
            SetupValues(node, prot);
            SetupAnimation(node, prot);

            var objName = node.Attributes["object-name"].Value;
            prot.ObjectName = objName;
            if (Prototypes.ContainsKey(objName)) Prototypes[objName] = prot;
            else Prototypes.Add(objName, prot);
        }

        /// <summary>
        /// Load prototype extension
        /// </summary>
        /// <param name="node">XML node</param>
        void ManagePrototypeExtension(XmlNode node)
        {
            var name = node.Attributes["extends"].Value;
            if (!Prototypes.ContainsKey(name)) throw new KeyNotFoundException("Protoype '" + name + "' doesn't exists");
            var prot = Prototypes[name];

            var mode = node.Attributes["extendtype"]?.Value ?? "append";
            switch (mode)
            {
                case "append":
                    {
                        SetupParameter(node, prot);
                        SetupContainer(node, prot);
                        SetupValues(node, prot);
                        SetupAnimation(node, prot);
                    } break;
                default: throw new NotImplementedException("extension mode '" + mode + "' not implemented");
            }
        }

        /// <summary>
        /// Load animations
        /// </summary>
        /// <param name="node">XML node</param>
        void ManageAnimation(XmlNode node)
        {
            var name = node.Attributes["targetname"].Value.Split('|');
            var type = node.Attributes["target"].Value;
            PrototypeBase prot;
            switch (type)
            {
                case "object":
                    if (!Objects.ContainsKey(name[0])) throw new KeyNotFoundException("Object '" + name[0] + "' doesn't exists");
                    prot = Objects[name[0]];
                    break;
                case "prototype":
                    if (!Prototypes.ContainsKey(name[0])) throw new KeyNotFoundException("Prototype '" + name[0] + "'doesn't exists");
                    prot = Prototypes[name[0]];
                    break;
                default: throw new NotImplementedException("Prototype source mode '" + type + "' not implemented");
            }
            if (name.Length > 1) prot = prot.GetElement(name[1]);
            if (name.Length > 2) throw new NotImplementedException("Prototype source path of length " + name.Length.ToString() + " not implemented");

            foreach (XmlNode group in node.ChildNodes)
            {
                switch (group.Attributes["extendtype"].Value)
                {
                    case "append":
                        {
                            var anim = prot.GetAnimation(group.Attributes["extend"].Value);
                            if (group["Activation"] != null)
                                foreach (XmlNode act in group["Activation"].ChildNodes)
                                    anim.Activations.Add(GetActivation(act, prot));
                            if (group["Effects"] != null)
                            {
                                foreach (XmlNode eff in group["Effects"].ChildNodes)
                                {
                                    var target = eff.Attributes["target"] == null ? prot : prot.GetElement(eff.Attributes["target"].Value);
                                    anim.Effects.Add(GetEffect(eff, target));
                                }
                                var timing = group["Effects"].Attributes["timing"]?.Value;
                                if (timing != null)
                                    SetParameter(timing, anim.EffectTiming, prot, false, parameterConverter["Double"]);
                            }
                            else anim.EffectTiming.Value = 1.0;
                        } break;
                    case "new":
                        {
                            var anim = new AnimationGroup();
                            anim.Name = group.Attributes["name"].Value;
                            prot.Animations.Add(anim);
                            if (group["Activation"] != null)
                                foreach (XmlNode act in group["Activation"].ChildNodes)
                                    anim.Activations.Add(GetActivation(act, prot));
                            if (group["Effects"] != null)
                            {
                                foreach (XmlNode eff in group["Effects"].ChildNodes)
                                {
                                    var target = eff.Attributes["target"] == null ? prot : prot.GetElement(eff.Attributes["target"].Value);
                                    anim.Effects.Add(GetEffect(eff, target));
                                }
                                SetParameter(group["Effects"].Attributes["timing"].Value, anim.EffectTiming, prot, false, parameterConverter["Double"]);
                            }
                            else anim.EffectTiming.Value = 1.0;
                        } break;
                    default: throw new NotImplementedException("Extend type '" + group.Attributes["extendtype"].Value + "' not implemented");
                }
            }
        }

        /// <summary>
        /// Load object creations
        /// </summary>
        /// <param name="node">XML node</param>
        void ManageObjectCreation(XmlNode node)
        {
            if (!Prototypes.ContainsKey(node.Name)) throw new KeyNotFoundException("Protoype '" + node.Name + "' doesn't exists");
            var prot = Prototypes[node.Name].Clone();

            SetupParameter(node, prot);
            SetupContainer(node, prot);
            SetupValues(node, prot);
            SetupAnimation(node, prot);

            var name = node.Attributes["name"]?.Value;
            prot.Name = name;
            if (Objects.ContainsKey(name)) Objects[name] = prot;
            else Objects.Add(name, prot);
        }

        /// <summary>
        /// load parameter values
        /// </summary>
        /// <param name="node">XML node</param>
        /// <param name="prot">current prototype</param>
        void SetupParameter(XmlNode node, PrototypeBase prot)
        {
            if (node["Parameter"] != null)
                foreach (XmlNode param in node["Parameter"].ChildNodes)
                {
                    IValueWrapper parameter;
                    if (param.Name == "Math")
                    {
                        parameter = CreateMathValue(param, prot);
                        parameter.Name = param.Name;
                    }
                    else
                    {
                        var type = parameterTypes[param.Name];
                        if (type == null) throw new KeyNotFoundException("Parameter type '" + param.Name + "' is not registered");
                        parameter = (IValueWrapper)Activator.CreateInstance(type);
                        parameter.Name = param.Name;
                        SetParameter(param.Attributes["value"]?.Value, parameter, prot.Parent);
                    }
                    if (param.Attributes["id"] != null)
                    {
                        idList.Add(param.Attributes["id"].Value, parameter);
                    }
                    var name = param.Attributes["name"].Value;
                    if (prot.Parameter.ContainsKey(name)) prot.Parameter[name] = parameter;
                    else prot.Parameter[name] = parameter;
                }
        }

        /// <summary>
        /// Load children prototypes of prototype
        /// </summary>
        /// <param name="node">XML node</param>
        /// <param name="prot">current prototype</param>
        void SetupContainer(XmlNode node, PrototypeBase prot)
        {
            if (node["Container"] != null)
                foreach (XmlNode param in node["Container"].ChildNodes)
                    prot.Container.Add(CreateInstace(param, prot));
        }

        /// <summary>
        /// set values of prototype
        /// </summary>
        /// <param name="node">XML node</param>
        /// <param name="prot">current Prototype</param>
        void SetupValues(XmlNode node, PrototypeBase prot)
        {
            if (node["ParameterSet"] != null)
                foreach (XmlNode param in node["ParameterSet"].ChildNodes)
                    SetupValueDirect(param, prot);
            var ignore = new string[]
            {
                "Container", "Animation", "Parameter", "ParameterSet"
            };
            foreach (XmlNode param in node.ChildNodes)
            {
                if (!ignore.Contains(param.Name))
                    SetupValueDirect(param, prot);
            }
        }

        /// <summary>
        /// Set the Values direct
        /// </summary>
        /// <param name="param">XML node</param>
        /// <param name="prot">current Prototype</param>
        void SetupValueDirect(XmlNode param, PrototypeBase prot)
        {
            var p = prot.GetParameter(param.Name);
            if (p == null) throw new KeyNotFoundException("Parameter '" + param.Name + "' not found");
            if (param.Attributes["ref"] != null)
                SetParameter(param.Attributes["ref"].Value, p, prot, true);
            else if (param.Attributes["value"] != null)
            {
                p.Value = parameterConverter[p.Name].DynamicInvoke(this, prot, param.Attributes["value"].Value);
                p.Exists = true;
            }
            else SetParameter(param.InnerText, p, prot.Parent);
        }

        /// <summary>
        /// Load animation for prototype
        /// </summary>
        /// <param name="node">XML node</param>
        /// <param name="prot">current Prototype</param>
        void SetupAnimation(XmlNode node, PrototypeBase prot)
        {
            if (node["Animation"] != null)
                foreach (XmlNode group in node["Animation"].ChildNodes)
                {
                    var anim = new AnimationGroup();
                    anim.Name = group.Attributes["name"]?.Value;
                    prot.Animations.Add(anim);
                    if (group.Attributes["id"] != null) groupList.Add(group.Attributes["id"].Value, anim);
                    if (group["Activation"] != null)
                        foreach (XmlNode act in group["Activation"].ChildNodes)
                            anim.Activations.Add(GetActivation(act, prot));
                    if (group["Effects"] != null)
                    {
                        foreach (XmlNode eff in group["Effects"].ChildNodes)
                        {
                            var target = eff.Attributes["target"] == null ? prot : prot.GetElement(eff.Attributes["target"].Value);
                            anim.Effects.Add(GetEffect(eff, target));
                        }
                        SetParameter(group["Effects"].Attributes["timing"]?.Value, anim.EffectTiming, prot, false, parameterConverter["Double"]);
                    }
                    else anim.EffectTiming.Value = 1.0;
                }
        }

        /// <summary>
        /// Load activation
        /// </summary>
        /// <param name="node">XML Node</param>
        /// <param name="prot">current Prototype</param>
        /// <returns></returns>
        AnimationActivation GetActivation(XmlNode node, PrototypeBase prot)
        {
            if (!activators.ContainsKey(node.Name)) throw new KeyNotFoundException("Activation '" + node.Name + "' is not registered");
            var d = activators[node.Name];
            var a = (AnimationActivation)Activator.CreateInstance(d.Item1);
            d.Item2.DynamicInvoke(this, prot, a, node);
            return a;
        }

        /// <summary>
        /// Load effect
        /// </summary>
        /// <param name="node">XML Node</param>
        /// <param name="prot">current Prototype</param>
        /// <returns></returns>
        AnimationEffect GetEffect(XmlNode node, PrototypeBase prot)
        {
            if (!effects.ContainsKey(node.Name)) throw new KeyNotFoundException("Effect '" + node.Name + "' is not registered");
            var d = effects[node.Name];
            var e = (AnimationEffect)Activator.CreateInstance(d.Item1);
            d.Item2.DynamicInvoke(this, prot, e, node);
            return e;
        }

        /// <summary>
        /// Create a child Prototype from its parent prototype and setup values
        /// </summary>
        /// <param name="node">XML node</param>
        /// <param name="parent">Parent Prototype</param>
        /// <returns></returns>
        PrototypeBase CreateInstace(XmlNode node, PrototypeBase parent)
        {
            if (!Prototypes.ContainsKey(node.Name)) throw new KeyNotFoundException("Prototype '" + node.Name + "' doesn't exists");
            var prot = Prototypes[node.Name].Clone();
            if (node.Attributes["name"] != null)
                prot.Name = node.Attributes["name"].Value;
            prot.Parent = parent;

            SetupParameter(node, prot);
            SetupContainer(node, prot);
            SetupValues(node, prot);
            SetupAnimation(node, prot);

            return prot;
        }

        /// <summary>
        /// Load the single value of the parameter
        /// </summary>
        /// <param name="value">value text</param>
        /// <param name="target">target value container</param>
        /// <param name="current">current Prototype</param>
        /// <param name="forceref">force to have a reference</param>
        /// <param name="customConverter">convert the string to its value</param>
        void SetParameter(string value, IValueWrapper target, PrototypeBase current, bool forceref = false, Delegate customConverter = null)
        {
            if (value == null)
            {
                target.Exists = false;
                return;
            }
            if (value.StartsWith("$"))
            {
                var name = value.Substring(1);
                if (!SystemValues.ContainsKey(name)) throw new KeyNotFoundException("System value '" + name + "' doesn't exists");
                target.RemoteSource = SystemValues[name];
                target.Exists = true;
            }
            else if (value.StartsWith("@"))
            {
                var name = value.Substring(1);
                for (var c = current; c != null; c = c.Parent)
                {
                    var param = c.GetParameter(name);
                    if (param == null) continue;
                    target.RemoteSource = param;
                    target.Exists = true;
                    break;
                }
            }
            else if (value.StartsWith("#"))
            {
                var name = value.Substring(1);
                if (!idList.ContainsKey(name)) throw new KeyNotFoundException("Id '" + name + "' not found");
                target.RemoteSource = idList[name];
                target.Exists = true;
            }
            else
            {
                if (forceref) throw new ArgumentException("'" + value + "' is not a valid reference name");
                else target.Value = (customConverter ?? parameterConverter[target.Name]).DynamicInvoke(this, current, value);
                target.Exists = true;
            }
        }

        /// <summary>
        /// Load a math value context
        /// </summary>
        /// <param name="node">XML node</param>
        /// <param name="parent">current Prototype</param>
        /// <returns>value container</returns>
        IValueWrapper CreateMathValue(XmlNode node, PrototypeBase parent)
        {
            var type = mathParameterTypes[node.Attributes["type"].Value];
            var prop = type.GetProperty("ValueSource");
            var math = (IValueWrapper)Activator.CreateInstance(type);
            if (node.ChildNodes.Count != 1) throw new Exception("math value requires exact one value generator or value");
            prop.SetValue(math, GenerateSubMathValue(node.FirstChild, parent));
            type.GetProperty("ValueType").SetValue(math, node.Attributes["type"].Value);
            return math;
        }

        /// <summary>
        /// Load Functions of Math context
        /// </summary>
        /// <param name="node">XML node</param>
        /// <param name="parent">current Prototype</param>
        /// <returns>value container</returns>
        IValueWrapper GenerateSubMathValue(XmlNode node, PrototypeBase parent)
        {
            if (node.Name == "Calc")
            {
                var calc = new Calc();
                if (node.Attributes["precompile"] != null)
                    calc.Precompile = bool.Parse(node.Attributes["precompile"].Value);
                calc.Name = "Calc";
                calc.Method = (CalcMethod)Enum.Parse(typeof(CalcMethod), node.Attributes["method"].Value);
                calc.Type = (CalcType)Enum.Parse(typeof(CalcType), node.Attributes["type"].Value);
                if (!calc.ValidMethod()) throw new InvalidOperationException("Calc element cannot perform operation " + calc.Method.ToString() + " on " + calc.Type.ToString());
                foreach (XmlNode sub in node.ChildNodes)
                    calc.ValueList.Add(GenerateSubMathValue(sub, parent));
                return calc;
            }
            if (node.Name == "If")
            {
                var val = new If();
                if (node.Attributes["precompile"] != null)
                    val.Precompile = bool.Parse(node.Attributes["precompile"].Value);
                val.Name = "If";
                if (node["Condition"].ChildNodes.Count != 1) throw new Exception("if condition requires exact one value generator or value");
                val.Condition = GenerateSubMathValue(node["Condition"].FirstChild, parent);
                if (node["True"].ChildNodes.Count != 1) throw new Exception("if true requires exact one value generator or value");
                val.True = GenerateSubMathValue(node["True"].FirstChild, parent);
                if (node["False"].ChildNodes.Count != 1) throw new Exception("if false requires exact one value generator or value");
                val.False = GenerateSubMathValue(node["False"].FirstChild, parent);
                return val;
            }
            if (node.Name == "Check")
            {
                var check = new Check();
                if (node.Attributes["precompile"] != null)
                    check.Precompile = bool.Parse(node.Attributes["precompile"].Value);
                check.Name = "Check";
                check.Mode = (CheckMode)Enum.Parse(typeof(CheckMode), node.Attributes["method"].Value);
                if (node.ChildNodes.Count != 2) throw new Exception("check requires exact two value generator or values");
                check.Value1 = GenerateSubMathValue(node.ChildNodes[0], parent);
                check.Value2 = GenerateSubMathValue(node.ChildNodes[1], parent);
                return check;
            }
            else
            {
                var type = parameterTypes[node.Name];
                if (type == null) throw new KeyNotFoundException("Parameter type '" + node.Name + "' is not registered");
                var parameter = (IValueWrapper)Activator.CreateInstance(type);
                parameter.Name = node.Name;
                if (node.Attributes["ref"] != null)
                    SetParameter(node.Attributes["ref"]?.Value, parameter, parent, true);
                else if (node.Attributes["value"] != null)
                {
                    parameter.Value = parameterConverter[parameter.Name].DynamicInvoke(this, parent, node.Attributes["value"].Value);
                    parameter.Exists = true;
                }
                else SetParameter(node.InnerText, parameter, parent);
                return parameter;
            }
        }

        #endregion
    }
}
