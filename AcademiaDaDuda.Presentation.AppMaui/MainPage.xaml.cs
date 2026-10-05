// Felipe Antonio Brüggemann
namespace AcademiaDaDuda.Presentation.AppMaui
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            // Este método é gerado automaticamente pelo XAML. 
            // Não o recrie manualmente.
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            // O CounterBtn é reconhecido automaticamente como 'Button' se
            // estiver declarado como x:Name="CounterBtn" no seu MainPage.xaml
            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}