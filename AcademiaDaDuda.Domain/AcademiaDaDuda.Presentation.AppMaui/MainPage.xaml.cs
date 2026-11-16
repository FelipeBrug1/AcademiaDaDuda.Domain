// Felipe Antonio Brüggemann

using AcademiaDaDuda.Presentation.AppMaui.Localization;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDaDuda.Presentation.AppMaui
{
    public partial class MainPage : ContentPage, IRecipient<CulturePreferencesUpdatedMessage>
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register(this);
        }

        public void Receive(CulturePreferencesUpdatedMessage message)
        {
            if (count > 0)
                UpdateCounterText();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count ++;
            UpdateCounterText();
            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void UpdateCounterText()
        {
            var key = count == 1 ? "Clicado {0} vez" : "Clicado {0} vezes";
            CounterBtn.Text = LocalizationManager.Instance.Format(key, count);
        }
    }
}

