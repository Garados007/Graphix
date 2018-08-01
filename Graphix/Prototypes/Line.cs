namespace Graphix.Prototypes
{
    /// <summary>
    /// Prototype for a single line
    /// </summary>
    public class Line : RenderingBase
    {
        /// <summary>
        /// The second x coordinate of this line
        /// </summary>
        public ValueWrapper<ScreenPos> X2 { get; private set; }
        /// <summary>
        /// The second y coordinate of this line
        /// </summary>
        public ValueWrapper<ScreenPos> Y2 { get; private set; }

        /// <summary>
        /// Creates a Prototype for a single line
        /// </summary>
        public Line()
        {
            RenderName = GetType().FullName;
            Parameter.Add("X2", X2 = new ValueWrapper<ScreenPos>());
            Parameter.Add("Y2", Y2 = new ValueWrapper<ScreenPos>());
        }
    }
}
