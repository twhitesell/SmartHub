﻿using Microsoft.ApplicationInsights;
using SmartHub.UWP.Core;
using SmartHub.UWP.Core.Infrastructure;
using System.Collections.Generic;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SmartHub.UWP.Applications.RCHub
{
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            WindowsAppInitializer.InitializeAsync(WindowsCollectors.Metadata | WindowsCollectors.Session);
            InitializeComponent();
            Suspending += App_Suspending;
            UnhandledException += App_UnhandledException;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user. Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            //if (Debugger.IsAttached)
            //    DebugSettings.EnableFrameRateCounter = true;
#endif

            var appView = ApplicationView.GetForCurrentView();
            appView.TitleBar.BackgroundColor = Colors.LightBlue;
            appView.TitleBar.ButtonBackgroundColor = Colors.LightBlue;
            appView.TitleBar.ForegroundColor = Colors.White;
            appView.TitleBar.ButtonForegroundColor = Colors.White;

            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            view.BackRequested += View_BackRequested;

            AppManager.Init();

            var assemblies = new List<Assembly>()
            {
                Assembly.Load(new AssemblyName("SmartHub.UWP.Plugins.Timer")),
                //GetType().GetTypeInfo().Assembly
            };
            //GetType().GetTypeInfo().Assembly.GetReferencedAssemblies()

            HubEnvironment.Init();
            var hub = new Core.Infrastructure.Hub();
            hub.Init(assemblies);
            hub.StartServices();

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content, just ensure that the window is active
            if (rootFrame == null)
            {
                rootFrame = new Frame(); // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (!e.PrelaunchActivated)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private async void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            await Utils.MessageBox("Failed to load Page " + e.SourcePageType.FullName);
            e.Handled = true;
        }
        private void View_BackRequested(object sender, BackRequestedEventArgs e)
        {

        }
        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void App_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            AppManager.UnInit();
            deferral.Complete();
        }
        private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            await Utils.MessageBox(e.Message);
            e.Handled = true;
        }
    }
}
