namespace Graphix
{
    /// <summary>
    /// The Position on the screen
    /// </summary>
    public struct ScreenPos
    {
        /// <summary>
        /// the raw value of the position
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// The dimension of the value
        /// </summary>
        public PosType PosType { get; set; }

        /// <summary>
        /// The Position on the screen
        /// </summary>
        public ScreenPos(double value, PosType posType)
        {
            Value = value;
            PosType = posType;
        }

        /// <summary>
        /// Creates representation of value
        /// </summary>
        /// <returns>representation</returns>
        public override string ToString()
        {
            switch (PosType)
            {
                case PosType.Relative: return Value.ToString() + "%";
                case PosType.RelativeWidth: return Value.ToString() + "%w";
                case PosType.RelativeHeight: return Value.ToString() + "%h";
                case PosType.Screen: return Value.ToString() + "v";
                case PosType.ScreenWidth: return Value.ToString() + "vw";
                case PosType.ScreenHeight: return Value.ToString() + "vh";
                default: return Value.ToString();
            }
        }
    }

    /// <summary>
    /// Define de Dimension of <see cref="ScreenPos"/>
    /// </summary>
    public enum PosType
    {
        /// <summary>
        /// This value is an absolute value in pixel
        /// </summary>
        Absolute,
        /// <summary>
        /// The value is a portion of the length in same axis of the parent 
        /// element (%)
        /// </summary>
        Relative,
        /// <summary>
        /// The value is a portion of the width of the parent element (%w)
        /// </summary>
        RelativeWidth,
        /// <summary>
        /// The value is a portion of the height of the parent element (%h)
        /// </summary>
        RelativeHeight,
        /// <summary>
        /// The value is a portion of the length in same axis of the screen (v)
        /// </summary>
        Screen,
        /// <summary>
        /// The value is a portion of the width of the screen (vw)
        /// </summary>
        ScreenWidth,
        /// <summary>
        /// The value is a portion of the height of the screen (vh)
        /// </summary>
        ScreenHeight
    }
}
