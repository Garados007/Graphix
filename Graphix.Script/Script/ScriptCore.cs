using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint;
using System.IO;

namespace Graphix.Script
{
    /// <summary>
    /// This core manage the runtime for execution of JS files.
    /// </summary>
    public class ScriptCore
    {
        internal Engine engine;
        /// <summary>
        /// The current Graphix core
        /// </summary>
        public Physic.AnimationRuntime Runtime { get; private set; }

        /// <summary>
        /// A class for the JS scripts that provide access to the Graphix core
        /// </summary>
        public ScriptAccess Access { get; private set; }

        /// <summary>
        /// Create a new <see cref="ScriptCore"/>
        /// </summary>
        /// <param name="runtime">The current Graphix core</param>
        /// <param name="config">Configuration parameter for the JS runtime</param>
        public ScriptCore(Physic.AnimationRuntime runtime, CoreConfig config = null)
        {
            config = config ?? new CoreConfig();
            Runtime = runtime ?? throw new ArgumentNullException("runtime");
            engine = new Engine();
            Access = new ScriptAccess(this);

            engine.SetValue(config.AccessVariable, Access);
            foreach (var t in ScriptAccess.CoreTypes)
            {
                engine.SetValue(t.Key, t.Value);
            }
            //log stuff in the local log file
            engine.SetValue("log", new Action<string>(JsLog));
            //log an exception
            engine.SetValue("log", new Action<Exception>(JsError));
            //execute a funtion in some ms. This function is simular to the JS on web
            engine.SetValue("setTimeout", new Func<Action, int, int>(SetTimeout));
            //execute a funtion repeadly after some ms. This function is simular to the JS on web
            engine.SetValue("setInterval", new Func<Action, int, int>(SetInterval));
            //stop execution of function. This function is simular to the JS on web
            engine.SetValue("clearTimeout", new Action<int>(ClearTimeout));
            //stop execution of function. This function is simular to the JS on web
            engine.SetValue("clearInterval", new Action<int>(ClearTimeout));
            //run this function asynchronosly now
            engine.SetValue("run", new Action<Action>(Run));
            //cast a variable to a given type
            engine.SetValue("cast", new Func<Type, object, object>(Cast));
            //load a script file
            engine.SetValue("load", new Action<string>(LoadFile));
        }

        /// <summary>
        /// Load a local script file and execute it.
        /// </summary>
        /// <param name="fileName">the name of the script file</param>
        public void LoadFile(string fileName)
        {
            if (!File.Exists(fileName)) throw new FileNotFoundException("cannot find file", fileName);
            var content = File.ReadAllText(fileName);
            try
            {
                Load(content);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw new InvalidDataException("error at execution of " + fileName, e);
            }
        }

        /// <summary>
        /// Execute the given script code
        /// </summary>
        /// <param name="code">the code to execute</param>
        public void Load(string code)
        {
            engine.Execute(code);
        }

        private void JsLog(string text)
        {
            Logger.Log("{JS} " + text);
        }

        private void JsError(Exception e)
        {
            Logger.Error(e);
        }

        object lockCancel = new object();
        Dictionary<int, bool> canceList = new Dictionary<int, bool>();
        int nextID = 0;

        private int SetTimeout(Action action, int timeout)
        {
            var id = nextID++;
            lock (lockCancel)
                canceList[id] = true;
            Task.Run(async () =>
            {
                await Task.Delay(timeout);
                bool enable;
                lock (lockCancel)
                {
                    enable = canceList[id];
                    canceList.Remove(id);
                }
                if (enable) action();
            });
            return id;
        }

        private void ClearTimeout(int id)
        {
            lock (lockCancel)
            {
                if (canceList.ContainsKey(id))
                    canceList[id] = false;
            }
        }

        private int SetInterval(Action action, int timeout)
        {
            var id = nextID++;
            lock (lockCancel)
                canceList[id] = true;
            Task.Run(async () =>
            {
                bool enable = true;
                while (enable)
                {
                    await Task.Delay(timeout);
                    lock (lockCancel)
                        enable = canceList[id];
                    if (enable) action();
                }
                lock (lockCancel)
                    canceList.Remove(id);
            });
            return id;
        }

        private void Run(Action action)
        {
            Task.Run(action);
        }


        private object Cast(Type type, object value)
        {
            if (type.IsArray)
            {
                var av = (Array)value;
                var na = (Array)Activator.CreateInstance(type, av.Length);
                var st = type.GetElementType();
                for (var i = 0; i < av.Length; ++i)
                    na.SetValue(Cast(st, av.GetValue(i)), i);
                return na;
            }
            //if (value.GetType() == typeof(System.Numerics.BigInteger))
            //{
            //    if (type == typeof(double))
            //        return (double)(System.Numerics.BigInteger)value;
            //    if (type == typeof(long))
            //        return (long)(System.Numerics.BigInteger)value;
            //    throw new InvalidCastException();
            //}
            else
            {
                if (value.GetType() == type) return value;
                else return Convert.ChangeType(value, type);
            }
        }
    }

    /// <summary>
    /// This class store configuration information for <see cref="ScriptCore"/>.
    /// </summary>
    public class CoreConfig
    {
        /// <summary>
        /// Define the variable for <see cref="ScriptAccess"/> inside the js scripts.
        /// Normaly this variable is $.
        /// </summary>
        public string AccessVariable { get; set; }

        /// <summary>
        /// Created <see cref="CoreConfig"/>
        /// </summary>
        public CoreConfig()
        {
            AccessVariable = "$";
        }
    }
}
