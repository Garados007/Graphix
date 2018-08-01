using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Prototypes.Math
{
    public class Calc : IValueWrapper
    {
        public object Value { get => bufcalc(); set { } }
        public IValueWrapper RemoteSource { get => null; set { } }
        public string Name { get; set; }
        public bool Exists { get; set; }

        public CalcMethod Method { get; set; }

        public CalcType Type { get; set; }

        public bool Precompile { get; set; }

        public List<IValueWrapper> ValueList { get; private set; }

        public Calc()
        {
            ValueList = new List<IValueWrapper>();
        }

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
            switch (Type)
            {
                case CalcType.Double:
                    switch(Method)
                    {
                        case CalcMethod.add:
                            {
                                double sum = 0;
                                foreach (var v in ValueList) sum += (double)v.Value;
                                return sum;
                            }
                        case CalcMethod.mult:
                            {
                                double mul = 1;
                                foreach (var v in ValueList) mul *= (double)v.Value;
                                return mul;
                            }
                        case CalcMethod.div:
                            {
                                if (ValueList.Count == 0) return 0.0;
                                double val = (double)ValueList[0].Value;
                                for (int i = 1; i < ValueList.Count; ++i)
                                    val /= (double)ValueList[i].Value;
                                return val;
                            }
                        case CalcMethod.sub:
                            {
                                if (ValueList.Count == 0) return 0.0;
                                double val = (double)ValueList[0].Value;
                                for (int i = 1; i < ValueList.Count; ++i)
                                    val -= (double)ValueList[i].Value;
                                return val;
                            }
                        case CalcMethod.pow:
                            {
                                if (ValueList.Count == 0) return 0.0;
                                double val = (double)ValueList[0].Value;
                                for (int i = 1; i < ValueList.Count; ++i)
                                    val = System.Math.Pow(val, (double)ValueList[i].Value);
                                return val;
                            }
                        case CalcMethod.neg:
                            {
                                if (ValueList.Count > 0)
                                    return -(double)ValueList[0].Value;
                            } break;
                        case CalcMethod.fromBool:
                            {
                                if (ValueList.Count == 0) return 0.0;
                                return (bool)ValueList[0].Value ? 1.0 : 0.0;
                            }
                        case CalcMethod.fromDouble:
                            {
                                if (ValueList.Count == 0) return 0.0;
                                return (double)ValueList[0].Value;
                            }
                        case CalcMethod.fromInt:
                            {
                                if (ValueList.Count == 0) return 0.0;
                                return (double)(int)ValueList[0].Value;
                            }
                        case CalcMethod.fromString:
                            {
                                if (ValueList.Count == 0) return 0.0;
                                double val;
                                return double.TryParse(ValueList[0].Value.ToString(), out val) ? val : 0.0;
                            }
                    } break;
                case CalcType.Int:
                    switch (Method)
                    {
                        case CalcMethod.add:
                            {
                                int sum = 0;
                                foreach (var v in ValueList) sum += (int)v.Value;
                                return sum;
                            }
                        case CalcMethod.mult:
                            {
                                int mul = 1;
                                foreach (var v in ValueList) mul *= (int)v.Value;
                                return mul;
                            }
                        case CalcMethod.div:
                            {
                                if (ValueList.Count == 0) return 0;
                                int val = (int)ValueList[0].Value;
                                for (int i = 1; i < ValueList.Count; ++i)
                                    val /= (int)ValueList[i].Value;
                                return val;
                            }
                        case CalcMethod.sub:
                            {
                                if (ValueList.Count == 0) return 0;
                                int val = (int)ValueList[0].Value;
                                for (int i = 1; i < ValueList.Count; ++i)
                                    val -= (int)ValueList[i].Value;
                                return val;
                            }
                        case CalcMethod.pow:
                            {
                                if (ValueList.Count == 0) return 0;
                                int val = (int)ValueList[0].Value;
                                for (int i = 1; i < ValueList.Count; ++i)
                                    val = (int)System.Math.Pow(val, (int)ValueList[i].Value);
                                return val;
                            }
                        case CalcMethod.neg:
                            {
                                if (ValueList.Count > 0)
                                    return -(int)ValueList[0].Value;
                            }
                            break;
                        case CalcMethod.fromBool:
                            {
                                if (ValueList.Count == 0) return 0;
                                return (bool)ValueList[0].Value ? 1 : 0;
                            }
                        case CalcMethod.fromDouble:
                            {
                                if (ValueList.Count == 0) return 0;
                                return (int)(double)ValueList[0].Value;
                            }
                        case CalcMethod.fromInt:
                            {
                                if (ValueList.Count == 0) return 0;
                                return (int)ValueList[0].Value;
                            }
                        case CalcMethod.fromString:
                            {
                                if (ValueList.Count == 0) return 0;
                                int val;
                                return int.TryParse(ValueList[0].Value.ToString(), out val) ? val : 0;
                            }
                    }
                    break;
                case CalcType.Bool:
                    switch (Method)
                    {
                        case CalcMethod.and:
                            {
                                bool val = (bool)ValueList[0].Value;
                                for (int i = 0; i < ValueList.Count; ++i) val &= (bool)ValueList[i].Value;
                                return val;
                            }
                        case CalcMethod.or:
                            {
                                bool val = (bool)ValueList[0].Value;
                                for (int i = 0; i < ValueList.Count; ++i) val |= (bool)ValueList[i].Value;
                                return val;
                            }
                        case CalcMethod.xor:
                            {
                                bool val = (bool)ValueList[0].Value;
                                for (int i = 0; i < ValueList.Count; ++i) val ^= (bool)ValueList[i].Value;
                                return val;
                            }
                        case CalcMethod.nand:
                            {
                                bool val = (bool)ValueList[0].Value;
                                for (int i = 0; i < ValueList.Count; ++i) val = !(val && (bool)ValueList[i].Value);
                                return val;
                            }
                        case CalcMethod.nor:
                            {
                                bool val = (bool)ValueList[0].Value;
                                for (int i = 0; i < ValueList.Count; ++i) val = !(val || (bool)ValueList[i].Value);
                                return val;
                            }
                        case CalcMethod.xnor:
                            {
                                bool val = (bool)ValueList[0].Value;
                                for (int i = 0; i < ValueList.Count; ++i) val = !(val ^ (bool)ValueList[i].Value);
                                return val;
                            }
                        case CalcMethod.not:
                            {
                                return !(bool)ValueList[0].Value;
                            }
                        case CalcMethod.fromBool:
                            {
                                return (bool)ValueList[0].Value;
                            }
                        case CalcMethod.fromDouble:
                            {
                                if (ValueList.Count == 0) return false;
                                return (double)ValueList[0].Value != 0;
                            }
                        case CalcMethod.fromInt:
                            {
                                if (ValueList.Count == 0) return false;
                                return (int)ValueList[0].Value != 0;
                            }
                        case CalcMethod.fromString:
                            {
                                if (ValueList.Count == 0) return false;
                                bool val;
                                return bool.TryParse(ValueList[0].Value.ToString(), out val) && val;
                            }
                    } break;
                case CalcType.String:
                    switch (Method)
                    {
                        case CalcMethod.concat:
                            {
                                var sb = new StringBuilder();
                                foreach (var v in ValueList) sb.Append(v.Value?.ToString());
                                return sb.ToString();
                            }
                        case CalcMethod.fromBool:
                        case CalcMethod.fromDouble:
                        case CalcMethod.fromInt:
                        case CalcMethod.fromString:
                            {
                                if (ValueList.Count == 0) return "";
                                return ValueList[0].Value.ToString();
                            }
                    } break;
            }
            return null;
        }

        public bool ValidMethod()
        {
            switch (Type)
            {
                case CalcType.Double:
                case CalcType.Int:
                    switch (Method)
                    {
                        case CalcMethod.add:
                        case CalcMethod.sub:
                        case CalcMethod.mult:
                        case CalcMethod.div:
                        case CalcMethod.pow:
                        case CalcMethod.neg:
                        case CalcMethod.fromBool:
                        case CalcMethod.fromDouble:
                        case CalcMethod.fromInt:
                        case CalcMethod.fromString:
                            return true;
                    }
                    break;
                case CalcType.Bool:
                    switch (Method)
                    {
                        case CalcMethod.and:
                        case CalcMethod.or:
                        case CalcMethod.xor:
                        case CalcMethod.nand:
                        case CalcMethod.nor:
                        case CalcMethod.xnor:
                        case CalcMethod.not:
                        case CalcMethod.fromBool:
                        case CalcMethod.fromDouble:
                        case CalcMethod.fromInt:
                        case CalcMethod.fromString:
                            return true;
                    }
                    break;
                case CalcType.String:
                    switch (Method)
                    {
                        case CalcMethod.concat:
                        case CalcMethod.fromBool:
                        case CalcMethod.fromDouble:
                        case CalcMethod.fromInt:
                        case CalcMethod.fromString:
                            return true;
                    }
                    break;
            }
            return false;
        }

        public IValueWrapper Clone()
        {
            var calc = new Calc();
            calc.Exists = Exists;
            calc.Method = Method;
            calc.Name = Name;
            calc.Precompile = Precompile;
            calc.Type = Type;
            calc.ValueList = ValueList.ConvertAll((v) => v.Clone());
            return calc;
        }

        public void MoveTargets(PrototypeFlattenerHelper helper)
        {
            for (int i = 0; i<ValueList.Count; ++i)
            {
                if (helper.Conversion.ContainsKey(ValueList[i]))
                    ValueList[i] = helper.Conversion[ValueList[i]];
                ValueList[i].MoveTargets(helper);
            }
        }
    }

    public enum CalcMethod
    {
        add,
        sub,
        mult,
        div,
        neg,
        and,
        or,
        xor,
        nor,
        xnor,
        not,
        nand,
        concat,
        pow,
        fromDouble,
        fromInt,
        fromBool,
        fromString
    }

    public enum CalcType
    {
        Double,
        Int,
        Bool,
        String
    }
}
