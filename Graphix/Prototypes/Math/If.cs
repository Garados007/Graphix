namespace Graphix.Prototypes.Math
{
    /// <summary>
    /// This Value returns one of two values based on a condition
    /// </summary>
    public class If : IValueWrapper
    {
        /// <summary>
        /// The value of this branch
        /// </summary>
        public object Value { get => bufcalc(); set { } }
        /// <summary>
        /// unused
        /// </summary>
        public IValueWrapper RemoteSource { get => null; set { } }
        /// <summary>
        /// The name of this value (should be "If")
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
        /// The boolean value of an condition
        /// </summary>
        public IValueWrapper Condition { get; set; }
        /// <summary>
        /// This value is returned if <see cref="Condition"/> returns true
        /// </summary>
        public IValueWrapper True { get; set; }
        /// <summary>
        /// This value is returned if <see cref="Condition"/> returns false
        /// </summary>
        public IValueWrapper False { get; set; }


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
        /// Calculates the value
        /// </summary>
        /// <returns>the result of this branch</returns>
        private object calc()
        {
            var cond = Condition.Value;
            bool success;
            if (cond is bool) success = (bool)cond;
            else if (cond is double) success = (double)cond != 0;
            else if (cond is int) success = (int)cond != 0;
            else if (cond is string) success = !string.IsNullOrEmpty((string)cond);
            else success = cond != null;
            if (success) return True.Value;
            else return False.Value;
        }

        /// <summary>
        /// Clone this value completely
        /// </summary>
        /// <returns>the cloned result</returns>
        public IValueWrapper Clone()
        {
            var val = new If();
            val.Condition = Condition.Clone();
            val.Exists = Exists;
            val.False = False.Clone();
            val.Name = Name;
            val.Precompile = Precompile;
            val.True = True.Clone();
            return val;
        }

        /// <summary>
        /// Move the targets of its values using the helper
        /// </summary>
        /// <param name="helper">The flattened helper</param>
        public void MoveTargets(PrototypeFlattenerHelper helper)
        {
            if (Condition != null)
            {
                if (helper.Conversion.ContainsKey(Condition))
                    Condition = helper.Conversion[Condition];
                Condition.MoveTargets(helper);
            }
            if (True != null)
            {
                if (helper.Conversion.ContainsKey(True))
                    True = helper.Conversion[True];
                True.MoveTargets(helper);
            }
            if (False != null)
            {
                if (helper.Conversion.ContainsKey(False))
                    False = helper.Conversion[False];
                False.MoveTargets(helper);
            }
        }
    }
}
