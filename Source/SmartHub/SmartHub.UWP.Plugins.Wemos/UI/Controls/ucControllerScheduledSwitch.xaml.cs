﻿using Newtonsoft.Json;
using SmartHub.UWP.Core;
using SmartHub.UWP.Plugins.Wemos.Controllers;
using SmartHub.UWP.Plugins.Wemos.Controllers.Models;
using SmartHub.UWP.Plugins.Wemos.Core.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Linq;
using System;
using SmartHub.UWP.Core.StringResources;

namespace SmartHub.UWP.Plugins.Wemos.UI.Controls
{
    public sealed partial class ucControllerScheduledSwitch : UserControl
    {
        #region Fields
        private WemosScheduledSwitchController.ControllerConfiguration configuration = null;
        #endregion

        #region Properties
        public static readonly DependencyProperty ControllerProperty = DependencyProperty.Register("Controller", typeof(WemosControllerObservable), typeof(ucControllerScheduledSwitch), new PropertyMetadata(null, new PropertyChangedCallback(OnControllerChanged)));
        public WemosControllerObservable Controller
        {
            get { return (WemosControllerObservable) GetValue(ControllerProperty); }
            set { SetValue(ControllerProperty, value); }
        }
        private static void OnControllerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public ObservableCollection<WemosLine> Lines
        {
            get;
        } = new ObservableCollection<WemosLine>();
        #endregion

        public ucControllerScheduledSwitch(WemosControllerObservable ctrl)
        {
            InitializeComponent();
            Utils.FindFirstVisualChild<Grid>(this).DataContext = this;

            Controller = ctrl;

            if (string.IsNullOrEmpty(Controller.Configuration))
            {
                configuration = new WemosScheduledSwitchController.ControllerConfiguration();
                Controller.Configuration = JsonConvert.SerializeObject(configuration);
            }
            else
                configuration = JsonConvert.DeserializeObject<WemosScheduledSwitchController.ControllerConfiguration>(Controller.Configuration);
        }

        private async Task UpdateLinesList()
        {
            Lines.Clear();

            var models = (await Utils.RequestAsync<List<WemosLine>>("/api/wemos/lines")).Where(m => m.Type == WemosLineType.Switch);
            foreach (var model in models)
                Lines.Add(model);

            cbLines.SelectedItem = Lines.FirstOrDefault(l => l.ID == configuration.LineSwitchID);
        }
        private void UpdatePeriodsList()
        {
            //configuration.ActivePeriods.Add(new Period
            //{
            //    From = new TimeSpan(11, 0, 0),
            //    To = new TimeSpan(15, 0, 0),
            //    IsEnabled = true
            //});
            //configuration.ActivePeriods.Add(new Period
            //{
            //    From = new TimeSpan(17, 0, 0),
            //    To = new TimeSpan(23, 0, 0),
            //    IsEnabled = true
            //});

            lvPeriods.ItemsSource = configuration.ActivePeriods;
        }

        #region Event handlers
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateLinesList();
            UpdatePeriodsList();
        }
        private void cbLines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var line = cbLines.SelectedItem as WemosLine;
            if (line != null && line.ID != configuration.LineSwitchID)
            {
                configuration.LineSwitchID = line.ID;
                Controller.Configuration = JsonConvert.SerializeObject(configuration);
            }
        }
        private async void btnDeletePeriod_Click(object sender, RoutedEventArgs e)
        {
            var period = (Period) ((sender as Button).Tag);

            await Utils.MessageBoxYesNo(Labels.confirmDeleteItem, (onYes) =>
            {
                configuration.ActivePeriods.Remove(period);
                Controller.Configuration = JsonConvert.SerializeObject(configuration);
            });
        }
        #endregion
    }
}
