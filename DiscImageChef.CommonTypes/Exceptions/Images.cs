// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
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
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.Serialization;

namespace DiscImageChef.CommonTypes.Exceptions
{
    /// <summary>
    ///     Feature is supported by image but not implemented yet.
    /// </summary>
    [Serializable]
    public class FeatureSupportedButNotImplementedImageException : Exception
    {
        /// <summary>
        ///     Feature is supported by image but not implemented yet.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="inner">Inner.</param>
        public FeatureSupportedButNotImplementedImageException(string message, Exception inner) :
            base(message, inner) { }

        /// <summary>
        ///     Feature is supported by image but not implemented yet.
        /// </summary>
        /// <param name="message">Message.</param>
        public FeatureSupportedButNotImplementedImageException(string message) : base(message) { }

        /// <summary>
        ///     Feature is supported by image but not implemented yet.
        /// </summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected FeatureSupportedButNotImplementedImageException(SerializationInfo info, StreamingContext context)
        {
            if(info == null) throw new ArgumentNullException(nameof(info));
        }
    }

    /// <summary>
    ///     Feature is not supported by image.
    /// </summary>
    [Serializable]
    public class FeatureUnsupportedImageException : Exception
    {
        /// <summary>
        ///     Feature is not supported by image.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="inner">Inner.</param>
        public FeatureUnsupportedImageException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Feature is not supported by image.
        /// </summary>
        /// <param name="message">Message.</param>
        public FeatureUnsupportedImageException(string message) : base(message) { }

        /// <summary>
        ///     Feature is not supported by image.
        /// </summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected FeatureUnsupportedImageException(SerializationInfo info, StreamingContext context)
        {
            if(info == null) throw new ArgumentNullException(nameof(info));
        }
    }

    /// <summary>
    ///     Feature is supported by image but not present on it.
    /// </summary>
    [Serializable]
    public class FeatureNotPresentImageException : Exception
    {
        /// <summary>
        ///     Feature is supported by image but not present on it.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="inner">Inner.</param>
        public FeatureNotPresentImageException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Feature is supported by image but not present on it.
        /// </summary>
        /// <param name="message">Message.</param>
        public FeatureNotPresentImageException(string message) : base(message) { }

        /// <summary>
        ///     Feature is supported by image but not present on it.
        /// </summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected FeatureNotPresentImageException(SerializationInfo info, StreamingContext context)
        {
            if(info == null) throw new ArgumentNullException(nameof(info));
        }
    }

    /// <summary>
    ///     Feature is supported by image but not by the disc it represents.
    /// </summary>
    [Serializable]
    public class FeaturedNotSupportedByDiscImageException : Exception
    {
        /// <summary>
        ///     Feature is supported by image but not by the disc it represents.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="inner">Inner.</param>
        public FeaturedNotSupportedByDiscImageException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Feature is supported by image but not by the disc it represents.
        /// </summary>
        /// <param name="message">Message.</param>
        public FeaturedNotSupportedByDiscImageException(string message) : base(message) { }

        /// <summary>
        ///     Feature is supported by image but not by the disc it represents.
        /// </summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected FeaturedNotSupportedByDiscImageException(SerializationInfo info, StreamingContext context)
        {
            if(info == null) throw new ArgumentNullException(nameof(info));
        }
    }

    /// <summary>
    ///     Corrupt, incorrect or unhandled feature found on image
    /// </summary>
    [Serializable]
    public class ImageNotSupportedException : Exception
    {
        /// <summary>
        ///     Corrupt, incorrect or unhandled feature found on image
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="inner">Inner.</param>
        public ImageNotSupportedException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Corrupt, incorrect or unhandled feature found on image
        /// </summary>
        /// <param name="message">Message.</param>
        public ImageNotSupportedException(string message) : base(message) { }

        /// <summary>
        ///     Corrupt, incorrect or unhandled feature found on image
        /// </summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected ImageNotSupportedException(SerializationInfo info, StreamingContext context)
        {
            if(info == null) throw new ArgumentNullException(nameof(info));
        }
    }
}