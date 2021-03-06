﻿using SmartHub.UWP.Core.Plugins;
using SmartHub.UWP.Plugins.Things;
using SmartHub.UWP.Plugins.Things.Models;
using SmartHub.UWP.Plugins.Speech;
using SmartHub.UWP.Plugins.Wemos.Core.Models;
using SmartHub.UWP.Plugins.Wemos.Infrastructure.Controllers.Models;
using System;

namespace SmartHub.UWP.Plugins.Wemos.Infrastructure.Controllers
{
    public class WemosControllerWorkerHeater : WemosControllerWorker
    {
        public class ControllerConfiguration
        {
            public string LineTemperatureID
            {
                get; set;
            }
            public string LineSwitchID
            {
                get; set;
            }

            public float TemperatureMin { get; set; } = 25.0f;
            public float TemperatureMax { get; set; } = 26.0f;
            public float TemperatureAlarmMin { get; set; } = 20.0f;
            public float TemperatureAlarmMax { get; set; } = 30.0f;
            public string TemperatureAlarmMinText { get; set; } = "Extremely low temperature";
            public string TemperatureAlarmMaxText { get; set; } = "Extremely high temperature";
        }

        #region Fields
        protected float? lastLineValue;
        #endregion

        #region Properties
        private WemosLine LineTemperature
        {
            get { return host.GetLine((Configuration as ControllerConfiguration).LineTemperatureID); }
        }
        private WemosLine LineSwitch
        {
            get { return host.GetLine((Configuration as ControllerConfiguration).LineSwitchID); }
        }
        #endregion

        #region Constructor
        internal WemosControllerWorkerHeater(WemosController ctrl, IServiceContext context)
            : base (ctrl, context)
        {
        }
        #endregion

        #region Abstract Overrides
        protected override Type GetConfigurationType() => typeof(ControllerConfiguration);
        protected override object GetDefaultConfiguration() => new ControllerConfiguration();

        protected override bool IsMyMessage(LineValue value)
        {
            return
                ThingsPlugin.IsValueFromLine(value, LineSwitch.ID) ||
                ThingsPlugin.IsValueFromLine(value, LineTemperature.ID);
        }
        protected async override void RequestLinesValues()
        {
            await host.RequestLineValueAsync(LineSwitch);
            await host.RequestLineValueAsync(LineTemperature);
        }
        protected override void Preprocess(LineValue value)
        {
            if (ThingsPlugin.IsValueFromLine(value, LineTemperature.ID))
                lastLineValue = value.Value;
        }
        protected async override void DoWork(DateTime now)
        {
            if (lastLineValue.HasValue)
            {
                float value = lastLineValue.Value;
                var config = Configuration as ControllerConfiguration;

                if (value < config.TemperatureMin)
                    await host.SetLineValueAsync(LineSwitch, 1);
                else if (value > config.TemperatureMax)
                    await host.SetLineValueAsync(LineSwitch, 0);

                // voice alarm:
                if (value <= config.TemperatureAlarmMin)
                    context.GetPlugin<SpeechPlugin>().Say($"{config.TemperatureAlarmMinText}, {value}");
                else if (value >= config.TemperatureAlarmMax)
                    context.GetPlugin<SpeechPlugin>().Say($"{config.TemperatureAlarmMaxText}, {value}");
            }
            else
                RequestLinesValues();
        }
        #endregion
    }
}
