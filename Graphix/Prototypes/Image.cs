﻿namespace Graphix.Prototypes
{
    /// <summary>
    /// Prototype for an image
    /// </summary>
    public class Image : DisplayBase
    {
        /// <summary>
        /// the url for the image file
        /// </summary>
        public ValueWrapper<string> Url { get; private set; }

        /// <summary>
        /// Creates a Prototype for an image
        /// </summary>
        public Image()
        {
            RenderName = GetType().FullName;
            Parameter.Add("Url", Url = new ValueWrapper<string>());
        }
    }
}
