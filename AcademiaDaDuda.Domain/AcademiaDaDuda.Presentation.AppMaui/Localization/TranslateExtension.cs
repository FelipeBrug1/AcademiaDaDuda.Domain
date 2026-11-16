using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace AcademiaDaDuda.Presentation.AppMaui.Localization;

[ContentProperty(nameof(Key))]
public sealed class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider serviceProvider) =>
        new Binding(nameof(LocalizedText.Value), source: new LocalizedText(Key), mode: BindingMode.OneWay);

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}
