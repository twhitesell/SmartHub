﻿using SmartHub.Plugins.MySensors.GatewayProxies;

namespace SmartHub.Plugins.MySensors.Core
{
    delegate void SensorMessageEventHandler(IGatewayProxy sender, SensorMessageEventArgs args);
}
