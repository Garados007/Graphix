using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphix.Internal;
using Graphix.Prototypes;
using Graphix.Physic;
using System.IO;
using Jint.Native;

namespace Graphix.Script
{
    /// <summary>
    /// This class provide the JS code lots of usefull functions. These functions are bundled under 
    /// <see cref="CoreConfig.AccessVariable"/> (normaly $).
    /// </summary>
    public class ScriptAccess
    {
        internal ScriptCore Core { get; private set; }

        internal static Dictionary<string, Type> CoreTypes;

        /// <summary>
        /// Creates a new <see cref="ScriptAccess"/>
        /// </summary>
        /// <param name="core">The script core with manage the script codes</param>
        public ScriptAccess(ScriptCore core)
        {
            Core = core ?? throw new ArgumentNullException("core");
        }

        static bool registed = false;
        /// <summary>
        /// Register the provides Types and functions to the Graphix core. This would be done
        /// automaticly after <see cref="ScriptAccess"/> is first used.
        /// </summary>
        public static void Register()
        {
            if (registed) return;
            registed = true;
            //a list of all types the js is allowed to create
            CoreTypes = new Dictionary<string, Type>
            {
                { "int", typeof(int) },
                { "double", typeof(double) },
                { "string", typeof(string) },
                { "bool", typeof(bool) },
                { "PrototypeLoader", typeof(PrototypeLoader) },
                { "DisplayChannel", typeof(DisplayChannel) },
                { "AnimationGroup", typeof(AnimationGroup) },
            };
            //register activator and effect
            PrototypeLoader.AddActivator<ScriptActivator>("Script", (pl, pb, a, node) =>
            {
                PrototypeLoaderAccess.SetParameter(pl, node.Attributes["enable"]?.Value, a.Enabled, pb, false, PrototypeLoaderAccess.ParameterConverter["Bool"]);
                PrototypeLoaderAccess.SetParameter(pl, node.Attributes["key"]?.Value, a.Key, pb, false, PrototypeLoaderAccess.ParameterConverter["String"]);
            });
            PrototypeLoader.AddEffect<ScriptEffect>("Script", (pl, pb, e, node) =>
            {
                PrototypeLoaderAccess.EffectBase(pl, pb, e, node);
                PrototypeLoaderAccess.SetParameter(pl, node.Attributes["code"]?.Value, e.Code, pb, false, PrototypeLoaderAccess.ParameterConverter["String"]);
                var code = node.InnerText.Trim();
                if (code != "")
                {
                    e.Code.Value = code;
                    e.Code.Exists = true;
                }
            });
        }

        static ScriptAccess()
        {
            Register();
        }

        /// <summary>
        /// The connection to the main Grafix core
        /// </summary>
        public AnimationRuntime Runtime => Core.Runtime;

