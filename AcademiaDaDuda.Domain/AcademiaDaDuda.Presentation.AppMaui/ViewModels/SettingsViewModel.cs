using AcademiaDaDuda.Application.Enums;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using AcademiaDaDuda.Presentation.AppMaui.Messages;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Storage;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly DatabaseSettingsStartup _databaseSettingsStartup;

    private string _selectedDatabaseType = "SQLite";
    private string _sqlitePath = string.Empty;
    private string _server = string.Empty;
    private string _port = "3306";
    private string _database = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _trustServerCertificate = true;
    private string _selectedTheme = "Sistema";

    public IReadOnlyList<string> DatabaseTypes { get; } = ["SQLite", "MySQL", "SQL Server"];
    public IReadOnlyList<string> Themes { get; } = ["Sistema", "Claro", "Escuro"];

    public string SelectedDatabaseType
    {
        get => _selectedDatabaseType;
        set
        {
            if (SetProperty(ref _selectedDatabaseType, value))
            {
                OnPropertyChanged(nameof(IsSqlite));
                OnPropertyChanged(nameof(IsServerDatabase));
                OnPropertyChanged(nameof(IsSqlServer));
                Port = value == "MySQL" ? "3306" : "1433";
            }
        }
    }

    public string SqlitePath { get => _sqlitePath; set => SetProperty(ref _sqlitePath, value); }
    public string Server { get => _server; set => SetProperty(ref _server, value); }
    public string Port { get => _port; set => SetProperty(ref _port, value); }
    public string Database { get => _database; set => SetProperty(ref _database, value); }
    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public bool TrustServerCertificate { get => _trustServerCertificate; set => SetProperty(ref _trustServerCertificate, value); }
    public string SelectedTheme { get => _selectedTheme; set => SetProperty(ref _selectedTheme, value); }

    public bool IsSqlite => SelectedDatabaseType == "SQLite";
    public bool IsServerDatabase => !IsSqlite;
    public bool IsSqlServer => SelectedDatabaseType == "SQL Server";

    public SettingsViewModel(DatabaseSettingsStartup databaseSettingsStartup)
    {
        _databaseSettingsStartup = databaseSettingsStartup;
        Title = "Configurações";
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();

            var databaseType = Preferences.Get(DatabaseSettingsStartup.DatabaseTypeKey, nameof(AppDatabaseType.Sqlite));
            SelectedDatabaseType = databaseType switch
            {
                nameof(AppDatabaseType.MySql) => "MySQL",
                nameof(AppDatabaseType.SqlServer) => "SQL Server",
                _ => "SQLite"
            };

            SqlitePath = Preferences.Get(DatabaseSettingsStartup.SqlitePathKey,
                Path.Combine(FileSystem.AppDataDirectory, "academia-da-duda.db3"));
            Server = Preferences.Get(DatabaseSettingsStartup.ServerKey, string.Empty);
            Port = Preferences.Get(DatabaseSettingsStartup.PortKey, IsSqlServer ? 1433 : 3306).ToString();
            Database = Preferences.Get(DatabaseSettingsStartup.NameKey, string.Empty);
            Username = Preferences.Get(DatabaseSettingsStartup.UsernameKey, string.Empty);
            TrustServerCertificate = Preferences.Get(DatabaseSettingsStartup.TrustCertificateKey, true);
            Password = await SecureStorage.Default.GetAsync(DatabaseSettingsStartup.PasswordKey) ?? string.Empty;

            SelectedTheme = Preferences.Get(DatabaseSettingsStartup.ThemeKey, "System") switch
            {
                "Light" => "Claro",
                "Dark" => "Escuro",
                _ => "Sistema"
            };
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Configurações", $"Não foi possível carregar as preferências. {exception.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var type = SelectedDatabaseType switch
            {
                "MySQL" => AppDatabaseType.MySql,
                "SQL Server" => AppDatabaseType.SqlServer,
                _ => AppDatabaseType.Sqlite
            };

            if (type == AppDatabaseType.Sqlite)
            {
                if (string.IsNullOrWhiteSpace(SqlitePath))
                {
                    await Shell.Current.DisplayAlertAsync("Caminho inválido", "Informe o caminho do arquivo do banco SQLite.", "OK");
                    return;
                }

                SqlitePath = Path.GetFullPath(SqlitePath.Trim());
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Server) || string.IsNullOrWhiteSpace(Database) || string.IsNullOrWhiteSpace(Username))
                {
                    await Shell.Current.DisplayAlertAsync("Dados incompletos", "Informe servidor, banco de dados e usuário.", "OK");
                    return;
                }

                if (!int.TryParse(Port, out var parsedPort) || parsedPort is < 1 or > 65535)
                {
                    await Shell.Current.DisplayAlertAsync("Porta inválida", "Informe uma porta entre 1 e 65535.", "OK");
                    return;
                }

                Port = parsedPort.ToString();
            }

            var selectedTheme = SelectedTheme switch
            {
                "Claro" => AppTheme.Light,
                "Escuro" => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };

            var connectionString = DatabaseConnectionStrings.Build(
                type,
                SqlitePath,
                Server.Trim(),
                type == AppDatabaseType.Sqlite ? 0 : int.Parse(Port),
                Database.Trim(),
                Username.Trim(),
                Password,
                TrustServerCertificate);

            // Secure storage can fail on some devices. Persist the rest only after the password is safe.
            if (type != AppDatabaseType.Sqlite)
                await SecureStorage.Default.SetAsync(DatabaseSettingsStartup.PasswordKey, Password);

            Preferences.Set(DatabaseSettingsStartup.DatabaseTypeKey, type.ToString());
            Preferences.Set(DatabaseSettingsStartup.SqlitePathKey, SqlitePath);
            Preferences.Set(DatabaseSettingsStartup.ServerKey, Server.Trim());
            Preferences.Set(DatabaseSettingsStartup.PortKey, type == AppDatabaseType.Sqlite ? 3306 : int.Parse(Port));
            Preferences.Set(DatabaseSettingsStartup.NameKey, Database.Trim());
            Preferences.Set(DatabaseSettingsStartup.UsernameKey, Username.Trim());
            Preferences.Set(DatabaseSettingsStartup.TrustCertificateKey, TrustServerCertificate);
            Preferences.Set(DatabaseSettingsStartup.ThemeKey, selectedTheme.ToString());

            WeakReferenceMessenger.Default.Send(new BancoPreferenciasAtualizadasMessage(new DatabaseSettings(type, connectionString)));
            WeakReferenceMessenger.Default.Send(new TemaPreferenciasAtualizadasMessage(selectedTheme));

            await Shell.Current.DisplayAlertAsync("Configurações salvas", "As preferências foram aplicadas.", "OK");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Falha ao salvar", $"Não foi possível salvar as configurações. {exception.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
