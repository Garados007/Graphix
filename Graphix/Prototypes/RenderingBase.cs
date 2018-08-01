using System.Drawing;

namespace Graphix.Prototypes
{
    /// <summary>
    /// The base prototype that has a colored representation
    /// </summary>
    public class RenderingBase : DisplayBase
    {
        /// <summary>
        /// The fore color of this object
        /// </summary>
        public ValueWrapper<Color> ForeColor { get; private set; }
        /// <summary>
        /// The background color of this object
        /// </summary>
        public ValueWrapper<Color> BackColor { get; private set; }

        /// <summary>
        /// Creates the base prototype that has a colored representation
        /// </summary>
        public RenderingBase()
        {
            RenderName = GetType().FullName;
            Parameter.Add("ForeColor", ForeColor = new ValueWrapper<Color>());
            Parameter.Add("BackColor", BackColor = new ValueWrapper<Color>());
        }
    }
}