        /// <summary>
        /// Load a registered .NET prototype.
        /// </summary>
        /// <param name="name">the type name of the prototype</param>
        /// <returns>the loaded prototype</returns>
        public PrototypeBase LoadPrototype(string name)
        {
            var type = PrototypeLoaderAccess.DotnetPrototypes.Find((t) => t.FullName == name);
            if (type == null) return null;
            try
            {
                return (PrototypeBase)Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        /// <summary>
        /// Get a loaded prototype from the loader
        /// </summary>
        /// <param name="prototypeLoader">the loader with the loaded prototypes</param>
        /// <param name="name">The name of the searches prototype</param>
        /// <returns>the loaded prototype</returns>
        public PrototypeBase LoadPrototype(PrototypeLoader prototypeLoader, string name)
        {
            if (prototypeLoader == null) throw new ArgumentNullException("prototypeLoader");
            if (name == null) throw new ArgumentNullException("name");

            if (prototypeLoader.Prototypes.TryGetValue(name, out PrototypeBase prototype))
                return prototype;
            else return null;
        }

        /// <summary>
        /// Create a value wrapper that can contain and bind values. The name need to be registred first
        /// </summary>
        /// <param name="prototypeLoader">a prototype loader (could be null)</param>
        /// <param name="prototype">a prototype that could contains this value (could be null)</param>
        /// <param name="name">the name of the type for this value wrapper</param>
        /// <param name="value">the string text for the value for this wrapper</param>
        /// <returns>a loaded value wrapper</returns>
        public IValueWrapper CreateValue(PrototypeLoader prototypeLoader, PrototypeBase prototype, string name, string value)
        {
            var type = PrototypeLoaderAccess.ParameterTypes[name];
            if (type == null) throw new KeyNotFoundException("Parameter type '" + name + "' is not registered");

            var parameter = (IValueWrapper)Activator.CreateInstance(type);
            parameter.Name = name;

            parameter.Value = PrototypeLoaderAccess.ParameterConverter[name].DynamicInvoke(prototypeLoader, prototype, value);
            parameter.Exists = true;

            return parameter;
        }

        /// <summary>
        /// Transform a value string to the target value using the registered conversion functions
        /// </summary>
        /// <param name="prototypeLoader">a prototype loader (could be null)</param>
        /// <param name="prototype">a prototype that could contains this value (could be null)</param>
        /// <param name="name">the name of the type</param>
        /// <param name="value">the string representation</param>
        /// <returns>converted value</returns>
        public object TransformValue(PrototypeLoader prototypeLoader, PrototypeBase prototype, string name, string value)
        {
            return PrototypeLoaderAccess.ParameterConverter[name].DynamicInvoke(prototypeLoader, prototype, value);
        }

        /// <summary>
        /// Create an activator for animation
        /// </summary>
        /// <param name="name">the name for the activator</param>
        /// <returns>the created activator</returns>
        public AnimationActivation CreateActivation(string name)
        {
            if (PrototypeLoaderAccess.Activators.TryGetValue(name, out Tuple<Type, Delegate> value))
                return (AnimationActivation)Activator.CreateInstance(value.Item1);
            else return null;
        }

        /// <summary>
        /// Create an effect for animation
        /// </summary>
        /// <param name="name">the name for the effect</param>
        /// <returns>the created effect</returns>
        public AnimationEffect CreateEffect(string name)
        {
            if (PrototypeLoaderAccess.Effects.TryGetValue(name, out Tuple<Type, Delegate> value))
                return (AnimationEffect)Activator.CreateInstance(value.Item1);
            else return null;
        }

        /// <summary>
        /// Get a loaded object from the current channel
        /// </summary>
        /// <param name="name">the name of the object</param>
        /// <returns>loaded object</returns>
        public FlatPrototype GetObject(string name)
        {
            return Core.Runtime.Channel.Objects.FirstOrDefault((p) => p.Name == name);
        }

        /// <summary>
        /// Get a loaded object from a specific channel
        /// </summary>
        /// <param name="channel">the channel</param>
        /// <param name="name">the name of the object</param>
        /// <returns>loaded object</returns>
        public FlatPrototype GetObject(DisplayChannel channel, string name)
        {
            return channel.Objects.FirstOrDefault((p) => p.Name == name);
        }

        /// <summary>
        /// Get a loaded object from a prototype loader
        /// </summary>
        /// <param name="prototypeLoader">the prototype loader</param>
        /// <param name="name">the name of the object</param>
        /// <returns>loaded object</returns>
        public PrototypeBase GetObject(PrototypeLoader prototypeLoader, string name)
        {
            return prototypeLoader.Objects[name];
        }

        /// <summary>
        /// Execute all animations with <see cref="ScriptActivator"/> that use the given key
        /// </summary>
        /// <param name="key">key for animation activation</param>
        public void RunAnimation(string key)
        {
            Core.Runtime.RunActivator<ScriptActivator>((act) =>
                !act.Key.Exists || act.Key.Value == key
            );
        }

        /// <summary>
        /// Load stored data on the hard drive
        /// </summary>
        /// <param name="fileName">the local file name</param>
        /// <returns>the loaded data or null if not found</returns>
        public JsValue LoadData(string fileName)
        {
            if (!File.Exists(fileName)) return JsValue.Null;
            return Core.engine.Json.Parse(JsValue.Null, new JsValue[]
            {
                JsValue.FromObject(Core.engine, File.ReadAllText(fileName))
            });
        }

        /// <summary>
        /// Save the data on the local hard drive
        /// </summary>
        /// <param name="fileName">the name of the file</param>
        /// <param name="data">the data to save</param>
        public void SaveData(string fileName, JsValue data)
        {
            var code = Core.engine.Json.Stringify(JsValue.Null, new JsValue[]
            {
                data
            });
            File.WriteAllText(fileName, code.AsString());
        }
    }
}
