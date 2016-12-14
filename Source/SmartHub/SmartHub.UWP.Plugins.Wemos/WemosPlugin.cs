﻿using SmartHub.UWP.Core.Plugins;
using SmartHub.UWP.Plugins.Wemos.Core;
using SmartHub.UWP.Plugins.Wemos.Models;
using SmartHub.UWP.Plugins.Wemos.Transport;
using System;
using System.Threading.Tasks;

namespace SmartHub.UWP.Plugins.Wemos
{
    public class WemosPlugin : PluginBase
    {
        #region Fields
        private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private WemosTransport transport = new WemosTransport();
        #endregion

        public event WemosMessageEventHandler MessageReceived;

        #region Plugin ovverrides
        //public override void InitDbModel(ModelMapper mapper)
        //{
        //    mapper.Class<MySensorsSetting>(cfg => cfg.Table("MySensors_Settings"));
        //    mapper.Class<Node>(cfg => cfg.Table("MySensors_Nodes"));
        //    mapper.Class<Sensor>(cfg => cfg.Table("MySensors_Sensors"));
        //    mapper.Class<BatteryLevel>(cfg => cfg.Table("MySensors_BatteryLevels"));
        //    mapper.Class<SensorValue>(cfg => cfg.Table("MySensors_SensorValues"));
        //}
        public override void InitPlugin()
        {
            transport.MessageReceived += OnMessageReceived;

            if (GetSetting("UnitSystem") == null)
                Save(new WemosSetting() { Name = "UnitSystem", Value = "M" });
        }
        public override async void StartPlugin()
        {
            await transport.Open();
            await RequestPresentation();
        }
        public override void StopPlugin()
        {
            transport.Close();
        }
        #endregion

        #region API
        public async Task Send(WemosMessage data, bool isBrodcast = false)
        {
            await transport.Send(data, isBrodcast);
        }
        public async Task RequestPresentation(int nodeID = -1, int lineID = -1)
        {
            await Send(new WemosMessage(nodeID, lineID, WemosMessageType.Presentation, -1));
        }
        public async Task RequestLineValue(WemosLine line)
        {
            await Send(new WemosMessage(line.NodeID, line.LineID, WemosMessageType.Get, (int) line.Type));
        }
        //public async void SetLineValue(WemosLine sensor, float value)
        //{
        //    //if (sensor != null)
        //    //{
        //    //    var lastSV = GetLastSensorValue(sensor);
        //    //    if (lastSV == null || (lastSV.Value != value))
        //    //        await Send(new WemosMessage(sensor.NodeID, sensor.LineID, WemosMessageType.Set, (int) sensor.Type, value));
        //    //}
        //}
        //public async void SetLineValue(WemosLine sensor, string value)
        //{
        //    await Send(new WemosMessage(sensor.NodeID, sensor.LineID, WemosMessageType.Set, (int) sensor.Type, value));
        //}
        public static bool IsMessageFromLine(WemosMessage msg, WemosLine line)
        {
            return msg != null && line != null && line.NodeID == msg.NodeID && line.LineID == msg.LineID;
        }
        #endregion

        #region Event handlers
        private void OnMessageReceived(object sender, WemosMessageEventArgs e)
        {
            if (e.Message != null)
            {
                //System.Diagnostics.Debug.WriteLine(e.Message.ToString());

                MessageReceived?.Invoke(this, e); // TODO: temporary!!!
                ProcessMessage(e.Message);
            }
        }
        #endregion

