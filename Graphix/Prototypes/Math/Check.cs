using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Prototypes.Math
{
    public class Check : IValueWrapper
    { 
        public object Value { get => bufcalc(); set { } }
        public IValueWrapper RemoteSource { get => null; set { } }
        public string Name { get; set; }
        public bool Exists { get; set; }

        public bool Precompile { get; set; }
        
        public IValueWrapper Value1 { get; set; }

        public IValueWrapper Value2 { get; set; }

        public CheckMode Mode { get; set; }

        object buffer; bool hasbuffer = false;
        private object bufcalc()
        {
            if (Precompile)
            {
                if (hasbuffer) return buffer;
                hasbuffer = true;
                return buffer = calc();
            }
            else return calc();
        }

        private object calc()
        {
            var v1 = Value1.Value;
            var v2 = Value2.Value;
            switch (Mode)
            {
                case CheckMode.eq:
                    {
                        if ((v1 is double) && (v2 is double)) return ((double)v1) == ((double)v2);
                        if ((v1 is int) && (v2 is int)) return ((int)v1) == ((int)v2);
                        if ((v1 is string) && (v2 is string)) return ((string)v1) == ((string)v2);
                        if ((v1 is bool) && (v2 is bool)) return ((bool)v1) == ((bool)v2);
                        return v1 == v2;
                    }
                case CheckMode.neq:
                    {
                        if ((v1 is double) && (v2 is double)) return ((double)v1) != ((double)v2);
                        if ((v1 is int) && (v2 is int)) return ((int)v1) != ((int)v2);
                        if ((v1 is string) && (v2 is string)) return ((string)v1) != ((string)v2);
                        if ((v1 is bool) && (v2 is bool)) return ((bool)v1) != ((bool)v2);
                        return v1 != v2;
                    }
                case CheckMode.lt:
                    {
                        if ((v1 is double) && (v2 is double)) return ((double)v1) < ((double)v2);
                        if ((v1 is int) && (v2 is int)) return ((int)v1) < ((int)v2);
                        if ((v1 is string) && (v2 is string)) return ((string)v1).Length < ((string)v2).Length;
                        if ((v1 is bool) && (v2 is bool)) return ((bool)v1 ? 1 : 0) < ((bool)v2 ? 1 : 0);
                        return null;
                    }
                case CheckMode.lteq:
                    {
                        if ((v1 is double) && (v2 is double)) return ((double)v1) <= ((double)v2);
                        if ((v1 is int) && (v2 is int)) return ((int)v1) <= ((int)v2);
                        if ((v1 is string) && (v2 is string)) return ((string)v1).Length <= ((string)v2).Length;
                        if ((v1 is bool) && (v2 is bool)) return ((bool)v1 ? 1 : 0) <= ((bool)v2 ? 1 : 0);
                        return null;
                    }
                case CheckMode.gt:
                    {
                        if ((v1 is double) && (v2 is double)) return ((double)v1) > ((double)v2);
                        if ((v1 is int) && (v2 is int)) return ((int)v1) > ((int)v2);
                        if ((v1 is string) && (v2 is string)) return ((string)v1).Length > ((string)v2).Length;
                        if ((v1 is bool) && (v2 is bool)) return ((bool)v1 ? 1 : 0) > ((bool)v2 ? 1 : 0);
                        return null;
                    }
                case CheckMode.gteq:
                    {
                        if ((v1 is double) && (v2 is double)) return ((double)v1) >= ((double)v2);
                        if ((v1 is int) && (v2 is int)) return ((int)v1) >= ((int)v2);
                        if ((v1 is string) && (v2 is string)) return ((string)v1).Length >= ((string)v2).Length;
                        if ((v1 is bool) && (v2 is bool)) return ((bool)v1 ? 1 : 0) >= ((bool)v2 ? 1 : 0);
                        return null;
                    }
                default: return null;
            }
        }

        public IValueWrapper Clone()
        {
            var check = new Check();
            check.Exists = Exists;
            check.Mode = Mode;
            check.Name = Name;
            check.Precompile = Precompile;
            check.Value1 = Value1.Clone();
            check.Value2 = Value2.Clone();
            return check;
        }

        public void MoveTargets(PrototypeFlattenerHelper helper)
        {
            if (Value1 != null)
            {
                if (helper.Conversion.ContainsKey(Value1))
                    Value1 = helper.Conversion[Value1];
                Value1.MoveTargets(helper);
            }
            if (Value2 != null)
            {
                if (helper.Conversion.ContainsKey(Value2))
                    Value1 = helper.Conversion[Value2];
                Value2.MoveTargets(helper);
            }
        }
    }

    public enum CheckMode
    {
        eq,
        neq,
        lt,
        lteq,
        gt,
        gteq
    }
}
