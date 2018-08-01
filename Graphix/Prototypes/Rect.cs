namespace Graphix.Prototypes
{
    /// <summary>
    /// Prototype for a single rectangle
    /// </summary>
    public class Rect : RenderingBase
    {
        /// <summary>
        /// the line width of the border
        /// </summary>
        public ValueWrapper<double> LineWidth { get; private set; }

        /// <summary>
        /// Creates a Prototype for a single rectangle
        /// </summary>
        public Rect()
        {
            RenderName = GetType().FullName;
            Parameter.Add("LineWidth", LineWidth = new ValueWrapper<double>()
            {
                Value = 1,
                Exists = true
            });
        }
    }
}
