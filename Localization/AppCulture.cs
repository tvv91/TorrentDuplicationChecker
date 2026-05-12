using System.Globalization;

namespace TorrentDuplicationChecker.Localization;

public static class AppCulture
{
    public static void Apply(string languageCode)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(languageCode.Trim());
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
        }
        catch (CultureNotFoundException)
        {
            Apply("en");
        }
    }
}
