using System;

namespace Graphix.Physic
{
    /// <summary>
    /// The sound player that can run some audio files
    /// </summary>
    public interface ISoundPlayer : IDisposable
    {
        /// <summary>
        /// Play a specific sound file
        /// </summary>
        /// <param name="file">path to file</param>
        /// <param name="volume">the volume this sound should played</param>
        void PlaySound(string file, double volume);

        /// <summary>
        /// Stop all sounds that was played with this file
        /// </summary>
        /// <param name="file">path to file</param>
        void StopSound(string file);
    }
}
