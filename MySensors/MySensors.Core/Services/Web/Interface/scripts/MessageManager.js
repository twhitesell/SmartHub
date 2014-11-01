
function MessageManager() {
    var me = this;
    
    this.IsFromServer = false;
    //----------------------------------------------------------------------------------------------------------------------
    // Events:
    this.onSend = null;
    this.ProcessMessage = function (txt) {
        var msg = new NetworkMessage(NetworkMessageID.Undefined);
        msg.FromText(txt);

        var response = processMessage(msg);
        send(response);
    }
    //----------------------------------------------------------------------------------------------------------------------
    // Public functions (commands):
    this.GetSettings = function () {
        send(new NetworkMessage(NetworkMessageID.Settings));
    }
    this.SetSettings = function (webTheme, unitSystem) {
        var msg = new NetworkMessage(NetworkMessageID.Settings);
        msg.SetParameter("WebTheme", webTheme);
        msg.SetParameter("UnitSystem", unitSystem);
        send(msg);
    }
    this.GetVersion = function () {
        send(new NetworkMessage(NetworkMessageID.Version));
    }
    this.GetNodes = function () {
        send(new NetworkMessage(NetworkMessageID.GetNodes));
    }
    //----------------------------------------------------------------------------------------------------------------------
    function send(msg) {
        if (me.onSend && msg)
            me.onSend(msg.ToText());
    }
    function processMessage(msg) {
        me.IsFromServer = true;

        var response = null;

        switch (msg.GetID()) {
            case NetworkMessageID.OK:
                mainView.showDialog(msg.GetParameter("Msg") || "Operation completed successfully!");
                break;
            case NetworkMessageID.Information:
                mainView.showDialog(msg.GetParameter("Msg"), "Information");
                break;
            case NetworkMessageID.Warning:
                mainView.showDialog(msg.GetParameter("Msg"), "Warning");
                break;
            case NetworkMessageID.Error:
                mainView.showDialog(msg.GetParameter("Msg"), "Error");
                break;
            case NetworkMessageID.Settings:
                viewModel.set("Settings.WebTheme", msg.GetParameter("WebTheme"));
                viewModel.set("Settings.UnitSystem", msg.GetParameter("UnitSystem"));
                break;
            case NetworkMessageID.Version:
                viewModel.set("Version", msg.GetParameter("Version"));
                break;
            case NetworkMessageID.GetNodes:
                var newItems = JSON.parse(msg.GetParameter("Nodes"));

                // approach #1:
                for (var i = 0; i < newItems.length; i++)
                    enrichItem(newItems[i]);
                viewModel.set("Devices", newItems);

                // approach #2:
                //var oldItems = viewModel.get("Devices");
                //oldItems.splice(0, oldItems.length); // remove all old nodes
                //for (var i = 0; i < newItems.length; i++) {
                //    var item = newItems[i];
                //    enrichItem(item);
                //    oldItems.push(item);
                //}

                break;
            case NetworkMessageID.NodePresentation:
                var node = JSON.parse(msg.GetParameter("Node"));
                addOrUpdateNode(node);
                break;
            case NetworkMessageID.SensorPresentation:
                var sensor = JSON.parse(msg.GetParameter("Sensor"));
                addOrUpdateSensor(sensor);
                break;
            case NetworkMessageID.BatteryLevel:
                var bl = JSON.parse(msg.GetParameter("Level"));
                bl.Time = new Date(bl.Time);
                addBatteryLevel(bl);
                break;
            case NetworkMessageID.SensorValue:
                var sv = JSON.parse(msg.GetParameter("Value"));
                sv.Time = new Date(sv.Time);
                addSensorValue(sv);
                break;
            default:
                break;
        }

        me.IsFromServer = false;

        return response;

        function enrichItem(item) {
            item.BatteryLevels = item.BatteryLevels || [];//new kendo.observableArray();
            for (var i = 0; i < item.BatteryLevels.length; i++)
                item.BatteryLevels[i].Time = new Date(item.BatteryLevels[i].Time); // convert battery level's date/time

            item.TypeName = function () {
                return getSensorTypeName(this.get("Type"));
            };
            item.LastBatteryLevel = function () {
                var bls = this.get("BatteryLevels");
                return bls.length ? bls[bls.length - 1].Percent : "-";
            };

            item.Sensors = item.Sensors || [];//new kendo.observableArray();
            for (var i = 0; i < item.Sensors.length; i++) {
                var sensor = item.Sensors[i];

                sensor.Values = sensor.Values || [];//new kendo.observableArray();
                for (var j = 0; j < sensor.Values.length; j++)
                    sensor.Values[j].Time = new Date(sensor.Values[j].Time); // convert sensor value's date/time

                sensor.TypeName = function () {
                    return getSensorTypeName(this.get("Type"));
                };
                sensor.LastValue = function () {
                    var vs = this.get("Values");
                    return vs.length ? vs[vs.length - 1].Value : "-";
                };
                sensor.LastValueType = function () {
                    var vs = this.get("Values");
                    return vs.length ? vs[vs.length - 1].Type : "-";
                };
            }
        }
        function addOrUpdateNode(node) {
            if (!viewModel.Devices)
                viewModel.Devices = new kendo.observableArray();

            var nodes = viewModel.Devices;
            for (var i = 0; i < nodes.length; i++) {
                var nd = nodes[i];
                if (nd.ID == node.ID) {
                    nd.set("Type", node.Type);
                    nd.set("ProtocolVersion", node.ProtocolVersion);
                    nd.set("SketchName", node.SketchName );
                    nd.set("SketchVersion", node.SketchVersion);
                    return;
                }
            }

            // node isn't in list; add it:
            nodes.push(node);
        }
        function addOrUpdateSensor(sensor) {
            var nodes = viewModel.Devices || [];
            for (var i = 0; i < nodes.length; i++) {
                var nd = nodes[i];
                if (nd.ID == sensor.NodeID) {
                    if (!nd.Sensors)
                        nd.Sensors = new kendo.observableArray();

                    for (var j = 0; j < nd.Sensors.length; j++) {
                        var snsr = nd.Sensors[j];
                        if (snsr.NodeID == sensor.NodeID && snsr.ID == sensor.ID) {
                            snsr.set("Type", sensor.Type);
                            snsr.set("ProtocolVersion", sensor.ProtocolVersion);
                            return;
                        }
                    }

                    // sensor isn't in list; add it:
                    nd.Sensors.push(sensor);
                }
            }
        }
        function addBatteryLevel(bl) {
            var nodes = viewModel.Devices || [];
            for (var i = 0; i < nodes.length; i++)
                if (nodes[i].ID == bl.NodeID)
                    nodes[i].BatteryLevels.push(bl);
        }
        function addSensorValue(sv) {
            var nodes = viewModel.Devices || [];
            for (var i = 0; i < nodes.length; i++)
                if (nodes[i].ID == sv.NodeID) {
                    var sensors = nodes[i].Sensors || [];
                    for (var j = 0; j < sensors.length; j++)
                        if (sensors[j].NodeID == sv.NodeID && sensors[j].ID == sv.ID) {
                            if (!sensors[j].Values)
                                sensors[j].Values = new kendo.observableArray();
                            sensors[j].Values.push(sv);
                        }
                }
        }
        function getSensorTypeName(value) {
            for (var name in SensorType)
                if (SensorType[name] == value)
                    return name;

            return "Unknown";
        }
    }
}