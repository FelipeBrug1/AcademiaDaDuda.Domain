// Felipe AntonioBrüggemann

using AcademiaDaDuda.Presentation.AppMaui.Views;

namespace AcademiaDaDuda.Presentation.AppMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("logradouro", typeof(LogradouroPage));
    }
}