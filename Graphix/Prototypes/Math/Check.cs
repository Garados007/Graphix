namespace Graphix.Prototypes.Math
{
    /// <summary>
    /// A boolean value thats result is the result of a comparison
    /// </summary>
    public class Check : IValueWrapper
    { 
        /// <summary>
        /// The Value of this comparison
        /// </summary>
        public object Value { get => bufcalc(); set { } }
        /// <summary>
        /// unused
        /// </summary>
        public IValueWrapper RemoteSource { get => null; set { } }
        /// <summary>
        /// The name of this value (should be "Check")
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Determines if the result of this value exists
        /// </summary>
        public bool Exists { get; set; }
        /// <summary>
        /// If Precompile is on then this value is calculated only once.
        /// If Precompile is off then this value is calculated each time.
        /// </summary>
        public bool Precompile { get; set; }
        /// <summary>
        /// The first value of this comparison
        /// </summary>
        public IValueWrapper Value1 { get; set; }
        /// <summary>
        /// The second value of this comparison
        /// </summary>
        public IValueWrapper Value2 { get; set; }
        /// <summary>
        /// The comparison mode
        /// </summary>
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

        /// <summary>
        /// Calculate the value
        /// </summary>
        /// <returns>the result of this comparison</returns>
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

        /// <summary>
        /// Clone this value completely
        /// </summary>
        /// <returns>the cloned result</returns>
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

        /// <summary>
        /// Move the targets of its values using the helper
        /// </summary>
        /// <param name="helper">The flattened helper</param>
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

    /// <summary>
    /// The comparison mode of <see cref="Check"/>
    /// </summary>
    public enum CheckMode
    {
        /// <summary>
        /// ==
        /// </summary>
        eq,
        /// <summary>
        /// !=
        /// </summary>
        neq,
        /// <summary>
        /// &lt;
        /// </summary>
        lt,
        /// <summary>
        /// &lt;=
        /// </summary>
        lteq,
        /// <summary>
        /// &gt;
        /// </summary>
        gt,
        /// <summary>
        /// &gt;=
        /// </summary>
        gteq
    }
}
