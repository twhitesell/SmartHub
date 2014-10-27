﻿using MySensors.Core.Nodes;
using SQLite;
using System;

namespace MySensors.Core.Services.Data
{
    [Table("SensorValues")]
    class SensorValueDto
    {
        //[ForeignKey(typeof(NodeDto))]
        public byte NodeID { get; set; }
        //[ForeignKey(typeof(SensorDto))]
        public byte ID { get; set; }
        public DateTime Time { get; set; }
        public byte Type { get; set; }
        public float Value { get; set; }

        public static SensorValueDto FromModel(SensorValue sv)
        {
            if (sv == null)
                return null;

            return new SensorValueDto()
            {
                NodeID = sv.NodeID,
                ID = sv.ID,
                Time = sv.Time,
                Type = (byte)sv.Type,
                Value = sv.Value
            };
        }
        public SensorValue ToModel()
        {
            return new SensorValue(NodeID, ID, Time, (SensorValueType)Type, Value);
        }
    }
}