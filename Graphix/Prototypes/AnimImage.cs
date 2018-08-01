namespace Graphix.Prototypes
{
    /// <summary>
    /// A Prototype for a animated Image
    /// </summary>
    public class AnimImage : Image
    {
        /// <summary>
        /// The time in ms that is used for the frames
        /// </summary>
        public ValueWrapper<double> FrameTime { get; private set; }

        /// <summary>
        /// Created a Prototype for a animated Image
        /// </summary>
        public AnimImage()
        {
            RenderName = GetType().FullName;
            Parameter.Add("FrameTime", FrameTime = new ValueWrapper<double>());
        }
    }
}
