using AcademiaDaDuda.Presentation.AppMaui.ViewModels;

namespace AcademiaDaDuda.Presentation.AppMaui.Views;

public partial class MatriculaPage : ContentPage
{
    public MatriculaPage(MatriculaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MatriculaViewModel viewModel)
        {
            await viewModel.InitializeAsync();
            viewModel.RecalcularDataFim();
            FimDatePicker.Date = viewModel.Matricula.DataFim.ToDateTime(new TimeOnly(0, 0));
        }
    }

    private void OnPlanoSelectedIndexChanged(object? sender, EventArgs e) => RecalcularVigencia();

    private void OnInicioDateSelected(object? sender, DateChangedEventArgs e) => RecalcularVigencia();

    private void RecalcularVigencia()
    {
        if (BindingContext is not MatriculaViewModel viewModel)
            return;

        viewModel.RecalcularDataFim();
        FimDatePicker.Date = viewModel.Matricula.DataFim.ToDateTime(new TimeOnly(0, 0));
    }
}
