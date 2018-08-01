using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix
{
    public interface IValueWrapper
    {
        object Value { get; set; }

        IValueWrapper RemoteSource { get; set; }

        string Name { get; set; }

        bool Exists { get; set; }

        IValueWrapper Clone();

        void MoveTargets(Prototypes.PrototypeFlattenerHelper helper);
    }

    public class ValueWrapper<T> : IValueWrapper
    {
        static int cid = 0;
        public int id = cid++;

        private T value;

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
        object IValueWrapper.Value { get => Value; set => Value = (T)value; }

        public ValueWrapper<T> RemoteSource { get; set; }
        IValueWrapper IValueWrapper.RemoteSource { get => RemoteSource; set => RemoteSource = (ValueWrapper<T>)value; }
        
        public string Name { get; set; }

        private bool exists = true;
        
        public bool Exists { get => exists; set => exists = value; }
        
        public override bool Equals(object obj)
        {
            if (obj is T) return Value?.Equals((T)obj) ?? obj == null;
            if (obj is ValueWrapper<T>) return ((ValueWrapper<T>)obj) == this;
            return false;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Value?.ToString();
        }

        public virtual IValueWrapper Clone()
        {
            var vw = new ValueWrapper<T>();
            vw.exists = exists;
            vw.Name = Name;
            vw.RemoteSource = RemoteSource;
            vw.value = value;
            return vw;
        }

        public virtual void MoveTargets(Prototypes.PrototypeFlattenerHelper helper)
        {
            if (RemoteSource != null)
            {
                if (helper.Conversion.ContainsKey(RemoteSource))
                    RemoteSource = (ValueWrapper<T>)helper.Conversion[RemoteSource];
                RemoteSource.MoveTargets(helper);
            }
        }

        public static implicit operator T(ValueWrapper<T> v)
        {
            return v.Value;
        }

        public static ValueWrapper<T> operator +(ValueWrapper<T> w, T v)
        {
            w.Value = v;
            return w;
        }

        public static bool operator ==(ValueWrapper<T> v1, ValueWrapper<T> v2)
        {
            if (Object.Equals(v1, null) || Object.Equals(v2, null))
                return Object.Equals(v1, v2);
            return v1.Value?.Equals(v2.Value) ?? v2.Value == null;
        }

        public static bool operator !=(ValueWrapper<T> v1, ValueWrapper<T> v2)
        {
            if (Object.Equals(v1, null) || Object.Equals(v2, null))
                return !Object.Equals(v1, v2);
            return !v1.Value?.Equals(v2.Value) ?? v2.Value != null;
        }
    }
}
