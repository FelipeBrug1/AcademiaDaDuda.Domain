using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDaDuda.Presentation.AppMaui.Messages;

public sealed class TemaPreferenciasAtualizadasMessage(AppTheme value)
    : ValueChangedMessage<AppTheme>(value);
