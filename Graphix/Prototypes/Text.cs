namespace Graphix.Prototypes
{
    /// <summary>
    /// The prototype that can present a string
    /// </summary>
    public class Text : RenderingBase
    {
        /// <summary>
        /// The Value of the text
        /// </summary>
        public ValueWrapper<string> TextValue { get; private set; }

        /// <summary>
        /// The Size of the text
        /// </summary>
        public ValueWrapper<ScreenPos> FontSize { get; private set; }

        /// <summary>
        /// The horizontal alignment of this text
        /// </summary>
        public ValueWrapper<Align> Align { get; private set; }

        /// <summary>
        /// The vertical alignment of this text
        /// </summary>
        public ValueWrapper<Valign> Valign { get; private set; }

        /// <summary>
        /// Creates a prototype that can present a string
        /// </summary>
        public Text()
        {
            RenderName = GetType().FullName;
            Parameter.Add("Text", TextValue = new ValueWrapper<string>());
            Parameter.Add("FontSize", FontSize = new ValueWrapper<ScreenPos>());
            Parameter.Add("Align", Align = new ValueWrapper<Align>());
            Parameter.Add("Valign", Valign = new ValueWrapper<Valign>());
        }
    }
}
