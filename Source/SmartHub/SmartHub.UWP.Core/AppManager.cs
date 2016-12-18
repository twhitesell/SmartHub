﻿using SmartHub.UWP.Core.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;

namespace SmartHub.UWP.Core
{
    public class AppManager
    {
        #region Fields
        private static Hub hub = new Hub();
        #endregion

        #region Properties
        //public static string AppId
        //{
        //    get { return "9b1c8c98-9041-497b-9c5b-40002ad629c8"; }
        //}
        public static string AppName
        {
            get { return Package.Current.DisplayName; }
        }
        public static string AppVersion
        {
            get
            {
                var version = Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}";
                //return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }
        public static string AppEmail
        {
            get { return "gothicmaestro@live.com"; }
        }
        //public static string AppHomePage
        //{
        //    get { return "http://www.smarthub.at.ua"; }
        //}
        public static AppData AppData
        {
            get; private set;
        }
        public static Hub Hub => hub;
        #endregion

        #region Public methods
        public static void Init()
        {
            AppData = new AppData(false);
            AppData.PropertyChanged += AppData_PropertyChanged;

            var assemblies = Utils.GetSatelliteAssemblies(file => file.FileType == ".dll" && file.DisplayName.StartsWith("SmartHub"));
            hub.Init(assemblies);
            hub.StartServices();

            SetLanguage(AppData.Language);
            //SetLanguage("en");
            //SetLanguage("de");
            //SetLanguage("ru");
            //SetLanguage("uk");
        }
        public static void OnSuspending(SuspendingDeferral defferal)
        {
            //hub.StopServices();
        }
        #endregion

        #region Private methods
        private static void SetLanguage(string id) //"en-US"
        {
            ApplicationLanguages.PrimaryLanguageOverride = id;
            ResourceContext.GetForCurrentView().Languages = new List<string>() { id };
        }
        #endregion

        #region Event handlers
        private static void AppData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AppData.Language):
                    SetLanguage(AppData.Language);
                    break;
            }
        }
        #endregion
    }
}
