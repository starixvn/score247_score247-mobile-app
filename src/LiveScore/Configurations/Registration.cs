using System;
using System.Diagnostics;
using System.Net;
using FFImageLoading.Helpers;
using LiveScore.Common;
using LiveScore.Common.Helpers;
using LiveScore.Common.Services;
using LiveScore.Core;
using LiveScore.Core.Controls.SearchPage;
using LiveScore.Core.Services;
using LiveScore.Core.ViewModels;
using LiveScore.Core.Views;
using LiveScore.Features.Favorites;
using LiveScore.Features.League;
using LiveScore.Features.Menu;
using LiveScore.Features.News;
using LiveScore.Features.Score;
using LiveScore.Features.TVSchedule;
using LiveScore.Soccer;
using LiveScore.Soccer.Services;
using LiveScore.ViewModels;
using LiveScore.Views;
using MessagePack.Resolvers;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AspNetCore.SignalR.Client;
using Prism.Ioc;
using Prism.Modularity;
using Refit;
using Sentry;
using Xamarin.Forms;

namespace LiveScore.Configurations
{
    public static class Registration
    {
        private static IConfiguration Configuration;

        public static IContainerRegistry UseContainerInstance(this IContainerRegistry containerRegistry, IContainerProvider container)
        {
            Debug.WriteLine($"UseContainerInstance {DateTime.Now.ToString("hh:mm:ss")}");
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

        public static IContainerRegistry RegisterServices(this IContainerRegistry containerRegistry)
        {
            SetupServicePointManager();

            if (Configuration == null)
            {
                throw new ArgumentNullException($"{nameof(Configuration)} is null. Please call {nameof(UseConfiguration)}");
            }

            AppCenter.Start(Configuration.AppCenterSecret, typeof(Analytics), typeof(Crashes));

            containerRegistry.RegisterSingleton<IDeviceInfo, UserDeviceInfo>();

            containerRegistry.RegisterSingleton<INetworkConnection, NetworkConnection>();

            containerRegistry.RegisterSingleton<IApiPolicy, ApiPolicy>();
            containerRegistry.RegisterSingleton<IApiService, ApiService>();

            containerRegistry.Register<IHubConnectionBuilder, HubConnectionBuilder>();

            containerRegistry.RegisterSingleton<ISportService, SportService>();
            containerRegistry.RegisterSingleton<IMatchService, MatchService>();
            containerRegistry.RegisterSingleton<ISettings, Settings>();

            containerRegistry.RegisterInstance(new RefitSettings
            {
                ContentSerializer = new MessagePackContentSerializer()
            });

            containerRegistry.RegisterSingleton<IDependencyResolver, DependencyResolver>();

            containerRegistry.RegisterInstance<Func<string, string>>((countryCode)
                 => string.IsNullOrWhiteSpace(countryCode)
                     ? "images/flag_league/default_flag.svg"
                     : $"{Configuration.AssetsEndPoint}flags/{countryCode}.svg",
                FuncNameConstants.BuildFlagUrlFuncName);

            containerRegistry.RegisterInstance<Action<Action>>(
                Xamarin.Forms.Device.BeginInvokeOnMainThread,
                FuncNameConstants.BeginInvokeOnMainThreadFuncName);

            CompositeResolver.RegisterAndSetAsDefault(
                SoccerModelResolver.Instance,
                CoreModelResolver.Instance,
                BuiltinResolver.Instance,
                PrimitiveObjectResolver.Instance);

            return containerRegistry;
        }

        public static IContainerRegistry RegisterNavigation(this IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<SearchNavigationPage>();
            containerRegistry.RegisterForNavigation<MenuTabbedView, MenuTabbedViewModel>();
            containerRegistry.RegisterForNavigation<MainView, MainViewModel>();
            containerRegistry.RegisterForNavigation<SelectSportView, SelectSportViewModel>();
            containerRegistry.RegisterForNavigation<SearchView, SearchViewModel>();

            return containerRegistry;
        }

        public static IModuleCatalog AddFeatureModules(this IModuleCatalog moduleCatalog)
        {
            if (moduleCatalog == null)
            {
                throw new ArgumentNullException(nameof(moduleCatalog));
            }

            moduleCatalog.AddModule<ScoreModule>();
            moduleCatalog.AddModule<LeagueModule>(InitializationMode.OnDemand);
            moduleCatalog.AddModule<FavoritesModule>(InitializationMode.OnDemand);
            moduleCatalog.AddModule<NewsModule>(InitializationMode.OnDemand);
            moduleCatalog.AddModule<TVScheduleModule>(InitializationMode.OnDemand);
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