﻿using SQLite.Net.Attributes;
using System;

namespace SmartHub.UWP.Plugins.Things.Models
{
    //https://www.gitbook.com/@louiszl

    public abstract class Device
    {
        [PrimaryKey, NotNull]
        public string ID
        {
            get; set;
        }
        [NotNull]
        public abstract DeviceType Type
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }

        public string IPAddress
        {
            get; set;
        }
        public int Port
        {
            get; set;
        }

        [Ignore]
        public abstract bool IsOnline
        {
            get;
        }

        public event EventHandler IsOnlineChanged;

        public virtual void Ping()
        {
        }
        public virtual void UpdateLines()
        {
        }
    }
}
