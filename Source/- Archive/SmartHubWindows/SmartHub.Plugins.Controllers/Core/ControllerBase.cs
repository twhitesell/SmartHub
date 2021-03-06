﻿using SmartHub.Core.Plugins;
using SmartHub.Plugins.Controllers.Data;
using SmartHub.Plugins.MySensors;
using SmartHub.Plugins.MySensors.Core;
using System;

namespace SmartHub.Plugins.Controllers.Core
{
    public abstract class ControllerBase
    {
        #region Fields
        protected Controller controller;
        protected MySensorsPlugin mySensors;
        protected IServiceContext Context;
        protected float? lastSensorValue;
        #endregion

        #region Constructor
        public ControllerBase(Controller controller)
        {
            this.controller = controller;
        }
        #endregion

        #region Properties
        public Guid ID
        {
            get { return controller.Id; }
        }
        public string Name
        {
            get { return controller.Name; }
            set
            {
                if (controller.Name != value)
                {
                    controller.Name = value;
                    SaveToDB();
                }
            }
        }
        public bool IsAutoMode
        {
            get { return controller.IsAutoMode; }
            set
            {
                if (controller.IsAutoMode != value)
                {
                    controller.IsAutoMode = value;
                    SaveToDB();
                }
            }
        }
        #endregion

        #region Public methods
        public void Init(IServiceContext context)
        {
            Context = context;
            mySensors = context.GetPlugin<MySensorsPlugin>();
            InitLastValues();
        }
        public void SaveToDB()
        {
            using (var session = Context.OpenSession())
            {
                session.SaveOrUpdate(controller);
                session.Flush();
            }
        }

        public abstract void SetConfiguration(string configuration);
        public abstract bool IsMyMessage(SensorMessage message);
        public abstract void RequestSensorsValues();
        #endregion

        #region Private methods
        protected virtual void InitLastValues()
        {
        }
        abstract protected void Process();
        #endregion

        #region Event handlers
        public virtual void MessageCalibration(SensorMessage message)
        {
        }
        public virtual void MessageReceived(SensorMessage message)
        {
            Process();
        }
        public void TimerElapsed(DateTime now)
        {
            Process();
        }
        #endregion
    }
}
