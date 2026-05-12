using System.Globalization;
using System.Resources;

namespace TorrentDuplicationChecker.Localization;

public static class Strings
{
    private static readonly ResourceManager Rm = new(
        "TorrentDuplicationChecker.Localization.Strings",
        typeof(Strings).Assembly);

    private static string T(string key) => Rm.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    public static string AppTitle => T(nameof(AppTitle));

    public static string Label_Client => T(nameof(Label_Client));

    public static string Btn_Settings => T(nameof(Btn_Settings));

    public static string Btn_Analyze => T(nameof(Btn_Analyze));

    public static string Btn_Cancel => T(nameof(Btn_Cancel));

    public static string Btn_OpenSaveFolder => T(nameof(Btn_OpenSaveFolder));

    public static string GridCol_MatchKind => T(nameof(GridCol_MatchKind));

    public static string GridCol_GroupKey => T(nameof(GridCol_GroupKey));

    public static string GridCol_Copies => T(nameof(GridCol_Copies));

    public static string GridCol_TorrentName => T(nameof(GridCol_TorrentName));

    public static string GridCol_Size => T(nameof(GridCol_Size));

    public static string GridCol_SavePath => T(nameof(GridCol_SavePath));

    public static string GridCol_ReferenceDate => T(nameof(GridCol_ReferenceDate));

    public static string Grid_ReferenceDateUnknown => T(nameof(Grid_ReferenceDateUnknown));

    public static string Grid_MatchKindComment => T(nameof(Grid_MatchKindComment));

    public static string Grid_MatchKindNameSize => T(nameof(Grid_MatchKindNameSize));

    public static string Grid_EmptyCommentKey => T(nameof(Grid_EmptyCommentKey));

    public static string Settings_WindowTitle => T(nameof(Settings_WindowTitle));

    public static string Settings_UiLanguage => T(nameof(Settings_UiLanguage));

    public static string Settings_TorrentClientApi => T(nameof(Settings_TorrentClientApi));

    public static string Settings_WebUiAddress => T(nameof(Settings_WebUiAddress));

    public static string Settings_AccountOptional => T(nameof(Settings_AccountOptional));

    public static string Label_Login => T(nameof(Label_Login));

    public static string Label_Password => T(nameof(Label_Password));

    public static string Btn_Ok => T(nameof(Btn_Ok));

    public static string Btn_CancelDialog => T(nameof(Btn_CancelDialog));

    public static string Msg_SpecifyWebUiUrl => T(nameof(Msg_SpecifyWebUiUrl));

    public static string Msg_SettingsSaved => T(nameof(Msg_SettingsSaved));

    public static string Msg_OpenFolderFailedTitle => T(nameof(Msg_OpenFolderFailedTitle));

    public static string Msg_OpenFolderInvalidPath => T(nameof(Msg_OpenFolderInvalidPath));

    public static string Status_Ready => T(nameof(Status_Ready));

    public static string Analyze_StatusConnecting => T(nameof(Analyze_StatusConnecting));

    public static string Analyze_StatusLoadingTorrents => T(nameof(Analyze_StatusLoadingTorrents));

    public static string Analyze_StatusAnalyzing => T(nameof(Analyze_StatusAnalyzing));

    public static string Analyze_StatusComplete_Format(int rows) =>
        string.Format(T("Analyze_StatusComplete"), rows);

    public static string Analyze_StatusNoDuplicates => T(nameof(Analyze_StatusNoDuplicates));

    public static string Analyze_StatusCancelled => T(nameof(Analyze_StatusCancelled));

    public static string Analyze_StatusError => T(nameof(Analyze_StatusError));

    public static string Err_LoginHttp_Format(int statusCode, string body) =>
        string.Format(T("Err_LoginHttp"), statusCode, body);

    public static string Err_InvalidCredentials => T(nameof(Err_InvalidCredentials));

    public static string Err_BaseUrlMissing => T(nameof(Err_BaseUrlMissing));

    public static string Help_WindowTitle => T(nameof(Help_WindowTitle));

    public static string Help_ButtonTooltip => T(nameof(Help_ButtonTooltip));

    public static string Help_LegendTitle => T(nameof(Help_LegendTitle));

    public static string Help_LegendIntro => T(nameof(Help_LegendIntro));

    public static string Help_LegendGreen => T(nameof(Help_LegendGreen));

    public static string Help_LegendAmber => T(nameof(Help_LegendAmber));

    public static string Help_LegendRed => T(nameof(Help_LegendRed));
}
