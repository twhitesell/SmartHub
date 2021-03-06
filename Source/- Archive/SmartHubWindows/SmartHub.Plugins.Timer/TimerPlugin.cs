﻿using Microsoft.Win32;
using SmartHub.Core.Plugins;
using SmartHub.Plugins.Timer.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Timers;

namespace SmartHub.Plugins.Timer
{
    [Plugin]
    public class TimerPlugin : PluginBase
    {
        #region Fields
        private const int TIMER_INTERVAL = 10000;
        private System.Timers.Timer timer;
        private readonly List<PeriodicalActionState> periodicalHandlers = new List<PeriodicalActionState>();
        #endregion

        #region Import
        [ImportMany(Timer_10sec_ElapsedAttribute.CLSID)]
        public Action<DateTime>[] Timer_ElapsedEventHandlers { get; set; }

        [ImportMany(RunPeriodicallyAttribute.CLSID)]
        public Lazy<Action<DateTime>, IRunPeriodicallyAttribute>[] PeriodicalActions { get; set; }
        #endregion

        #region Plugin ovverrides
        public override void InitPlugin()
        {
            CultureInfo.CurrentCulture.ClearCachedData();
            SystemEvents.TimeChanged += (sender, args) => CultureInfo.CurrentCulture.ClearCachedData();

            timer = new System.Timers.Timer(TIMER_INTERVAL);
            timer.Elapsed += timer_Elapsed;

            RegisterPeriodicalHandlers();
        }
        public override void StartPlugin()
        {
            timer.Enabled = true;
        }
        public override void StopPlugin()
        {
            timer.Enabled = false;
        }
        #endregion

        #region Event handlers
        private void timer_Elapsed(object source, ElapsedEventArgs e)
        {
            //timer.Enabled = false;

            var now = DateTime.Now;

            // periodical actions
            foreach (var handler in periodicalHandlers)
                handler.TryToExecute(now);

            Run(Timer_ElapsedEventHandlers, handler => handler(now));

            //timer.Enabled = true;
        }
        #endregion

        #region Private methods
        private void RegisterPeriodicalHandlers()
        {
            var now = DateTime.Now;

            Logger.Info("Register periodical actions at {0:yyyy.MM.dd, HH:mm:ss}", now);

            foreach (var action in PeriodicalActions)
                periodicalHandlers.Add(new PeriodicalActionState(action.Value, action.Metadata.Interval, now, Logger));
        }
        #endregion
    }
}
