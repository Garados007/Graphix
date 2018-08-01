namespace Graphix.Physic
{
    /// <summary>
    /// Defines the function that the execution of an <see cref="AnimationEffect"/> can be modified
    /// </summary>
    public enum AnimationMode
    {
        /// <summary>
        /// No changement, the animation is called normal. f(t) = t
        /// </summary>
        Linear,
        /// <summary>
        /// The animation is slower at the beginning and very fast at the end.
        /// f(t) = t²
        /// </summary>
        SwingIn,
        /// <summary>
        /// The animation is fast at the beginning and slow at the end.
        /// f(t) = - t² + 2 t
        /// </summary>
        SwingOut,
        /// <summary>
        /// The Animation is slow at the beginning and at the end. In the middle its fast.
        /// f(t) = if x&lt;0.5 then 2 x² else - 2x² + 4x - 1
        /// </summary>
        Swing,
        /// <summary>
        /// The Animation is fast at the beginning and at the end. In the middle its slow.
        /// f(x) = 0.5 ( 2x - 1 )³ + 0.5
        /// </summary>
        Focus,
        /// <summary>
        /// The Animation jumps from 0 to 1 in the middle of the time.
        /// f(x) = if x&lt;0.5 then 0 else 1
        /// </summary>
        Jump
    }
}
