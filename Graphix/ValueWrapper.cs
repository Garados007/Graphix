using System;

namespace Graphix
{
    /// <summary>
    /// Wrapps a single value and make it easier to manipulate and reference it
    /// </summary>
    public interface IValueWrapper
    {
        /// <summary>
        /// The current value
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// The remote source where the value should be taken from
        /// </summary>
        IValueWrapper RemoteSource { get; set; }

        /// <summary>
        /// The name of this value
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// checks if an value exists
        /// </summary>
        bool Exists { get; set; }

        /// <summary>
        /// make a deep copy of this value
        /// </summary>
        /// <returns>the copied value</returns>
        IValueWrapper Clone();

        /// <summary>
        /// Move the targets of all references
        /// </summary>
        /// <param name="helper">the flatten helper</param>
        void MoveTargets(Prototypes.PrototypeFlattenerHelper helper);
    }

    /// <summary>
    /// Wrapps a single value and make it easier to manipulate and reference it
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    public class ValueWrapper<T> : IValueWrapper
    {
        static int cid = 0;
        public int id = cid++;

        private T value;

        /// <summary>
        /// The current value
        /// </summary>
        public virtual T Value
        {
            get => ((object)RemoteSource) == null ? value : RemoteSource.Value;
            set
            {
                if (((object)RemoteSource) == null)
                    this.value = value;
                else RemoteSource.Value = value;
            }
        }
        /// <summary>
        /// The current value
        /// </summary>
        object IValueWrapper.Value { get => Value; set => Value = (T)value; }

        /// <summary>
        /// The remote source where the value should be taken from
        /// </summary>
        public ValueWrapper<T> RemoteSource { get; set; }
        /// <summary>
        /// The remote source where the value should be taken from
        /// </summary>
        IValueWrapper IValueWrapper.RemoteSource { get => RemoteSource; set => RemoteSource = (ValueWrapper<T>)value; }

        /// <summary>
        /// The name of this value
        /// </summary>
        public string Name { get; set; }

        private bool exists = true;

        /// <summary>
        /// checks if an value exists
        /// </summary>
        public bool Exists { get => exists; set => exists = value; }
        
        /// <summary>
        /// Compared the object to this object
        /// </summary>
        /// <param name="obj">other object</param>
        /// <returns>other is equal to this one</returns>
        public override bool Equals(object obj)
        {
            if (obj is T) return Value?.Equals((T)obj) ?? obj == null;
            if (obj is ValueWrapper<T>) return ((ValueWrapper<T>)obj) == this;
            return false;
        }

        /// <summary>
        /// Calculates Hash code
        /// </summary>
        /// <returns>hash</returns>
        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Transform the value to a string
        /// </summary>
        /// <returns>string representation of value</returns>
        public override string ToString()
        {
            return Value?.ToString();
        }

        /// <summary>
        /// make a deep copy of this value
        /// </summary>
        /// <returns>the copied value</returns>
        public virtual IValueWrapper Clone()
        {
            var vw = new ValueWrapper<T>();
            vw.exists = exists;
            vw.Name = Name;
            vw.RemoteSource = RemoteSource;
            vw.value = value;
            return vw;
        }

        /// <summary>
        /// Move the targets of all references
        /// </summary>
        /// <param name="helper">the flatten helper</param>
        public virtual void MoveTargets(Prototypes.PrototypeFlattenerHelper helper)
        {
            if (RemoteSource != null)
            {
                if (helper.Conversion.ContainsKey(RemoteSource))
                    RemoteSource = (ValueWrapper<T>)helper.Conversion[RemoteSource];
                RemoteSource.MoveTargets(helper);
            }
        }

        /// <summary>
        /// Convert the value
        /// </summary>
        /// <param name="v">value</param>
        public static implicit operator T(ValueWrapper<T> v)
        {
            return v.Value;
        }

        /// <summary>
        /// Set the value of this <see cref="ValueWrapper{T}"/>
        /// </summary>
        /// <param name="w">target value wrapper</param>
        /// <param name="v">new value</param>
        /// <returns>target value wrapper</returns>
        public static ValueWrapper<T> operator +(ValueWrapper<T> w, T v)
        {
            w.Value = v;
            return w;
        }

        /// <summary>
        /// Checks if both value wrapper are equal
        /// </summary>
        /// <param name="v1">first wrapper</param>
        /// <param name="v2">second wrapper</param>
        /// <returns>both are equal</returns>
        public static bool operator ==(ValueWrapper<T> v1, ValueWrapper<T> v2)
        {
            if (Object.Equals(v1, null) || Object.Equals(v2, null))
                return Object.Equals(v1, v2);
            return v1.Value?.Equals(v2.Value) ?? v2.Value == null;
        }

        /// <summary>
        /// Checks if both value wrapper are not equal
        /// </summary>
        /// <param name="v1">first wrapper</param>
        /// <param name="v2">second wrapper</param>
        /// <returns>both are not equal</returns>
        public static bool operator !=(ValueWrapper<T> v1, ValueWrapper<T> v2)
        {
            if (Object.Equals(v1, null) || Object.Equals(v2, null))
                return !Object.Equals(v1, v2);
            return !v1.Value?.Equals(v2.Value) ?? v2.Value != null;
        }
    }
}
