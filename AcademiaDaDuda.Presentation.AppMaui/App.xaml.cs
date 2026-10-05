// Felipe Antonio Brüggemann 

using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDaDuda.Presentation.AppMaui;

public partial class App : IApplication
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}