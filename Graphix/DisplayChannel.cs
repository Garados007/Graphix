using Graphix.Physic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix
{
    /// <summary>
    /// Channels contains all information about the current ui state. Channels can be swapped. Only unloaded 
    /// channels can be edited.
    /// </summary>
    public class DisplayChannel
    {
        internal enum UseMode
        {
            Unused,
            Used,
            Cooldown
        }

        /// <summary>
        /// Wrapper of <see cref="List{T}"/> to enable protected access.
        /// </summary>
        /// <typeparam name="T">Element type of <see cref="List{T}"/></typeparam>
        public class ProtectedList<T> : IList<T>
        {
            internal List<T> source;
            internal DisplayChannel lockChannel;

            public T this[int index]
            {
                get => ((IList<T>)source)[index];
                set
                {
                    if (!lockChannel.IsUnused) throw new AccessViolationException();
                    ((IList<T>)source)[index] = value;
                }
            }

            public int Count => ((IList<T>)source).Count;

            public bool IsReadOnly => !lockChannel.IsUnused;

            public void Add(T item)
            {
                if (!lockChannel.IsUnused) throw new AccessViolationException();
                ((IList<T>)source).Add(item);
            }

            public void Clear()
            {
                if (!lockChannel.IsUnused) throw new AccessViolationException();
                ((IList<T>)source).Clear();
            }

            public bool Contains(T item)
            {
                return ((IList<T>)source).Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                ((IList<T>)source).CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return ((IList<T>)source).GetEnumerator();
            }

            public int IndexOf(T item)
            {
                return ((IList<T>)source).IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                if (!lockChannel.IsUnused) throw new AccessViolationException();
                ((IList<T>)source).Insert(index, item);
            }

            public bool Remove(T item)
            {
                if (!lockChannel.IsUnused) throw new AccessViolationException();
                return ((IList<T>)source).Remove(item);
            }

            public void RemoveAt(int index)
            {
                if (!lockChannel.IsUnused) throw new AccessViolationException();
                ((IList<T>)source).RemoveAt(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IList<T>)source).GetEnumerator();
            }
        }

        /// <summary>
        /// The name of the current channel. Its not required to be unique.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The current state of the ui logic
        /// </summary>
        public Status CurrentStatus { get; internal set; }

        /// <summary>
        /// A list of all states.
        /// </summary>
        public Dictionary<string, Status> Status { get; private set; }

        internal List<FlatPrototype> ObjectsInternal { get; private set; }

        /// <summary>
        /// A list of all loaded objects in this channel. This list has protected
        /// access. If the channel is loaded any modifications to this list will throw
        /// exceptions.
        /// </summary>
        public ProtectedList<FlatPrototype> Objects { get; private set; }

        internal List<AnimationGroup> Animations { get; private set; }

        internal List<AnimationRuntime.RunningAnimation> Running { get; private set; }

        /// <summary>
        /// Create new <see cref="DisplayChannel"/>
        /// </summary>
        public DisplayChannel()
        {
            Name = "";
            Status = new Dictionary<string, Status>();
            ObjectsInternal = new List<FlatPrototype>();
            Objects = new ProtectedList<FlatPrototype>
            {
                lockChannel = this,
                source = ObjectsInternal
            };
            Animations = new List<AnimationGroup>();
            Running = new List<AnimationRuntime.RunningAnimation>();
        }

        /// <summary>
        /// Search for a state with the given name and return them
        /// </summary>
        /// <param name="fullName">the complete status name</param>
        /// <returns>the found state</returns>
        public Status GetStatus(string fullName)
        {
            if (fullName == null) return null;
            var parts = fullName.Split('|');
            if (!Status.ContainsKey(parts[0])) return null;
            var st = Status[parts[0]];
            return st.Find(parts);
        }

        /// <summary>
        /// Import an ui logic. This will only work if this channel is
        /// unloaded.
        /// </summary>
        /// <param name="prototypes"></param>
        public void Import(PrototypeLoader prototypes)
        {
            if (!IsUnused) throw new AccessViolationException();
            var exp = new PrototypeExporter();
            exp.ImportFlatten(prototypes);
            foreach (var obj in exp.Objects)
            {
                ObjectsInternal.Add(obj);
                LoadAnimations(obj);
            }
            foreach (var st in exp.Status)
                Status[st.Key] = st.Value;
        }

        void LoadAnimations(FlatPrototype prot)
        {
            foreach (var obj in prot.Container)
                LoadAnimations(obj);
            foreach (var anim in prot.Animations)
                Register(anim);
        }

        /// <summary>
        /// Register an animation. This will only work if this channel is
        /// unloaded.
        /// </summary>
        /// <param name="animation">the animation to register</param>
        public void Register(AnimationGroup animation)
        {
            if (!IsUnused) throw new AccessViolationException();
            if (!Animations.Contains(animation))
            {
                Animations.Add(animation);
                foreach (var act in animation.Activations)
                    if (act is AfterAnimation)
                    {
                        var anim = act as AfterAnimation;
                        anim.Effect.HookedAnimations.Add(anim);
                    }
            }
        }

        /// <summary>
        /// Clear a registration of an animation. This will only work if this channel is
        /// unloaded.
        /// </summary>
        /// <param name="animation">the animation to unregister</param>
        public void UnRegister(AnimationGroup animation)
        {
            if (!IsUnused) throw new AccessViolationException();
            Animations.Remove(animation);
            foreach (var act in animation.Activations)
                if (act is AfterAnimation)
                {
                    var anim = act as AfterAnimation;
                    anim.Effect.HookedAnimations.Remove(anim);
                }
        }

        int usecounter = 0;
        UseMode useMode = UseMode.Unused;
        internal bool Using
        {
            get => useMode == UseMode.Used;
            set
            {
                if (value == Using) return;
                var counter = ++usecounter;
                if (value)
                {
                    useMode = UseMode.Used;
                }
                else
                {
                    useMode = UseMode.Cooldown;
                    Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        if (counter == usecounter)
                            useMode = UseMode.Unused;
                    });
                }
            }
        }

        /// <summary>
        /// Checks if this channel is used from the ui. If this flag is on
        /// any changes to this channel are forbidden.
        /// </summary>
        public bool IsUsed => useMode == UseMode.Used;

        /// <summary>
        /// Checks if this channel is in cooldown mode. If this channel is removed from the 
        /// ui a timer will start and lock it for 1 second. This time intervall is used for
        /// tasks that started their work when this channel was active and need more time.
        /// </summary>
        public bool IsCoolDown => useMode == UseMode.Cooldown;

        /// <summary>
        /// This channel is unused from the ui and no cooldown is active.
        /// </summary>
        public bool IsUnused => useMode == UseMode.Unused;
    }
}
