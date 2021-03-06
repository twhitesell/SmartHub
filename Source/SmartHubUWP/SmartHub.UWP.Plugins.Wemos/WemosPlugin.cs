﻿using Newtonsoft.Json;
using SmartHub.UWP.Core.Communication.Udp;
using SmartHub.UWP.Core.Plugins;
using SmartHub.UWP.Plugins.ApiListener;
using SmartHub.UWP.Plugins.ApiListener.Attributes;
using SmartHub.UWP.Plugins.Things;
using SmartHub.UWP.Plugins.Things.Models;
using SmartHub.UWP.Plugins.Scripts;
using SmartHub.UWP.Plugins.Scripts.Attributes;
using SmartHub.UWP.Plugins.UI.Attributes;
using SmartHub.UWP.Plugins.Wemos.Core.Messages;
using SmartHub.UWP.Plugins.Wemos.Core.Models;
using SmartHub.UWP.Plugins.Wemos.Infrastructure.Controllers.Models;
using SmartHub.UWP.Plugins.Wemos.Infrastructure.Monitors;
using SmartHub.UWP.Plugins.Wemos.Infrastructure.Monitors.Models;
using SmartHub.UWP.Plugins.Wemos.UI;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.UI.Core;
using SmartHub.UWP.Core;

namespace SmartHub.UWP.Plugins.Wemos
{
    [Plugin]
    //[AppSectionItem("Wemos", AppSectionType.Applications, typeof(MainPage), "Wemos modules")]
    [AppSectionItem("Wemos", AppSectionType.System, typeof(SettingsPage), "ESP8266 based Wi-Fi modules", "/api/wemos/ui/settings")]
    public class WemosPlugin : PluginBase
    {
        public const int BroadcastID = -1;

        #region Fields
        private const string localService = "11111";
        private const string remoteService = "22222";
        private const string remoteMulticastAddress = "224.3.0.5";
        //private const string broadcastAddress = "255.255.255.255";
        private UdpClient udpClient = new UdpClient();

        private List<WemosController> oldControllers = new List<WemosController>();
        private List<WemosLineController> controllers = new List<WemosLineController>();
        //private Task taskControllers;
        //private CancellationTokenSource ctsControllers;
        //private bool isTaskControllersActive = false;
        #endregion

        #region Imports
        //[ImportMany]
        //public Action<WemosMessage>[] WemosMessageHandlers
        //{
        //    get; set;
        //}

        //[ScriptEvent("wemos.connected")]
        //public ScriptEventHandlerDelegate[] OnConnectedForScripts
        //{
        //    get; set;
        //}
        //private void NotifyConnectedForScripts()
        //{
        //    this.RaiseScriptEvent(x => x.OnConnectedForScripts);
        //}
        #endregion

        #region Exports
        //[Export(typeof(Action<DateTime>)), RunPeriodically(Interval = 1)]
        //public Action<DateTime> TimerElapsed => (dt =>
        //{
        //    foreach (var controller in Context.GetPlugin<WemosPlugin>().controllers)
        //        controller.ProcessTimer(dt);
        //});

        //[ScriptCommand("wemosSend")]
        //public ScriptCommand scriptSendCommand => (args =>
        //{
        //    var nodeID = int.Parse(args[0].ToString());
        //    var lineID = int.Parse(args[1].ToString());
        //    var messageType = int.Parse(args[2].ToString());
        //    var messageSubtype = int.Parse(args[3].ToString());
        //    var value = args[4].ToString();

        //    return Context.GetPlugin<WemosPlugin>().SendAsync(new WemosMessage(nodeID, lineID, (WemosMessageType)messageType, messageSubtype).Set(value));
        //});

        //[ScriptCommand("wemosSetLineValue")]
        //public ScriptCommand scriptSetLineValue => (args =>
        //{
        //    var nodeID = int.Parse(args[0].ToString());
        //    var lineID = int.Parse(args[1].ToString());
        //    var value = args[2].ToString();

        //    return Context.GetPlugin<WemosPlugin>().SetLineValueAsync(GetLine(nodeID, lineID), value);
        //});

