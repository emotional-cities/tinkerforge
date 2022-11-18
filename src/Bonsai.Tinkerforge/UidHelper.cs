using System;

namespace Bonsai.Tinkerforge
{
    internal static class UidHelper
    {
        public static string ThrowIfNullOrEmpty(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new InvalidOperationException("A bricklet device UID must be specified.");
            }

            return uid;
        }
    }
}
