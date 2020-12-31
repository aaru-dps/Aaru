// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Images.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines exceptions to be thrown by disc image plugins.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.Serialization;

namespace Aaru.CommonTypes.Exceptions
{
    /// <summary>Feature is supported by image but not implemented yet.</summary>
    [Serializable]
    public class FeatureSupportedButNotImplementedImageException : Exception
    {
        /// <summary>Feature is supported by image but not implemented yet.</summary>
        /// <param name="message">Message.</param>
        /// <param name="inner">Inner.</param>
        public FeatureSupportedButNotImplementedImageException(string message, Exception inner) :
            base(message, inner) {}

        /// <summary>Feature is supported by image but not implemented yet.</summary>
        /// <param name="message">Message.</param>
        public FeatureSupportedButNotImplementedImageException(string message) : base(message) {}

        /// <summary>Feature is supported by image but not implemented yet.</summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected FeatureSupportedButNotImplementedImageException(SerializationInfo info, StreamingContext context)
        {
            if(info == null)
                throw new ArgumentNullException(nameof(info));
        }
    }

    /// <summary>Feature is not supported by image.</summary>
    [Serializable]
    public class FeatureUnsupportedImageException : Exception
    {
        /// <summary>Feature is not supported by image.</summary>
        /// <param name="message">Message.</param>
        /// <param name="inner">Inner.</param>
        public FeatureUnsupportedImageException(string message, Exception inner) : base(message, inner) {}

        /// <summary>Feature is not supported by image.</summary>
        /// <param name="message">Message.</param>
        public FeatureUnsupportedImageException(string message) : base(message) {}

        /// <summary>Feature is not supported by image.</summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected FeatureUnsupportedImageException(SerializationInfo info, StreamingContext context)
        {
            if(info == null)
                throw new ArgumentNullException(nameof(info));
        }
    }

    /// <summary>Feature is supported by image but not present on it.</summary>
    [Serializable]
    public class FeatureNotPresentImageException : Exception
    {
        /// <summary>Feature is supported by image but not present on it.</summary>
        /// <param name="message">Message.</param>
        /// <param name="inner">Inner.</param>
        public FeatureNotPresentImageException(string message, Exception inner) : base(message, inner) {}

        /// <summary>Feature is supported by image but not present on it.</summary>
        /// <param name="message">Message.</param>
        public FeatureNotPresentImageException(string message) : base(message) {}

        /// <summary>Feature is supported by image but not present on it.</summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected FeatureNotPresentImageException(SerializationInfo info, StreamingContext context)
        {
            if(info == null)
                throw new ArgumentNullException(nameof(info));
        }
    }

    /// <summary>Feature is supported by image but not by the disc it represents.</summary>
    [Serializable]
    public class FeaturedNotSupportedByDiscImageException : Exception
    {
        /// <summary>Feature is supported by image but not by the disc it represents.</summary>
        /// <param name="message">Message.</param>
        /// <param name="inner">Inner.</param>
        public FeaturedNotSupportedByDiscImageException(string message, Exception inner) : base(message, inner) {}

        /// <summary>Feature is supported by image but not by the disc it represents.</summary>
        /// <param name="message">Message.</param>
        public FeaturedNotSupportedByDiscImageException(string message) : base(message) {}

        /// <summary>Feature is supported by image but not by the disc it represents.</summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected FeaturedNotSupportedByDiscImageException(SerializationInfo info, StreamingContext context)
        {
            if(info == null)
                throw new ArgumentNullException(nameof(info));
        }
    }

    /// <summary>Corrupt, incorrect or unhandled feature found on image</summary>
    [Serializable]
    public class ImageNotSupportedException : Exception
    {
        /// <summary>Corrupt, incorrect or unhandled feature found on image</summary>
        /// <param name="message">Message.</param>
        /// <param name="inner">Inner.</param>
        public ImageNotSupportedException(string message, Exception inner) : base(message, inner) {}

        /// <summary>Corrupt, incorrect or unhandled feature found on image</summary>
        /// <param name="message">Message.</param>
        public ImageNotSupportedException(string message) : base(message) {}

        /// <summary>Corrupt, incorrect or unhandled feature found on image</summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected ImageNotSupportedException(SerializationInfo info, StreamingContext context)
        {
            if(info == null)
                throw new ArgumentNullException(nameof(info));
        }
    }
}