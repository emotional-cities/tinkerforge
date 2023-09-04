using System;

namespace EmotionalCities.Tinkerforge
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class DeviceTypeAttribute : Attribute
    {
        public DeviceTypeAttribute(Type type)
        {
            DeviceType = type;
        }

        public Type DeviceType { get; }
    }
}
