using System.Diagnostics;
using System.Threading.Tasks;
using LiveScore.Core.Events;
using LiveScore.Core.Models.Notifications;
using LiveScore.Core.PubSubEvents.Notifications;
using MethodTimer;
using Prism.Common;
using Prism.Events;
using Prism.Ioc;
using Xamanimation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LiveScore.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SplashScreen : ContentPage
    {
        private const int MillisecondsDelay = 1000;
        private readonly IEventAggregator eventAggregator;
        private readonly NotificationMessage notificationMessage;

#pragma warning disable S3366 // "this" should not be exposed from constructors

        public SplashScreen(IContainerProvider container, NotificationMessage notificationMessage)
        {
            InitializeComponent();

            this.notificationMessage = notificationMessage;
            eventAggregator = container.Resolve<IEventAggregator>();

            NavigationPage.SetHasNavigationBar(this, false);
        }

#pragma warning restore S3366 // "this" should not be exposed from constructors

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadMainPageAsync();
        }

        [Time]
        private async Task LoadMainPageAsync()
        {
            var mainPage = new MainView { Detail = new MenuTabbedView() };
            await PageUtilities.OnInitializedAsync(mainPage, null);
            Navigation.InsertPageBefore(mainPage, Navigation.NavigationStack[0]);

            await Task.Delay(MillisecondsDelay);

            await Task.WhenAll(SplashIcon.Animate(new ScaleToAnimation { Scale = 0, Duration = "200", Easing = EasingType.Linear }),
                        SplashIcon.Animate(new FadeToAnimation { Opacity = 0, Duration = "200", Easing = EasingType.Linear }));

            await Navigation.PopToRootAsync(false);

            if (notificationMessage != null)
            {
                eventAggregator.GetEvent<NotificationPubSubEvent>().Publish(notificationMessage);
            }

            Debug.WriteLine("Finish LoadMainPageAsync");

            Device.BeginInvokeOnMainThread(() => eventAggregator.GetEvent<FinishInitializationEvent>().Publish());
        }
    }
}