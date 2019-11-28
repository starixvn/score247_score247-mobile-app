using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using FFImageLoading.Forms.Platform;
using FFImageLoading.Svg.Forms;
using Foundation;
using LiveScore.Common.Helpers;
using LiveScore.Common.Services;
using LiveScore.Core.Events;
using ObjCRuntime;
using PanCardView.iOS;
using Prism;
using Prism.Events;
using Prism.Ioc;
using UIKit;
using Runtime = ObjCRuntime.Runtime;

namespace LiveScore.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            Profiler.Start("IOS Application");
            Rg.Plugins.Popup.Popup.Init();
            Xamarin.Forms.Forms.Init();
            CachedImageRenderer.Init();
            SvgCachedImage.Init();
            CardsViewRenderer.Preserve();
            XamEffects.iOS.Effects.Init();

            var application = new App(new iOSInitializer());
            LoadApplication(application);
            SubscribeEvents(application);
            HandleGlobalExceptions(application);

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        private static void SubscribeEvents(App application)
        {
            var eventAggregator = application.Container.Resolve<IEventAggregator>();

            eventAggregator.GetEvent<StartLoadDataEvent>().Subscribe(() =>
            {
                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
            });

            eventAggregator.GetEvent<StopLoadDataEvent>().Subscribe(() => UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false);
        }

        private void HandleGlobalExceptions(App application)
        {
            Runtime.MarshalManagedException += HandleMarshalException;
            Runtime.MarshalObjectiveCException += HandleMarshalObjectCException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            TaskScheduler.UnobservedTaskException += UnobservedTaskException;
        }

        private void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
        }

        private void HandleMarshalException(object sender, MarshalManagedExceptionEventArgs args)
        {
        }

        private void HandleMarshalObjectCException(object sender, MarshalObjectiveCExceptionEventArgs args)
        {
        }

        private void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
        }
    }

#pragma warning disable S101 // Types should be named in PascalCase

    public class iOSInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register any platform specific implementations
        }
    }

#pragma warning restore S101 // Types should be named in PascalCase
}