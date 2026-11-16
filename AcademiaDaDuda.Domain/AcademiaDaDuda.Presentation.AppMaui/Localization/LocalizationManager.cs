using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Storage;

namespace AcademiaDaDuda.Presentation.AppMaui.Localization;

public sealed class LocalizationManager : INotifyPropertyChanged
{
    public const string LanguagePreferenceKey = "AppLanguage";
    private static readonly string[] SupportedCultures = ["pt-BR", "en-US", "es-ES"];
    private static readonly Regex FormatToken = new(@"\{\d+(?:,[^}]*)?(?:\:[^}]*)?\}", RegexOptions.Compiled);

    public static LocalizationManager Instance { get; } = new();

    private CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");

    public event PropertyChangedEventHandler? PropertyChanged;

    public CultureInfo CurrentCulture => _culture;

    public string this[string key] => Translate(key);

    private LocalizationManager()
    {
        AppResources.Culture = _culture;
    }

    public void InitializeFromPreferences()
    {
        var cultureName = Preferences.Get(LanguagePreferenceKey, GetDefaultCultureName());
        SetCulture(cultureName, savePreference: false);
    }

    private static string GetDefaultCultureName() => CultureInfo.InstalledUICulture.TwoLetterISOLanguageName switch
    {
        "en" => "en-US",
        "es" => "es-ES",
        _ => "pt-BR"
    };

    public void SetCulture(string cultureName, bool savePreference = true)
    {
        var normalizedName = SupportedCultures.FirstOrDefault(
            supported => string.Equals(supported, cultureName, StringComparison.OrdinalIgnoreCase)) ?? "pt-BR";
        var culture = CultureInfo.GetCultureInfo(normalizedName);
        ApplyCultureToCurrentThreads(culture);
        if (string.Equals(_culture.Name, culture.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (savePreference)
                Preferences.Set(LanguagePreferenceKey, normalizedName);
            return;
        }

        _culture = culture;
        AppResources.Culture = culture;
        if (savePreference)
            Preferences.Set(LanguagePreferenceKey, normalizedName);

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        WeakReferenceMessenger.Default.Send(new CulturePreferencesUpdatedMessage(normalizedName));
    }

    private static void ApplyCultureToCurrentThreads(CultureInfo culture)
    {
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

#if WINDOWS
        try
        {
            global::Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = culture.Name;
        }
        catch (Exception)
        {
            // Some Windows hosts do not allow changing the process language at runtime.
        }
#endif
    }

    public string Translate(string key)
    {
        if (string.IsNullOrEmpty(key))
            return key;

        var localized = AppResources.GetString(key);
        return string.IsNullOrEmpty(localized) ? key : localized;
    }

    public string Format(string key, params object?[] arguments)
    {
        var format = Translate(key);
        return string.Format(_culture, format, arguments);
    }

    public string TranslateMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        var direct = AppResources.GetString(message);
        if (!string.IsNullOrEmpty(direct))
            return direct;

        var formatted = TryTranslateFormattedMessage(message);
        if (formatted is not null)
            return formatted;

        var lines = message.Split(["\r\n", "\n"], StringSplitOptions.None);
        return string.Join(Environment.NewLine, lines.Select(TranslateMessageLine));
    }

    private string TranslateMessageLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return line;

        var localized = AppResources.GetString(line);
        if (!string.IsNullOrEmpty(localized))
            return localized;

        if (line.StartsWith("• ", StringComparison.Ordinal))
        {
            var text = line[2..];
            var translatedText = AppResources.GetString(text) ?? TryTranslateFormattedMessage(text);
            return translatedText is null ? line : "• " + translatedText;
        }

        return TryTranslateFormattedMessage(line) ?? line;
    }

    private string? TryTranslateFormattedMessage(string source)
    {
        var neutral = AppResources.GetResourceSet(CultureInfo.GetCultureInfo("pt-BR"), true, true);
        if (neutral is null)
            return null;

        foreach (System.Collections.DictionaryEntry entry in neutral)
        {
            if (entry.Key is not string pattern || !FormatToken.IsMatch(pattern))
                continue;

            var placeholders = FormatToken.Matches(pattern);
            var regexBuilder = new System.Text.StringBuilder("^");
            var lastIndex = 0;
            foreach (Match placeholder in placeholders)
            {
                regexBuilder.Append(Regex.Escape(pattern[lastIndex..placeholder.Index]));
                regexBuilder.Append("(.+?)");
                lastIndex = placeholder.Index + placeholder.Length;
            }
            regexBuilder.Append(Regex.Escape(pattern[lastIndex..]));
            regexBuilder.Append('$');
            var regexPattern = regexBuilder.ToString();
            var matchResult = Regex.Match(source, regexPattern, RegexOptions.Singleline);
            if (!matchResult.Success || matchResult.Groups.Count != placeholders.Count + 1)
                continue;

            var translatedPattern = AppResources.GetString(pattern);
            if (string.IsNullOrEmpty(translatedPattern))
                return source;

            try
            {
                return string.Format(_culture, translatedPattern,
                    Enumerable.Range(1, placeholders.Count).Select(index => (object?)matchResult.Groups[index].Value).ToArray());
            }
            catch (FormatException)
            {
                return source;
            }
        }

        return null;
    }
}
