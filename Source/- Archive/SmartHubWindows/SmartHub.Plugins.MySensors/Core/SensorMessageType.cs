﻿
namespace SmartHub.Plugins.MySensors.Core
{
    public enum SensorMessageType : byte
    {
        Presentation = 0,       // Sent by nodes when they present attached sensors. This is usually done in setup() at startup.
        Set = 1,                // This message is sent from or to a sensor when a sensor value should be updated.
        Request = 2,            // Requests a variable value (usually from an actuator destined for controller).
        Internal = 3,           // This is a special internal message.
        Stream = 4              // Used for OTA firmware updates.
    }
}
