﻿using SmartHub.UWP.Plugins.Wemos.Core;
using SQLite.Net.Attributes;

namespace SmartHub.UWP.Plugins.Wemos.Models
{
    public class WemosNode
    {
        [PrimaryKey, NotNull]
        public int NodeID { get; set; }
        public string Name { get; set; }
        [NotNull]
        public WemosLineType Type { get; set; }
        public float ProtocolVersion { get; set; }
        public string FirmwareName { get; set; }
        public float FirmwareVersion { get; set; }
        public bool NeedsReboot { get; set; }
    }
}
