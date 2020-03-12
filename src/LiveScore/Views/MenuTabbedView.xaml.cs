using LiveScore.Common.Services;
using LiveScore.Core.Models.Notifications;
using LiveScore.Core.PubSubEvents.Notifications;
using LiveScore.Core.ViewModels;
using LiveScore.Soccer.ViewModels.Matches;
using LiveScore.Soccer.Views.Matches;
using LiveScore.ViewModels;
using MethodTimer;
using Prism;
using Prism.Navigation;
using Rg.Plugins.Popup.Contracts;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace LiveScore.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuTabbedView : Xamarin.Forms.TabbedPage
    {
        public MenuTabbedView()
        {
            MenuTabbedView page = this;
            NavigationPage.SetHasNavigationBar(page, false);

            InitializeComponent();
            if (Device.RuntimePlatform == Device.iOS)
            {
                SetUpBarTextColor();
            }
            On<Android>().SetIsSwipePagingEnabled(false);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var viewModel = BindingContext as MenuTabbedViewModel;
            viewModel.EventAggregator.GetEvent<NotificationPubSubEvent>().Subscribe(OnReceivedNotification);
        }

        [Time]
        private async void OnReceivedNotification(NotificationMessage message)
        {
            if (BindingContext is MenuTabbedViewModel viewModel)
            {
                var networkConnection = viewModel.DependencyResolver.Resolve<INetworkConnection>();

                if (networkConnection.IsFailureConnection())
                {
                    return;
                }

                var page = BuildNotificationPage(message, viewModel);
                await CurrentPage.Navigation.PushAsync(page);
            }
        }

        // TODO: Refactor this method later, move to factory class
        [Time]
        private static Page BuildNotificationPage(NotificationMessage message, ViewModelBase viewModel)
        {
            var page = new Page();

            if (message.SportType.IsSoccer() && message.Type.IsMatchType())
            {
                page = new MatchDetailView();
                var matchDetailViewModel = new MatchDetailViewModel(
                        viewModel.NavigationService,
                        viewModel.DependencyResolver,
                        viewModel.EventAggregator,
                        viewModel.DependencyResolver.Resolve<IPopupNavigation>());

                var parameters = new NavigationParameters
                {
                    { "Id", message.Id },
                    { "EventDate", message.EventDate }
                };

                page.BindingContext = matchDetailViewModel;
                matchDetailViewModel.Initialize(parameters);
            }

            return page;
        }

        private void SetUpBarTextColor()
        {
            var color = PrismApplicationBase.Current.Resources["FunctionBarActiveColor"];

            if (color != null)
            {
                BarTextColor = (Color)color;
            }
        }
    }
}