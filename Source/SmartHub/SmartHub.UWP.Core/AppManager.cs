﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;

namespace SmartHub.UWP.Core
{
    public class AppManager
    {
        #region Fields
        //private static Hub hub = new Hub();
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
                return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
                //return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }
        public static string AppEmail
        {
            get { return "gothicmaestro@live.com"; }
        }
        public static string AppHomePage
        {
            get { return "http://www.wintx.at.ua"; }
        }

        public static AppData AppData
        {
            get; private set;
        }
        //public static ModelsManager ModelsManager
        //{
        //    get; private set;
        //}
        #endregion

        #region Public methods
        public static void Init()
        {
            AppData = new AppData(true);
            AppData.PropertyChanged += AppData_PropertyChanged;

            //var ds = AppData.Device;
            //ds.DateTime = new DateTime(2016, 5, 10);
            //AppData.Device = ds;

            //ModelsManager = new ModelsManager();
            //ModelsManager.Items = AppData.Profiles;
            //ModelsManager.Items.CollectionChanged += (s, args) => { SaveProfiles(); };

            //SetLanguage(AppData.Device.Language);
            //SetLanguage("en");
            //SetLanguage("de");
            //SetLanguage("ru");
            //SetLanguage("uk");

            //HubEnvironment.Init();
            //hub.Init();
            //hub.StartServices();
        }
        public static void UnInit()
        {
            //hub.StopServices();
        }

        //public static void SaveModels()
        //{
        //    AppData.Models = ModelsManager.Items;
        //}
        public static void SetLanguage(string id) //"en-US"
        {
            ApplicationLanguages.PrimaryLanguageOverride = id;

            var context = ResourceContext.GetForCurrentView();
            //context.Reset();
            //context.QualifierValues["Language"] = id;

            var lang = new List<string>();
            lang.Add(id);
            context.Languages = lang;

            //LabelsManager.RefreshResources();

            //var frame = Window.Current.Content as Frame;
            //(frame.Content as BasePage).NavigationHelper.Reload();
        }
        #endregion

        #region Event handlers
        private static void AppData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //switch (e.PropertyName)
            //{
            //    case AppData.LanguageKey:
            //        SetLanguage(AppData.Language);
            //        break;
            //}
        }
        #endregion
    }
}