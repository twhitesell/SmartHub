﻿using SQLite.Net.Attributes;
using System;

namespace SmartHub.UWP.Plugins.Wemos.Infrastructure.Monitors.Models
{
    public class WemosLineMonitor
    {
        [PrimaryKey, NotNull]
        public string ID
        {
            get; set;
        } = Guid.NewGuid().ToString();
        [NotNull]
        public string LineID
        {
            get; set;
        }

        // value = value * factor + Offset
        [NotNull]
        public float Factor
        {
            get; set;
        } = 1;
        [NotNull, Default()]
        public float Offset
        {
            get; set;
        } = 0;
        public string Units
        {
            get; set;
        }

        [NotNull]
        public float Min
        {
            get; set;
        }
        [NotNull]
        public float Max
        {
            get; set;
        }
        [NotNull]
        public int ValuesCount
        {
            get; set;
        } = 10;

        [NotNull, Default()]
        public int Precision
        {
            get; set;
        } = 0;
    }
}