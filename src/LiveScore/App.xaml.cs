using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Com.OneSignal;
using Com.OneSignal.Abstractions;
using FFImageLoading;
using FFImageLoading.Helpers;
using LiveScore.Common;
using LiveScore.Common.LangResources;
using LiveScore.Common.Services;
using LiveScore.Configurations;
using LiveScore.Core.Enumerations;
using LiveScore.Core.Events;
using LiveScore.Core.Models.Notifications;
using LiveScore.Core.PubSubEvents.Notifications;
using LiveScore.Core.Services;
using LiveScore.Views;
using Plugin.Multilingual;
using Prism;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace LiveScore
{
    public partial class App : PrismApplication
    {
        /*
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor.
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */      
        private INetworkConnection networkConnectionManager;
        private IHubService soccerHub;
        private DateTime appSleepTime;
        private ILoggingService loggingService;
        private bool isResumeWithNotification;

        public App() : this(null, null)
        {
        }

        public App(IPlatformInitializer initializer, NotificationMessage notificationMessage) : base(initializer)
        {
            SetMainPage(notificationMessage);
        }

        protected override void OnInitialized()
        {
            InitializeComponent();

            AppResources.Culture = CrossMultilingual.Current.DeviceCultureInfo;
            ImageService
                .Instance
                .Initialize(new FFImageLoading.Config.Configuration { Logger = Container.Resolve<IMiniLogger>() });

            StartEventHubs();
            StartGlobalTimer();

            networkConnectionManager = Container.Resolve<INetworkConnection>();
            networkConnectionManager.StartListen();
            loggingService = Container.Resolve<ILoggingService>();
            var configuration = Container.Resolve<IConfiguration>();

            OneSignal.Current
                .StartInit(configuration.OneSignalAppId)
                .InFocusDisplaying(OSInFocusDisplayOption.Notification)
                .HandleNotificationOpened((result) =>
                {
                    Debug.WriteLine("HandleNotificationOpened");

                    var notificationMessage = new NotificationMessage(
                            result.notification.payload.additionalData["SportId"].ToString(),
                            result.notification.payload.additionalData["id"].ToString(),
                            result.notification.payload.additionalData["type"].ToString(),
                            result.notification.payload.title,
                            result.notification.payload.body);


                    HandlePushNotification(notificationMessage);
                })
                .EndInit();
            
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                var viewName = viewType.FullName?.Replace(".ViewModels.", ".Views.");
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var viewModelName = $"{viewName?.Replace("View", string.Empty)}ViewModel, {viewAssemblyName}";

                return Type.GetType(viewModelName);
            });
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
            => containerRegistry
                .UseContainerInstance(Container)
                .UseConfiguration(new Configuration())
                .UseSentry()
                .RegisterServices(Container)
                .RegisterNavigation();

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
            => moduleCatalog
                .AddProductModules()
                .AddFeatureModules()
                .Verify();

        private void StartEventHubs()
        {
            soccerHub = Container.Resolve<IHubService>(SportType.Soccer.Value.ToString());

            soccerHub.Start();
        }

        protected override async void OnSleep()
        {
            try
            {
                base.OnSleep();

                appSleepTime = DateTime.Now;

                await soccerHub.Stop();
            }
            catch (Exception ex)
            {
                await loggingService.LogExceptionAsync(ex, $"OnSleep: {ex.Message}");
            }
        }

        protected override async void OnResume()
        {
            try
            {
                base.OnResume();

                if (soccerHub == null)
                {
                    soccerHub = Container.Resolve<IHubService>(SportType.Soccer.Value.ToString());
                }

                await Task.Run(async () => await soccerHub.ConnectWithRetry());

                if (NeedToRestartApp() && !isResumeWithNotification)
                {
                    SetMainPage();
                }

                isResumeWithNotification = false;
            }
            catch (Exception ex)
            {
                await loggingService.LogExceptionAsync(ex, $"OnResume: {ex.Message}");
            }
        }

        private void SetMainPage(NotificationMessage notificationMessage = null)
        {
            MainPage = new NavigationPage(new SplashScreen(Container, notificationMessage));
        }

        private void StartGlobalTimer(int intervalMinutes = 1)
        {
            Device.StartTimer(TimeSpan.FromMinutes(intervalMinutes), () =>
            {
                if (networkConnectionManager.IsSuccessfulConnection())
                {
                    var eventAggregator = Container.Resolve<IEventAggregator>();
                    eventAggregator.GetEvent<OneMinuteTimerCountUpEvent>().Publish();
                }

                return true;
            });
        }

        private void HandlePushNotification(NotificationMessage notificationMessage)
        {
            var eventAggregator = Container.Resolve<IEventAggregator>();

            isResumeWithNotification = true;

            if (NeedToRestartApp())
            {
                SetMainPage(notificationMessage);
            }
            else
            {
                eventAggregator.GetEvent<NotificationPubSubEvent>().Publish(notificationMessage);
            }
        }

        private bool NeedToRestartApp() => appSleepTime != default && DateTime.Now - appSleepTime > TimeSpan.FromMinutes(30);
    }
}