using System.Collections.Generic;
using System.Text;

namespace Graphix.Prototypes.Math
{
    /// <summary>
    /// Calculates a value using an operator
    /// </summary>
    public class Calc : IValueWrapper
    {
        /// <summary>
        /// The result value of the calculation
        /// </summary>
        public object Value { get => bufcalc(); set { } }
        /// <summary>
        /// unused
        /// </summary>
        public IValueWrapper RemoteSource { get => null; set { } }
        /// <summary>
        /// The name of this Value (should be "Calc")
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Define if the real value of this value object exists
        /// </summary>
        public bool Exists { get; set; }
        /// <summary>
        /// The calculation method that is used
        /// </summary>
        public CalcMethod Method { get; set; }
        /// <summary>
        /// The result type of this calculation
        /// </summary>
        public CalcType Type { get; set; }
        /// <summary>
        /// If Precompile is on then this value is calculated only once.
        /// If Precompile is off then this value is calculated each time.
        /// </summary>
        public bool Precompile { get; set; }

        /// <summary>
        /// The List of values that are used in this calculation
        /// </summary>
        public List<IValueWrapper> ValueList { get; private set; }

        /// <summary>
        /// Creates a new value that can calculate its result using an operator
        /// </summary>
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

        /// <summary>
        /// calculate the value
        /// </summary>
        /// <returns>the result</returns>
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
                                bool val = true;
                                for (int i = 0; i < ValueList.Count; ++i) val &= (bool)ValueList[i].Value;
                                return val;
                            }
                        case CalcMethod.or:
                            {
                                bool val = false;
                                for (int i = 0; i < ValueList.Count; ++i) val |= (bool)ValueList[i].Value;
                                return val;
                            }
                        case CalcMethod.xor:
                            {
                                bool val = (bool)ValueList[0].Value;
                                for (int i = 1; i < ValueList.Count; ++i) val ^= (bool)ValueList[i].Value;
                                return val;
                            }
                        case CalcMethod.nand:
                            {
                                bool val = (bool)ValueList[0].Value;
                                for (int i = 1; i < ValueList.Count; ++i) val = !(val && (bool)ValueList[i].Value);
                                return val;
                            }
                        case CalcMethod.nor:
                            {
                                bool val = (bool)ValueList[0].Value;
                                for (int i = 1; i < ValueList.Count; ++i) val = !(val || (bool)ValueList[i].Value);
                                return val;
                            }
                        case CalcMethod.xnor:
                            {
                                bool val = (bool)ValueList[0].Value;
                                for (int i = 1; i < ValueList.Count; ++i) val = !(val ^ (bool)ValueList[i].Value);
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

        /// <summary>
        /// Check if the combination of <see cref="Method"/> and <see cref="Type"/> are valid.
        /// </summary>
        /// <returns>true if the combination is valid</returns>
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

        /// <summary>
        /// Clone this value completely
        /// </summary>
        /// <returns>the cloned result</returns>
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

        /// <summary>
        /// Move the targets of its values using the helper
        /// </summary>
        /// <param name="helper">The flattened helper</param>
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

    /// <summary>
    /// The method for the calculation in <see cref="Calc"/>
    /// </summary>
    public enum CalcMethod
    {
        /// <summary>
        /// Add (+)
        /// </summary>
        add,
        /// <summary>
        /// Subtract (-)
        /// </summary>
        sub,
        /// <summary>
        /// Multiply (*)
        /// </summary>
        mult,
        /// <summary>
        /// Divide (/)
        /// </summary>
        div,
        /// <summary>
        /// Negate (-)
        /// </summary>
        neg,
        /// <summary>
        /// Boolean and
        /// </summary>
        and,
        /// <summary>
        /// Boolean or
        /// </summary>
        or,
        /// <summary>
        /// Boolean xor
        /// </summary>
        xor,
        /// <summary>
        /// Boolean nor
        /// </summary>
        nor,
        /// <summary>
        /// Boolean xnor
        /// </summary>
        xnor,
        /// <summary>
        /// Boolean not
        /// </summary>
        not,
        /// <summary>
        /// Boolean nand
        /// </summary>
        nand,
        /// <summary>
        /// Concat two strings
        /// </summary>
        concat,
        /// <summary>
        /// Power of two numbers
        /// </summary>
        pow,
        /// <summary>
        /// transform double to the result type
        /// </summary>
        fromDouble,
        /// <summary>
        /// transform int to the result type
        /// </summary>
        fromInt,
        /// <summary>
        /// transform bool to the result type
        /// </summary>
        fromBool,
        /// <summary>
        /// transform string to the result type
        /// </summary>
        fromString
    }


    /// <summary>
    /// The result type for the calculation in <see cref="Calc"/>
    /// </summary>
    public enum CalcType
    {
        /// <summary>
        /// Double value (e.g. 3.14)
        /// </summary>
        Double,
        /// <summary>
        /// Integer value (e.g. 42)
        /// </summary>
        Int,
        /// <summary>
        /// Boolean value (e.g. true)
        /// </summary>
        Bool,
        /// <summary>
        /// String value (e.g. "abc")
        /// </summary>
        String
    }
}
