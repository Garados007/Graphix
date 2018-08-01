namespace Graphix.Prototypes
{
    /// <summary>
    /// The basic prototype that can positioned on the screen
    /// </summary>
    public class DisplayBase : PrototypeBase
    {
        /// <summary>
        /// The x position of this object
        /// </summary>
        public ValueWrapper<ScreenPos> X { get; private set; }

        /// <summary>
        /// The y position of this object
        /// </summary>
        public ValueWrapper<ScreenPos> Y { get; private set; }

        /// <summary>
        /// The width of this object
        /// </summary>
        public ValueWrapper<ScreenPos> Width { get; private set; }

        /// <summary>
        /// The height of this object
        /// </summary>
        public ValueWrapper<ScreenPos> Height { get; private set; }

        /// <summary>
        /// Defines if a object is visible or not
        /// </summary>
        public ValueWrapper<bool> Visible { get; private set; }

        /// <summary>
        /// Defines if this object is solid for mouse events
        /// </summary>
        public ValueWrapper<bool> MouseSolid { get; private set; }

        /// <summary>
        /// Creates the basic prototype that can positioned on the screen
        /// </summary>
        public DisplayBase()
        {
            RenderName = GetType().FullName;
            Parameter.Add("X", X = new ValueWrapper<ScreenPos>());
            Parameter.Add("Y", Y = new ValueWrapper<ScreenPos>());
            Parameter.Add("Width", Width = new ValueWrapper<ScreenPos>());
            Parameter.Add("Height", Height = new ValueWrapper<ScreenPos>());
            Parameter.Add("Visible", Visible = new ValueWrapper<bool>());
            Parameter.Add("MouseSolid", MouseSolid = new ValueWrapper<bool>());
            Visible.Value = true;
        }
    }


}
