using System;
using System.Threading.Tasks;
using LiveScore.Common.LangResources;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LiveScore.Core.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FavoritePopupView : PopupPage
    {
        private const byte timeoutSeconds = 5;

        public FavoritePopupView(string message = null)
        {
            Message = message ?? AppResources.ConnectionLostMessage;
            InitializeComponent();
            InitConnectionLostMessage();
        }

        private void InitConnectionLostMessage()
        {
            BackgroundInputTransparent = true;
            BackgroundColor = Color.Transparent;
            CloseWhenBackgroundIsClicked = true;
            BindingContext = this;
            AutoCloseMessageWhenTimeoutExpired();
        }

        private void AutoCloseMessageWhenTimeoutExpired()
        {
            var self = this;
            Device.StartTimer(TimeSpan.FromSeconds(timeoutSeconds), () =>
            {
                Task.Factory.StartNew(async () =>
                {
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        await PopupNavigation.Instance.RemovePageAsync(self);
                    });
                });

                return false;
            });
        }

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
        public string Message { get; }

        public async void OnTapped(object sender, EventArgs e)
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
            {
                await PopupNavigation.Instance.RemovePageAsync(this);
            }
        }
    }
}