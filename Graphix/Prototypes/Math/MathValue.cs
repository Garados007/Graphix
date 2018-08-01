namespace Graphix.Prototypes.Math
{
    /// <summary>
    /// A Value that can produce its value from a calculation
    /// </summary>
    /// <typeparam name="T">The type of result value</typeparam>
    public class MathValue<T> : ValueWrapper<T>
    {
        /// <summary>
        /// Get the result of this calculation
        /// </summary>
        public override T Value { get => (T)ValueSource.Value; set { } }
        
        /// <summary>
        /// Define the source of the value
        /// </summary>
        public IValueWrapper ValueSource { get; set; }

        /// <summary>
        /// Define the string representation of the value type
        /// </summary>
        public string ValueType { get; set; }

        /// <summary>
        /// Clones the calculation completely
        /// </summary>
        /// <returns>The identical clone of this calculation</returns>
        public override IValueWrapper Clone()
        {
            var math = new MathValue<T>();
            math.Exists = Exists;
            math.Name = Name;
            math.ValueSource = ValueSource.Clone();
            math.ValueType = ValueType;
            return math;
        }

        /// <summary>
        /// Move the targets of source value
        /// </summary>
        /// <param name="helper">the helper that contains information about the movements</param>
        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            if (ValueSource != null)
            {
                if (helper.Conversion.ContainsKey(ValueSource))
                    ValueSource = helper.Conversion[ValueSource];
                ValueSource.MoveTargets(helper);
            }
        }
    }
}
