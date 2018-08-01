namespace Graphix.Physic
{
    /// <summary>
    /// The data that is given from <see cref="AnimationRuntime"/> to the
    /// <see cref="AnimationEffect"/> to perform their animations.
    /// </summary>
    public class AnimationRuntimeData
    {
        /// <summary>
        /// The current time in the global animation cycle
        /// </summary>
        public double CurrentTime { get; set; }

        /// <summary>
        /// The time the last <see cref="AnimationEffect"/> was executed.
        /// </summary>
        public double LastTileTime { get; set; }

        /// <summary>
        /// The time this <see cref="AnimationEffect"/> was started
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// The <see cref="AnimationRuntime"/> that is responsible for this animation.
        /// </summary>
        public AnimationRuntime Runtime { get; set; }

        /// <summary>
        /// This flag informs the <see cref="AnimationRuntime"/> that this animation
        /// can start.
        /// </summary>
        public bool CanAnimate { get; set; }
    }
}
