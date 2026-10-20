using AcademiaDaDuda.Application.Enums;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDaDuda.Presentation.AppMaui.Messages;

public sealed record DatabaseSettings(AppDatabaseType DatabaseType, string ConnectionString);

public sealed class BancoPreferenciasAtualizadasMessage(DatabaseSettings value)
    : ValueChangedMessage<DatabaseSettings>(value);
