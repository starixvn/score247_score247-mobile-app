using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using FFImageLoading.Forms.Platform;
using FFImageLoading.Svg.Forms;
using Foundation;
using LiveScore.Common.Helpers;
using LiveScore.Common.Services;
using LiveScore.Core.Events;
using LiveScore.Core.Models.Notifications;
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
        private ILoggingService loggingService;
        private IEventAggregator eventAggregator;

        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            Profiler.Start("IOS Application");
            Rg.Plugins.Popup.Popup.Init();
            Xamarin.Forms.Forms.Init();
            CachedImageRenderer.Init();
            SvgCachedImage.Init();
            CardsViewRenderer.Preserve();
            XamEffects.iOS.Effects.Init();

            var notificationMessage = BuildNotificationMessage(launchOptions);
            var application = new App(new iOSInitializer(), notificationMessage);

            loggingService = application.Container.Resolve<ILoggingService>();
            eventAggregator = application.Container.Resolve<IEventAggregator>();

            LoadApplication(application);
            SubscribeEvents();
            HandleGlobalExceptions();

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        private static NotificationMessage BuildNotificationMessage(NSDictionary launchOptions)
        {
            NotificationMessage notificationMessage = null;

            if (launchOptions?.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey) == true)
            {
                var customNotificationData =
                    (launchOptions.ObjectForKey(UIApplication.LaunchOptionsRemoteNotificationKey) as NSDictionary)?
                        .ObjectForKey(new NSString("mobile_center")) as NSDictionary;

                notificationMessage = new NotificationMessage(
                     customNotificationData?.ValueForKey(new NSString("SportId")).ToString(),
                     customNotificationData?.ValueForKey(new NSString("id")).ToString(),
                     customNotificationData?.ValueForKey(new NSString("type")).ToString(),
                     customNotificationData?.ValueForKey(new NSString("EventDate")).ToString());
            }

            return notificationMessage;
        }

        private void SubscribeEvents()
        {
            eventAggregator.GetEvent<StartLoadDataEvent>().Subscribe(() =>
            {
                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
            });

            eventAggregator.GetEvent<StopLoadDataEvent>().Subscribe(() => UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false);
        }

        private void HandleGlobalExceptions()
        {
            Runtime.MarshalManagedException += HandleMarshalException;
            Runtime.MarshalObjectiveCException += HandleMarshalObjectCException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            TaskScheduler.UnobservedTaskException += UnobservedTaskException;
        }

        private void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            loggingService.LogException(e.Exception);
        }

        private void HandleMarshalException(object sender, MarshalManagedExceptionEventArgs args)
        {
            loggingService.LogException(args.Exception);
        }

        private void HandleMarshalObjectCException(object sender, MarshalObjectiveCExceptionEventArgs args)
        {
            loggingService.LogException(new InvalidOperationException($"Marshaling Objective-C exception. {args.Exception.DebugDescription}"));
        }

        private void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            loggingService.LogException(e.Exception);
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            loggingService.LogException(e.ExceptionObject as Exception);
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