        //[ScriptCommand("wemosDeleteLinesValues")]
        //public ScriptCommand scriptClearLinesValuesCommand => (args =>
        //{
        //    var lastDayToPreserve = int.Parse(args[0].ToString());
        //    //return Context.GetPlugin<WemosPlugin>().DeleteSensorValues(DateTime.Now.AddDays(-lastDayToPreserve));
        //    return null;
        //});
        #endregion

        #region Plugin overrides
        public override void InitDbModel()
        {
            //var db = Context.StorageGet();

            //db.CreateTable<WemosSetting>();
            //db.CreateTable<WemosNode>();
            //db.CreateTable<WemosLine>();
            //db.CreateTable<WemosNodeBatteryValue>();

            //db.CreateTable<WemosLineMonitor>();
            //db.CreateTable<WemosLineController>();
            //db.CreateTable<WemosController>();
            //db.CreateTable<WemosZone>();
        }
        public override void InitPlugin()
        {
            //if (GetSetting("UnitSystem") == null)
            //    Context.StorageSave(new WemosSetting() { Name = "UnitSystem", Value = "M" });

            udpClient.MessageReceived += async (sender, e) =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    foreach (var msg in WemosMessage.FromDto(e.Data))
                        await ProcessMessage(msg, e.RemoteAddress);
                });
            };

            //GetControllers().ForEach(controller =>
            //{
            //    controller.Init(Context);
            //    oldControllers.Add(controller);
            //});

            //GetLineControllers().ForEach(controller =>
            //{
            //    controller.Init(Context);
            //    controllers.Add(controller);
            //});
        }
        public override async void StartPlugin()
        {
            //await udpClient.Start(localService, remoteMulticastAddress);

            //await RequestPresentationAsync();
            //await Task.Delay(1000);
            //await RequestLinesValueAsync();



            //foreach (var controller in oldControllers)
            //    controller.Start();

            //StartControllersTask();
        }
        public override async void StopPlugin()
        {
            //await udpClient.Stop();



            //StopControllersTask();
        }
        #endregion

        #region Public methods

        #region Send/Receive
        public async Task SendAsync(WemosMessage msg)
        {
            try
            {
                if (msg != null)
                    await udpClient.Send(remoteMulticastAddress, remoteService, msg.ToDto());
            }
            catch (Exception ex)
            {
            }
        }

        public async Task RequestPresentationAsync(int nodeID = BroadcastID, int lineID = BroadcastID)
        {
            await SendAsync(new WemosMessage(nodeID, lineID, WemosMessageType.Presentation));
        }

        public async Task RequestLinesValueAsync()
        {
            //GetLines().ForEach(async line => await RequestLineValueAsync(line));
            foreach (var line in GetLines())
                await RequestLineValueAsync(line);
        }
        public async Task RequestLineValueAsync(WemosLine line)
        {
            if (line != null)
                await SendAsync(new WemosMessage(line.NodeID, line.LineID, WemosMessageType.Get, (int) line.Type));
        }

        public async Task SetLineValueAsync(WemosLine line, float value)
        {
            if (line != null && Context.GetPlugin<ThingsPlugin>().GetLineLastValue(line.ID)?.Value != value)
                await SendAsync(new WemosMessage(line.NodeID, line.LineID, WemosMessageType.Set, (int) line.Type).Set(value));
        }
        public async Task SetLineValueAsync(WemosLine line, string value)
        {
            if (line != null)
                await SendAsync(new WemosMessage(line.NodeID, line.LineID, WemosMessageType.Set, (int) line.Type).Set(value));
        }

        public async Task RebootNodeAsync(int nodeID)
        {
            await SendAsync(new WemosMessage(nodeID, -1, WemosMessageType.Internal, (int) WemosInternalMessageType.Reboot));
        }
        #endregion

        #region Nodes & lines
        public List<WemosNode> GetNodes()
        {
            return Context.StorageGet().Table<WemosNode>().ToList();
        }
        public WemosNode GetNode(int nodeID)
        {
            //Context.StorageOpen().TraceListener = new DebugTraceListener(); // activate tracing
            return Context.StorageGet().Table<WemosNode>().FirstOrDefault(n => n.NodeID == nodeID);
        }

        public List<WemosLine> GetLines()
        {
            return Context.StorageGet().Table<WemosLine>()
                .Select(l => { l.LastTimeStamp = l.LastTimeStamp.ToLocalTime(); return l; }) // time in DB is in UTC; convert to local
                .ToList();
        }
        public WemosLine GetLine(int nodeID, int lineID)
        {
            return Context.StorageGet().Table<WemosLine>().FirstOrDefault(l => l.NodeID == nodeID && l.LineID == lineID);
        }
        public WemosLine GetLine(string id)
        {
            return Context.StorageGet().Table<WemosLine>().FirstOrDefault(l => l.ID == id);
        }

        public static bool IsMessageFromLine(WemosMessage msg, WemosLine line)
        {
            return msg != null && line != null && line.NodeID == msg.NodeID && line.LineID == msg.LineID;
        }
        public static string LineTypeToUnits(LineType lt)
        {
            switch (lt)
            {
                case LineType.Switch: return "";
                case LineType.Temperature: return "°C";
                case LineType.Humidity: return "%";
                case LineType.Pressure: return "Pa"; // mm Hg
                case LineType.Weight: return "kg";
                case LineType.Voltage: return "V";
                case LineType.Current: return "A";
                case LineType.Power: return "Wt";
                case LineType.Rain: return "";
                case LineType.UV: return "";
                case LineType.Distance: return "m";
                case LineType.LightLevel: return "lux";
                case LineType.IR: return "";
                case LineType.AirQuality: return "";
                case LineType.Vibration: return "";
                case LineType.Ph: return "";
                case LineType.ORP: return "";

                default: return "";
            }
        }

        //public static bool IsWemosLine(Line line)
        //{
        //    return line.ID.StartsWith("WEMOS");
        //}

        public static string BuildDeviceID(int nodeID)
        {
            return $"WEMOS-{nodeID}";
        }
        public static string BuildLineID(int nodeID, int lineNumber)
        {
            return $"{BuildDeviceID(nodeID)}-{lineNumber}";
        }
        #endregion

        #region Line Monitors
        public List<WemosLineMonitor> GetLineMonitors()
        {
            return Context.StorageGet().Table<WemosLineMonitor>().ToList();
        }
        public WemosLineMonitor GetLineMonitor(string id)
        {
            return Context.StorageGet().Table<WemosLineMonitor>().FirstOrDefault(m => m.ID == id);
        }
        #endregion

        #region Line Controllers
        public List<WemosLineController> GetLineControllers()
        {
            return Context.StorageGet().Table<WemosLineController>().ToList();
        }
        public WemosLineController GetLineController(string id)
        {
            return Context.StorageGet().Table<WemosLineController>().FirstOrDefault(m => m.ID == id);
        }

        public List<WemosController> GetControllers()
        {
            return Context.StorageGet().Table<WemosController>().ToList();
        }
        public WemosController GetWorkingController(string id)
        {
            return oldControllers.FirstOrDefault(c => c.ID == id);
        }
        public void AddWorkingController(WemosController ctrl)
        {
            if (ctrl != null)
                oldControllers.Add(ctrl);
        }
        public void RemoveWorkingController(WemosController ctrl)
        {
            if (ctrl != null)
                oldControllers.Remove(ctrl);
        }
        #endregion

        //#region Settings
        //public WemosSetting GetSetting(string name)
        //{
        //    return Context.StorageGet().Table<WemosSetting>().Where(setting => setting.Name == name).FirstOrDefault();
        //}
        //#endregion

        #endregion

        #region Private methods
        //private void StartControllersTask()
        //{
        //    if (!isTaskControllersActive)
        //    {
        //        ctsControllers = new CancellationTokenSource();

        //        taskControllers = Task.Factory.StartNew(async () =>
        //        {
        //            while (!ctsControllers.IsCancellationRequested)
        //                if (isTaskControllersActive)
        //                {
        //                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
        //                    {
        //                        for (int i = 0; i < Context.GetPlugin<WemosPlugin>().oldControllers.Count; i++)
        //                            Context.GetPlugin<WemosPlugin>().oldControllers[i].ProcessTimer(DateTime.Now);

        //                        //var controllers = Context.GetPlugin<WemosPlugin>().controllers;
        //                        //for (int i = 0; i < controllers.Count; i++)
        //                        //    await controllers[i].ProcessAsync();
        //                    });

        //                    await Task.Delay(50);
        //                }

        //        }, ctsControllers.Token);

        //        isTaskControllersActive = true;
        //    }
        //}
        //private void StopControllersTask()
        //{
        //    if (isTaskControllersActive)
        //    {
        //        ctsControllers?.Cancel();
        //        isTaskControllersActive = false;
        //    }
        //}

        private async Task ProcessMessage(WemosMessage message, HostName remoteAddress)
        {
            if (message == null)
                return;

            switch (message.Type)
            {
                #region Presentation
                case WemosMessageType.Presentation: // sent by nodes when they present attached lines.
                    if (message.LineID == -1) // node
                    {
                        //var device = new Device()
                        //{
                        //    ID = BuildDeviceID(message.NodeID),
                        //    Name = $"Wemos Node {message.NodeID}",
                        //    Type = DeviceType.ESP,
                        //    IPAddress = remoteAddress.CanonicalName
                        //};

                        //Context.GetPlugin<ThingsPlugin>().RegisterDevice(device);
                    }
                    else // line
                    {
                        var line = new Line()
                        {
                            ID = BuildLineID(message.NodeID, message.LineID),
                            DeviceID = BuildDeviceID(message.NodeID),
                            LineID = message.LineID.ToString(),
                            Name = $"Wemos Line {message.NodeID}-{message.LineID}",
                            Type = (LineType) message.SubType,
                            Factor = 1,
                            Offset = 0
                        };

                        Context.GetPlugin<ThingsPlugin>().RegisterLine(line);
                    }
                    break;
                #endregion

                #region Report
                case WemosMessageType.Report: Context.GetPlugin<ThingsPlugin>().UpdateLineValue(BuildLineID(message.NodeID, message.LineID), message.GetFloat()); break;
                #endregion

                #region Internal
                case WemosMessageType.Internal:
                    switch ((WemosInternalMessageType)message.SubType)
                    {
                        case WemosInternalMessageType.Time: await SendAsync(new WemosMessage(message.NodeID, message.LineID, WemosMessageType.Internal, (int) WemosInternalMessageType.Time).Set(CoreUtils.GetUnixSeconds())); break;
                        case WemosInternalMessageType.Config: await SendAsync(new WemosMessage(message.NodeID, -1, WemosMessageType.Internal, (int) WemosInternalMessageType.Config).Set("M")); break;
                    }
                    break;
                #endregion

                #region Stream
                //case WemosMessageType.Stream: //used for OTA firmware updates
                //    switch ((StreamValueType) message.SubType)
                //    {
                //        case StreamValueType.Sound:
                //            break;
                //        case StreamValueType.Image:
                //            break;
                //    }
                //    break;
                #endregion
            }
        }
        //private async Task ProcessMessageOld(WemosMessage message, HostName remoteAddress)
        //{
        //    if (message == null)
        //        return;

        //    var node = GetNode(message.NodeID);
        //    var line = GetLine(message.NodeID, message.LineID);

        //    switch (message.Type)
        //    {
        //        #region Presentation
        //        case WemosMessageType.Presentation: // sent by nodes when they present attached lines.
        //            if (message.LineID == -1) // node
        //            {
        //                if (node == null)
        //                {
        //                    node = new WemosNode
        //                    {
        //                        NodeID = message.NodeID,
        //                        Name = $"Node {message.NodeID}",
        //                        Type = (LineType)message.SubType,
        //                        ProtocolVersion = message.GetFloat(),
        //                        IPAddress = remoteAddress.CanonicalName
        //                    };

        //                    Context.StorageSave(node);
        //                }
        //                else
        //                {
        //                    node.Type = (LineType)message.SubType;
        //                    node.ProtocolVersion = message.GetFloat();
        //                    node.IPAddress = remoteAddress.CanonicalName;

        //                    Context.StorageSaveOrUpdate(node);
        //                }

        //                //NotifyMessageReceivedForPlugins(message);
        //                //NotifyMessageReceivedForScripts(message);
        //                //NotifyForSignalR(new { MsgId = "NodePresentation", Data = BuildNodeRichWebModel(node) });
        //            }
        //            else // line
        //            {
        //                if (node != null)
        //                {
        //                    if (line == null)
        //                    {
        //                        line = new WemosLine()
        //                        {
        //                            ID = GetLineID(message.NodeID, message.LineID),
        //                            Name = $"Wemos Line {message.NodeID}-{message.LineID}",
        //                            Type = (LineType)message.SubType,
        //                            ProtocolVersion = message.GetFloat(),
        //                            Factor = 1,
        //                            Offset = 0
        //                        };

        //                        Context.StorageSave(line);
        //                    }
        //                    else
        //                    {
        //                        line.Type = (LineType)message.SubType;
        //                        line.ProtocolVersion = message.GetFloat();

        //                        Context.StorageSaveOrUpdate(line);
        //                    }

        //                    //NotifyMessageReceivedForPlugins(message);
        //                    //NotifyMessageReceivedForScripts(message);
        //                    //NotifyForSignalR(new { MsgId = "SensorPresentation", Data = BuildSensorRichWebModel(line) });
        //                }
        //            }
        //            break;
        //        #endregion

        //        #region Report
        //        case WemosMessageType.Report:
        //            if (line != null)
        //            {
        //                // save value:
        //                var lv = new LineValue()
        //                {
        //                    LineID = line.ID,
        //                    TimeStamp = DateTime.UtcNow,
        //                    Value = line.Factor * message.GetFloat() + line.Offset // tune value
        //                };
        //                Context.StorageSave(lv);

        //                // update line:
        //                line.LastTimeStamp = lv.TimeStamp;
        //                line.LastValue = lv.Value;
        //                Context.StorageSaveOrUpdate(line);

        //                // process:
        //                //NotifyForSignalR(new { MsgId = "MySensorsTileContent", Data = BuildTileContent() }); // update MySensors tile
        //                Run(WemosMessageHandlers, method => method(message));
        //                //NotifyMessageReceivedForScripts(message);
        //                //NotifyForSignalR(new { MsgId = "SensorValue", Data = sv }); // notify Web UI

        //                //foreach (var controller in oldControllers)
        //                //    controller.ProcessMessage(lv);
        //            }
        //            break;
        //        #endregion

        //        #region Set
        //        case WemosMessageType.Set: // sent to a line when a line value should be updated
        //            break;
        //        #endregion

        //        #region Request
        //        case WemosMessageType.Get: // requests a variable value (usually from an actuator destined for controller)
        //            break;
        //        #endregion

        //        #region Internal
        //        case WemosMessageType.Internal:
        //            var imt = (WemosInternalMessageType)message.SubType;
        //            switch (imt)
        //            {
        //                case WemosInternalMessageType.BatteryLevel: // int, in %
        //                    if (node != null)
        //                    {
        //                        WemosNodeBatteryValue bl = new WemosNodeBatteryValue()
        //                        {
        //                            NodeID = message.NodeID,
        //                            TimeStamp = DateTime.Now,
        //                            Value = (int)message.GetInteger()
        //                        };
        //                        Context.StorageSave(bl);

        //                        node.LastTimeStamp = bl.TimeStamp;
        //                        node.LastBatteryValue = bl.Value;
        //                        node.IPAddress = remoteAddress.CanonicalName;
        //                        Context.StorageSaveOrUpdate(node);

        //                        //NotifyMessageReceivedForPlugins(message);
        //                        //NotifyMessageReceivedForScripts(message);
        //                        //NotifyForSignalR(new { MsgId = "BatteryValue", Data = bl });
        //                    }
        //                    break;
        //                case WemosInternalMessageType.Time:
        //                    await SendAsync(new WemosMessage(message.NodeID, message.LineID, WemosMessageType.Internal, (int)WemosInternalMessageType.Time).Set(CoreUtils.GetUnixSeconds()));
        //                    break;
        //                case WemosInternalMessageType.Version:
        //                    if (node != null)
        //                    {
        //                        node.ProtocolVersion = message.GetFloat();
        //                        Context.StorageSaveOrUpdate(node);
        //                    }
        //                    break;
        //                case WemosInternalMessageType.Config:
        //                    //await SendAsync(new WemosMessage(message.NodeID, -1, WemosMessageType.Internal, (int) WemosInternalMessageType.Config).Set(GetSetting("UnitSystem").Value));
        //                    await SendAsync(new WemosMessage(message.NodeID, -1, WemosMessageType.Internal, (int)WemosInternalMessageType.Config).Set("M"));
        //                    break;
        //                case WemosInternalMessageType.FirmwareName:
        //                case WemosInternalMessageType.FirmwareVersion:
        //                    if (node != null)
        //                    {
        //                        if (imt == WemosInternalMessageType.FirmwareName)
        //                            node.FirmwareName = message.GetString();
        //                        else
        //                            node.FirmwareVersion = message.GetFloat();

        //                        Context.StorageSaveOrUpdate(node);

        //                        //NotifyMessageReceivedForPlugins(message);
        //                        //NotifyMessageReceivedForScripts(message);
        //                        //NotifyForSignalR(new { MsgId = "NodePresentation", Data = BuildNodeRichWebModel(node) });
        //                    }
        //                    break;
        //            }
        //            break;
        //            #endregion

        //        #region Stream
        //        //case WemosMessageType.Stream: //used for OTA firmware updates
        //        //    switch ((StreamValueType) message.SubType)
        //        //    {
        //        //        case StreamValueType.FirmwareConfigRequest:
        //        //            var fwtype = pullWord(payload, 0);
        //        //            var fwversion = pullWord(payload, 2);
        //        //            sendFirmwareConfigResponse(sender, fwtype, fwversion, db, gw);
        //        //            break;
        //        //        case StreamValueType.FirmwareConfigResponse:
        //        //            break;
        //        //        case StreamValueType.FirmwareRequest:
        //        //            break;
        //        //        case StreamValueType.FirmwareResponse:
        //        //            var fwtype = pullWord(payload, 0);
        //        //            var fwversion = pullWord(payload, 2);
        //        //            var fwblock = pullWord(payload, 4);
        //        //            sendFirmwareResponse(sender, fwtype, fwversion, fwblock, db, gw);
        //        //            break;
        //        //        case StreamValueType.Sound:
        //        //            break;
        //        //        case StreamValueType.Image:
        //        //            break;
        //        //    }
        //        //    break;
        //        #endregion
        //    }
        //}
        #endregion

        #region Remote API

        //[ApiMethod(MethodName = "/api/wemos/presentation"), Export(typeof(ApiMethod))]
        [ApiMethod("/api/wemos/presentation")]
        public ApiMethod apiPresentation => (args =>
        {
            Context.GetPlugin<WemosPlugin>().RequestPresentationAsync();

            return true;
        });

        #region Nodes
        [ApiMethod("/api/wemos/nodes")]
        public ApiMethod apiGetNodes => (args =>
        {
            return Context.GetPlugin<WemosPlugin>().GetNodes();
        });

        [ApiMethod("/api/wemos/nodes/update")]
        public ApiMethod apiUpdateNode => (args =>
        {
            var item = JsonConvert.DeserializeObject<WemosNode>(args[0].ToString());

            if (item != null)
            {
                Context.StorageSaveOrUpdate(item);
                //NotifyForSignalR(new { MsgId = "NodeNameChanged", Data = new { Id = id, Name = name } });
                return true;
            }

            return false;
        });
        #endregion

        #region Lines
        [ApiMethod("/api/wemos/lines")]
        public ApiMethod apiGetLines => (args =>
        {
            return Context.GetPlugin<WemosPlugin>().GetLines();
        });

        [ApiMethod("/api/wemos/lines/update")]
        public ApiMethod apiUpdateLine => (args =>
        {
            var item = JsonConvert.DeserializeObject<WemosLine>(args[0].ToString());

            if (item != null)
            {
                Context.StorageSaveOrUpdate(item);
                //NotifyForSignalR(new { MsgId = "SensorNameChanged", Data = new { Id = id, Name = name } });
                return true;
            }

            return false;
        });

        [ApiMethod("/api/wemos/line/values")]
        public ApiMethod apiGetLineValues => (args =>
        {
            var id = args[0].ToString();
            var count = int.Parse(args[1].ToString());

            return Context.GetPlugin<ThingsPlugin>().GetLineValues(id, count);
        });
        #endregion

        #region Line Monitors
        [ApiMethod("/api/wemos/monitors")]
        public ApiMethod apiGetMonitors => (args =>
        {
            return Context.GetPlugin<WemosPlugin>().GetLineMonitors().Select(m => new WemosLineMonitorDto(m)
            {
                LineName = Context.GetPlugin<WemosPlugin>().GetLine(m.LineID).Name,
                LineType = Context.GetPlugin<WemosPlugin>().GetLine(m.LineID).Type
            }).ToList();
        });

        [ApiMethod("/api/wemos/monitor")]
        public ApiMethod apiGetMonitor => (args =>
        {
            var model = Context.GetPlugin<WemosPlugin>().GetLineMonitor(args[0].ToString());
            var line = Context.GetPlugin<WemosPlugin>().GetLine(model.LineID);

            return new WemosLineMonitorDto(model) { LineName = line.Name, LineType = line.Type };
        });

        [ApiMethod("/api/wemos/monitors/add")]
        public ApiMethod apiAddMonitor => (args =>
        {
            var lineID = args[0].ToString();
            var line = Context.GetPlugin<WemosPlugin>().GetLine(lineID);
            var min = float.Parse(args[1].ToString());
            var max = float.Parse(args[2].ToString());

            var model = new WemosLineMonitor()
            {
                LineID = lineID,
                Min = min,
                Max = max,
                ValuesCount = 10,
                Factor = 1,
                Offset = 0,
                Precision = 0,
                Units = LineTypeToUnits(line.Type)
            };

            Context.StorageSave(model);

            //NotifyForSignalR(new { MsgId = "MonitorAdded", Data = BuildMonitorWebModel(ctrl) });

            return new WemosLineMonitorDto(model) { LineName = line.Name, LineType = line.Type };
        });

        [ApiMethod("/api/wemos/monitors/update")]
        public ApiMethod apiUpdateMonitor => (args =>
        {
            var item = JsonConvert.DeserializeObject<WemosLineMonitor>(args[0].ToString());

            if (item != null)
            {
                Context.StorageSaveOrUpdate(item);
                return true;
            }

            return false;
        });

        [ApiMethod("/api/wemos/monitors/delete")]
        public ApiMethod apiDeleteMonitor => (args =>
        {
            var item = Context.GetPlugin<WemosPlugin>().GetLineMonitor(args[0].ToString());

            if (item != null)
            {
                Context.StorageDelete(item);
                return true;
            }

            return false;
        });
        #endregion

        #region Line Controllers
        [ApiMethod("/api/wemos/controllers")]
        public ApiMethod apiGetControllers => (args =>
        {
            return Context.GetPlugin<WemosPlugin>().GetControllers();
        });

        [ApiMethod("/api/wemos/controllers/add")]
        public ApiMethod apiAddController => (args =>
        {
            var name = args[0].ToString();
            var type = (WemosControllerType)int.Parse(args[1].ToString());

            var ctrl = new WemosController()
            {
                Name = name,
                Type = type,
                IsAutoMode = false,
                Configuration = null
            };
            ctrl.Init(Context);

            Context.StorageSave(ctrl);
            Context.GetPlugin<WemosPlugin>().AddWorkingController(ctrl);

            ctrl.Start();
            //NotifyForSignalR(new { MsgId = "ControllerAdded", Data = BuildControllerWebModel(ctrl) });

            return ctrl;
        });

        [ApiMethod("/api/wemos/controllers/update")]
        public ApiMethod apiUpdateController => (args =>
        {
            var item = JsonConvert.DeserializeObject<WemosController>(args[0].ToString());

            if (item != null)
            {
                Context.StorageSaveOrUpdate(item);

                var ctrl = Context.GetPlugin<WemosPlugin>().GetWorkingController(item.ID);
                if (ctrl != null)
                {
                    ctrl.Name = item.Name;
                    ctrl.IsAutoMode = item.IsAutoMode;
                    ctrl.Configuration = item.Configuration;
                }

                return true;
            }

            return false;
        });

        [ApiMethod("/api/wemos/controllers/delete")]
        public ApiMethod apiDeleteController => (args =>
        {
            var item = Context.GetPlugin<WemosPlugin>().GetWorkingController(args[0].ToString());

            if (item != null)
            {
                Context.StorageDelete(item);
                Context.GetPlugin<WemosPlugin>().RemoveWorkingController(item);
                return true;
            }

            return false;
        });
        #endregion

        #region Web UI
        [ApiMethod("/api/wemos/ui/settings")]
        public ApiMethod apiGetUISettings => (args =>
        {
            var stream = GetType().GetTypeInfo().Assembly.GetManifestResourceStream("SmartHub.UWP.Plugins.Wemos.UIWeb.Settings.html");
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        });
        #endregion

        #endregion
    }
}
