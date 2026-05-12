using System.Globalization;
using TorrentDuplicationChecker.Localization;

namespace TorrentDuplicationChecker.Models;

public enum DuplicateMatchKind
{
    Comment,
    NameAndSize
}

public sealed class DuplicateTorrentRow
{
    public required DuplicateMatchKind Kind { get; init; }

    /// <summary>Normalized grouping key (comment text or synthetic name+size label).</summary>
    public required string GroupKey { get; init; }

    /// <summary>
    /// Stable id for duplicate cluster grouping in the UI; not shown to the user.
    /// Rows in the same cluster share identical <see cref="Kind"/> and <see cref="GroupKey"/>.
    /// </summary>
    public string DuplicateClusterId => $"{(int)Kind}\u001f{GroupKey}";

    public required int CopiesInGroup { get; init; }

    public required string TorrentName { get; init; }

    public required string Hash { get; init; }

    public long SizeBytes { get; init; }

    public required string SavePath { get; init; }

    /// <summary>Instant used to rank rows within a duplicate group (metadata creation time, otherwise <c>added_on</c>).</summary>
    public long? EffectiveUnixSeconds { get; init; }

    public DuplicateRecencyHighlight RecencyHighlight { get; init; }

    public string ReferenceDateDisplay =>
        EffectiveUnixSeconds is > 0
            ? DateTimeOffset.FromUnixTimeSeconds(EffectiveUnixSeconds.Value).ToLocalTime()
                .ToString("g", CultureInfo.CurrentCulture)
            : Strings.Grid_ReferenceDateUnknown;

    public string MatchKindDisplay => Kind switch
    {
        DuplicateMatchKind.Comment => Strings.Grid_MatchKindComment,
        _ => Strings.Grid_MatchKindNameSize
    };

    public string GroupDisplay =>
        Kind == DuplicateMatchKind.Comment && string.IsNullOrEmpty(GroupKey)
            ? Strings.Grid_EmptyCommentKey
            : GroupKey;

    public string SizeDisplay => FormatSize(SizeBytes);

    private static string FormatSize(long bytes)
    {
        const int scale = 1024;
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double v = bytes;
        var u = 0;
        while (v >= scale && u < units.Length - 1)
        {
            v /= scale;
            u++;
        }

        return u == 0 ? $"{bytes:0} {units[u]}" : $"{v:0.##} {units[u]}";
    }
}
