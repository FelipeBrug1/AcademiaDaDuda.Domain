using AcademiaDaDuda.Presentation.AppMaui.ViewModels;

namespace AcademiaDaDuda.Presentation.AppMaui.Views;

[QueryProperty(nameof(LogradouroId), "Id")]
public partial class LogradouroPage : ContentPage
{
    public int LogradouroId { get; set; }

    public LogradouroPage(LogradouroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LogradouroViewModel viewModel)
        {
            viewModel.LogradouroId = LogradouroId;
            await viewModel.InitializeAsync();
        }
    }
}
