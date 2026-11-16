using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace AcademiaDaDuda.Presentation.AppMaui.Localization;

[ContentProperty(nameof(Value))]
public sealed class TranslateFormatExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;
    public BindingBase Value { get; set; } = new Binding();

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new MultiBinding
        {
            Converter = new LocalizedFormatConverter(),
            Mode = BindingMode.OneWay
        };
        binding.Bindings.Add(new Binding(nameof(LocalizedText.Value), source: new LocalizedText(Key)));
        binding.Bindings.Add(Value);
        return binding;
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}
