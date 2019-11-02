using System;

namespace DiscImageChef.Devices
{
    /// <summary>
    ///     Exception to be returned by the device constructor
    /// </summary>
    public class DeviceException : Exception
    {
        internal DeviceException(string message) : base(message)
        {
        }

        internal DeviceException(int lastError)
        {
            LastError = lastError;
        }

        /// <summary>
        ///     Last error sent by the operating systen
        /// </summary>
        public int LastError { get; }
    }
}