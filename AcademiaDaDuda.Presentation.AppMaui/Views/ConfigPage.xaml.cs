// Felipe Antonio Brüggemann

using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using AcademiaDaDuda.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDaDuda.Presentation.AppMaui.Views;

public partial class ConfigPage : ContentPage
{
    public ConfigPage()
    {
        InitializeComponent();
        CarregarTema();
        CarregarBanco();
    }

    #region Tema
    private void CarregarTema()
    {
        TemaPicker.SelectedIndex = Preferences.Get("Tema", "system") switch
        {
            "light" => 0,
            "dark" => 1,
            _ => 2
        };
    }

    private async void OnSalvarTemaClicked(object sender, EventArgs e)
    {
        string selectedTheme = TemaPicker.SelectedIndex switch
        {
            0 => "light",
            1 => "dark",
            _ => "system"
        };

        Preferences.Set("Tema", selectedTheme);
        WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage("TemaAlterado"));
        await DisplayAlertAsync("Sucesso", "Tema salvo com sucesso!", "OK");
        await Shell.Current.GoToAsync("//dashboard");
    }
    #endregion

    #region Banco de Dados
    private void CarregarBanco()
    {
        DatabaseTypePicker.Items.Clear();
        DatabaseTypePicker.Items.Add(AppDatabaseType.Sqlite.ToString());
        DatabaseTypePicker.SelectedItem = AppDatabaseType.Sqlite.ToString();

        AtualizarInterfacePorTipoBanco();
    }

    private void OnDatabaseTypeChanged(object? sender, EventArgs? e)
    {
        AtualizarInterfacePorTipoBanco();
    }

    private void AtualizarInterfacePorTipoBanco()
    {
        SqliteInfoCard.IsVisible = true;
        SqliteContainer.IsVisible = true;
        SqliteCaminhoEntry.Text = Preferences.Get(
            "Sqlite_Caminho",
            Preferences.Get("SqliteCaminho", ConfigurationHelper.ObterCaminhoPadraoSqlite()));
        ComplementoLabel.Text = "Complemento (ex: Default Timeout=5;)";
        ComplementoEntry.Placeholder = "Default Timeout=5;";
        ComplementoEntry.Text = Preferences.Get(
            "Sqlite_Complemento",
            Preferences.Get("Complemento", "Default Timeout=5;"));
    }

    private async void OnSalvarBdClicked(object sender, EventArgs e)
    {
        if (DatabaseTypePicker.SelectedItem is not string selectedTypeStr
            || !Enum.TryParse<AppDatabaseType>(selectedTypeStr, out var selectedType)
            || selectedType != AppDatabaseType.Sqlite)
        {
            await DisplayAlertAsync("Aviso", "Selecione um tipo de banco de dados válido", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(SqliteCaminhoEntry.Text))
        {
            await DisplayAlertAsync("Validação", "Informe o caminho do arquivo de banco SQLite", "OK");
            return;
        }

        var caminho = SqliteCaminhoEntry.Text.Trim();
        var complemento = ComplementoEntry.Text?.Trim() ?? string.Empty;

        Preferences.Set("Sqlite_Caminho", caminho);
        Preferences.Set("Sqlite_Complemento", complemento);
        Preferences.Set("SqliteCaminho", caminho);
        Preferences.Set("Complemento", complemento);
        Preferences.Set("DatabaseType", AppDatabaseType.Sqlite.ToString());

        // Dispara mensagem para recarga dinâmica
        WeakReferenceMessenger.Default.Send(new BancoPreferencesUpdatedMessage("BancoAlterado"));

        await DisplayAlertAsync("Sucesso", "Configurações do banco SQLite salvas com sucesso!", "OK");
        await Shell.Current.GoToAsync("//dashboard");
    }
    #endregion

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//dashboard");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}
