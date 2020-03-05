using System;
using System.Net;
using Com.OneSignal;
using Fanex.Caching;
using FFImageLoading.Helpers;
using LiveScore.Common;
using LiveScore.Common.Helpers;
using LiveScore.Common.Services;
using LiveScore.Core;
using LiveScore.Core.Controls.CustomSearchBar;
using LiveScore.Core.Controls.SearchTeams;
using LiveScore.Core.Services;
using LiveScore.Features.Favorites;
using LiveScore.Features.League;
using LiveScore.Features.Menu;
using LiveScore.Features.News;
using LiveScore.Features.Score;
using LiveScore.Soccer;
using LiveScore.Soccer.Services;
using LiveScore.ViewModels;
using LiveScore.Views;
using MessagePack.Resolvers;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using Microsoft.AspNetCore.SignalR.Client;
using Prism.Ioc;
using Prism.Modularity;
using Refit;
using Rg.Plugins.Popup.Services;
using Sentry;
using Xamarin.Forms;

namespace LiveScore.Configurations
{
    public static class Registration
    {
        private static IConfiguration Configuration;

        public static IContainerRegistry UseContainerInstance(this IContainerRegistry containerRegistry, IContainerProvider container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            containerRegistry.RegisterInstance(container);

            return containerRegistry;
        }

        public static IContainerRegistry UseConfiguration(this IContainerRegistry containerRegistry, IConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            containerRegistry.RegisterInstance(Configuration);

            return containerRegistry;
        }

        public static IContainerRegistry UseSentry(this IContainerRegistry containerRegistry)
        {
            SentrySdk.Init(o =>
            {
                o.Dsn = new Dsn(Configuration.SentryDsn);
                o.Debug = Configuration.Debug;
                o.Environment = Configuration.Environment;
            });

            return containerRegistry;
        }

        public static IContainerRegistry RegisterServices(this IContainerRegistry containerRegistry, IContainerProvider container)
        {
            SetupServicePointManager();

            if (Configuration == null)
            {
                throw new ArgumentNullException($"{nameof(Configuration)} is null. Please call {nameof(UseConfiguration)}");
            }

            AppCenter.Start(Configuration.AppCenterSecret, typeof(Analytics), typeof(Crashes), typeof(Push));
            OneSignal.Current.StartInit(Configuration.OneSignalAppId).EndInit();

            containerRegistry.RegisterSingleton<IDeviceInfo, UserDeviceInfo>();
            containerRegistry.RegisterSingleton<ICacheManager, CacheManager>();
            containerRegistry.RegisterSingleton<ICacheService, CacheService>();
            containerRegistry.RegisterSingleton<INetworkConnection, NetworkConnection>();
            containerRegistry.RegisterSingleton<ILoggingService, LoggingService>();
            containerRegistry.RegisterSingleton<IApiPolicy, ApiPolicy>();
            containerRegistry.RegisterSingleton<IApiService, ApiService>();
            containerRegistry.Register<IHubConnectionBuilder, HubConnectionBuilder>();
            containerRegistry.RegisterSingleton<ISportService, SportService>();
            containerRegistry.RegisterSingleton<ISettings, Settings>();
            containerRegistry.RegisterSingleton<ICryptographyHelper, CryptographyHelper>();
            containerRegistry.RegisterSingleton<IUserSettingService, UserSettingService>();
            containerRegistry.RegisterSingleton<IUserService, UserService>();
            containerRegistry.RegisterSingleton<ISettingsService, SettingsService>();

            containerRegistry.RegisterInstance(new RefitSettings
            {
                ContentSerializer = new MessagePackContentSerializer(container.Resolve<ILoggingService>())
            });

            containerRegistry.RegisterSingleton<IDependencyResolver, DependencyResolver>();

            containerRegistry.RegisterInstance<Func<string, string>>((countryCode)
                 => string.IsNullOrWhiteSpace(countryCode)
                     ? "images/flag_league/default_flag.svg"
                     : $"{Configuration.AssetsEndPoint}flags/{countryCode}.svg",
                FuncNameConstants.BuildFlagUrlFuncName);

            containerRegistry.RegisterInstance<Func<string, string>>((teamAbrv)
               => string.IsNullOrWhiteSpace(teamAbrv)
                   ? "images/logos/logo_2.png"
                   : $"{Configuration.AssetsEndPoint}teamLogos/{teamAbrv}.png",
              FuncNameConstants.BuildTeamLogoUrlFuncName);

            containerRegistry.RegisterInstance<Func<string, string>>((imageName)
                 => $"{Configuration.ImageEndPoint}/{imageName}",
                FuncNameConstants.BuildNewsImageUrlFuncName);

            containerRegistry.RegisterInstance<Action<Action>>(
                Xamarin.Forms.Device.BeginInvokeOnMainThread,
                FuncNameConstants.BeginInvokeOnMainThreadFuncName);

            containerRegistry.RegisterSingleton<IMiniLogger, FFLoadingImageLogger>();

            CompositeResolver.RegisterAndSetAsDefault(
                SoccerModelResolver.Instance,
                CoreModelResolver.Instance,
                BuiltinResolver.Instance,
                PrimitiveObjectResolver.Instance);

            HttpClientManager.SetupHttpClient(Configuration.ApiEndPoint, container, containerRegistry);

            return containerRegistry;
        }

        public static IContainerRegistry RegisterNavigation(this IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(PopupNavigation.Instance);
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<SearchNavigationPage>();
            containerRegistry.RegisterForNavigation<MenuTabbedView, MenuTabbedViewModel>();
            containerRegistry.RegisterForNavigation<MainView, MainViewModel>();
            containerRegistry.RegisterForNavigation<SearchTeamsView, SearchTeamsViewModel>();

            return containerRegistry;
        }

        public static IModuleCatalog AddFeatureModules(this IModuleCatalog moduleCatalog)
        {
            if (moduleCatalog == null)
            {
                throw new ArgumentNullException(nameof(moduleCatalog));
            }

            moduleCatalog.AddModule<ScoreModule>();
            moduleCatalog.AddModule<LeagueModule>();
            moduleCatalog.AddModule<FavoritesModule>();
            moduleCatalog.AddModule<NewsModule>();
            moduleCatalog.AddModule<MenuModule>();

            return moduleCatalog;
        }

        public static IModuleCatalog AddProductModules(this IModuleCatalog moduleCatalog)
        {
            if (moduleCatalog == null)
            {
                throw new ArgumentNullException(nameof(moduleCatalog));
            }

            moduleCatalog.AddModule<SoccerModule>(nameof(SoccerModule));

            return moduleCatalog;
        }

        public static void Verify(this IModuleCatalog moduleCatalog)
        {
            if (!moduleCatalog.Exists(nameof(SoccerModule)))
            {
                throw new InvalidOperationException($"Module {nameof(SoccerModule)} doest not exist");
            }
        }

        private static void SetupServicePointManager()
        {
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
        }
    }
}