        #region Private methods
        private async void ProcessMessage(WemosMessage message)
        {
            WemosNode node = null;// GetNode(message.NodeID);
            WemosLine line = null;// GetLine(message.NodeID, message.LineID); // if message.SensorID == 255 it returns null

            switch (message.Type)
            {
                #region Presentation
                case WemosMessageType.Presentation: // sent by a nodes when they present attached sensors.
                    if (message.LineID == -1) // node
                    {
                        if (node == null)
                        {
                            node = new WemosNode
                            {
                                NodeID = message.NodeID,
                                Type = (WemosLineType) message.SubType,
                                ProtocolVersion = message.GetFloat()
                            };
                            Save(node);
                        }
                        else
                        {
                            node.Type = (WemosLineType) message.SubType;
                            node.ProtocolVersion = message.GetFloat();
                            SaveOrUpdate(node);
                        }

                        //NotifyMessageReceivedForPlugins(message);
                        //NotifyMessageReceivedForScripts(message);
                        //NotifyForSignalR(new { MsgId = "NodePresentation", Data = BuildNodeRichWebModel(node) });
                    }
                    else // line
                    {
                        if (node != null)
                        {
                            if (line == null)
                            {
                                line = new WemosLine()
                                {
                                    NodeID = node.NodeID,
                                    LineID = message.LineID,
                                    Type = (WemosLineType) message.SubType,
                                    ProtocolVersion = message.GetFloat()
                                };
                                Save(line);
                            }
                            else
                            {
                                line.Type = (WemosLineType) message.SubType;
                                line.ProtocolVersion = message.GetFloat();
                                SaveOrUpdate(line);
                            }

                            //NotifyMessageReceivedForPlugins(message);
                            //NotifyMessageReceivedForScripts(message);
                            //NotifyForSignalR(new { MsgId = "SensorPresentation", Data = BuildSensorRichWebModel(line) });
                        }
                    }
                    break;
                #endregion

                #region Report
                case WemosMessageType.Report:
                    if (line != null)
                    {
                        //NotifyMessageCalibrationForPlugins(message); // before saving to DB plugins may adjust the sensor value due to their calibration params
                        var sv = SaveLineValueToDB(message);

                        //NotifyForSignalR(new { MsgId = "MySensorsTileContent", Data = BuildTileContent() }); // update MySensors tile
                        //NotifyMessageReceivedForPlugins(message);
                        //NotifyMessageReceivedForScripts(message);
                        //NotifyForSignalR(new { MsgId = "SensorValue", Data = sv }); // notify Web UI
                    }
                    break;
                #endregion

                #region Set
                case WemosMessageType.Set: // sent to a sensor when a sensor value should be updated
                    break;
                #endregion

                #region Request
                case WemosMessageType.Get: // requests a variable value (usually from an actuator destined for controller)
                    break;
                #endregion

                #region Internal
                case WemosMessageType.Internal:
                    var imt = (WemosInternalMessageType) message.SubType;
                    switch (imt)
                    {
                        case WemosInternalMessageType.BatteryLevel: // int, in %
                            if (node != null)
                            {
                                var dtDB = DateTime.UtcNow;
                                var dt = DateTime.Now;

                                WemosNodeBatteryValue bl = new WemosNodeBatteryValue()
                                {
                                    NodeID = message.NodeID,
                                    TimeStamp = dtDB,
                                    Value = message.GetInteger()
                                };

                                Save(bl);

                                bl.TimeStamp = dt;

                                //NotifyMessageReceivedForPlugins(message);
                                //NotifyMessageReceivedForScripts(message);
                                //NotifyForSignalR(new { MsgId = "BatteryValue", Data = bl });
                            }
                            break;
                        case WemosInternalMessageType.Time: // seconds since 1970
                            var sec = Convert.ToInt64(DateTime.Now.Subtract(unixEpoch).TotalSeconds);
                            await Send(new WemosMessage(message.NodeID, message.LineID, WemosMessageType.Internal, (int) WemosInternalMessageType.Time).Set(sec));
                            break;
                        case WemosInternalMessageType.Version: // float!
                            break;
                        case WemosInternalMessageType.Config:
                            await Send(new WemosMessage(message.NodeID, -1, WemosMessageType.Internal, (int) WemosInternalMessageType.Config).Set(GetSetting("UnitSystem").Value));
                            break;
                        case WemosInternalMessageType.FirmwareName:
                        case WemosInternalMessageType.FirmwareVersion:
                            if (node != null)
                            {
                                if (imt == WemosInternalMessageType.FirmwareName)
                                    node.FirmwareName = message.GetString();
                                else
                                    node.FirmwareVersion = message.GetFloat();

                                SaveOrUpdate(node);

                                //NotifyMessageReceivedForPlugins(message);
                                //NotifyMessageReceivedForScripts(message);
                                //NotifyForSignalR(new { MsgId = "NodePresentation", Data = BuildNodeRichWebModel(node) });
                            }
                            break;
                    }
                    break;
                    #endregion

                #region Stream
                //case WemosMessageType.Stream: //used for OTA firmware updates
                //    switch ((StreamValueType) message.SubType)
                //    {
                //        case StreamValueType.FirmwareConfigRequest:
                //            var fwtype = pullWord(payload, 0);
                //            var fwversion = pullWord(payload, 2);
                //            sendFirmwareConfigResponse(sender, fwtype, fwversion, db, gw);
                //            break;
                //        case StreamValueType.FirmwareConfigResponse:
                //            break;
                //        case StreamValueType.FirmwareRequest:
                //            break;
                //        case StreamValueType.FirmwareResponse:
                //            var fwtype = pullWord(payload, 0);
                //            var fwversion = pullWord(payload, 2);
                //            var fwblock = pullWord(payload, 4);
                //            sendFirmwareResponse(sender, fwtype, fwversion, fwblock, db, gw);
                //            break;
                //        case StreamValueType.Sound:
                //            break;
                //        case StreamValueType.Image:
                //            break;
                //    }
                //    break;
                #endregion
            }

            if (node != null && node.NeedsReboot)
                await Send(new WemosMessage(node.NodeID, -1, WemosMessageType.Internal, (int) WemosInternalMessageType.Reboot));
        }
        #endregion

        #region DB
        private WemosLineValue SaveLineValueToDB(WemosMessage message)
        {
            //WemosLineValue lastSV = GetLastSensorValue(message.NodeNo, message.SensorNo);
            //if (lastSV != null && lastSV.Type == (SensorValueType)message.SubType && lastSV.Value == message.PayloadFloat)
            //    return lastSV;

            var dtDB = DateTime.UtcNow;
            var dt = DateTime.Now;

            WemosLineValue sv = new WemosLineValue()
            {
                NodeID = message.NodeID,
                LineID = message.LineID,
                TimeStamp = dtDB,
                //Type = (SensorValueType) message.SubType,
                Value = message.GetFloat()
            };

            Save(sv);

            sv.TimeStamp = dt;

            return sv;
        }

        private WemosSetting GetSetting(string name)
        {
            //using (var session = Context.OpenSession())
            //    return session.Query<WemosSetting>().FirstOrDefault(setting => setting.Name == name);
            return null;
        }
        private void Save(object item)
        {
            //using (var session = Context.OpenSession())
            //{
            //    try
            //    {
            //        session.Save(item);
            //        session.Flush();
            //    }
            //    catch (Exception) { }
            //}
        }
        private void SaveOrUpdate(object item)
        {
            //using (var session = Context.OpenSession())
            //{
            //    session.SaveOrUpdate(item);
            //    session.Flush();
            //}
        }
        private void Delete(object item)
        {
            //using (var session = Context.OpenSession())
            //{
            //    session.Delete(item);
            //    session.Flush();
            //}
        }
        #endregion
    }
}
