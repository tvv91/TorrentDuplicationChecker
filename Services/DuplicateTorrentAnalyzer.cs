using TorrentDuplicationChecker.Models;

namespace TorrentDuplicationChecker.Services;

public static class DuplicateTorrentAnalyzer
{
    /// <summary>Returns info hashes that belong to at least one duplicate group (comment or name+size).</summary>
    public static HashSet<string> GetHashesInDuplicateGroups(IReadOnlyList<TorrentListItemDto> torrents)
    {
        var hashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var list = torrents ?? [];

        foreach (var g in list
                     .GroupBy(t => (t.Comment ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
                     .Where(g => g.Count() > 1))
        {
            foreach (var t in g)
                AddHash(hashes, t.Hash);
        }

        foreach (var g in list
                     .GroupBy(t => ((t.Name ?? "").Trim(), t.Size))
                     .Where(g => g.Count() > 1))
        {
            foreach (var t in g)
                AddHash(hashes, t.Hash);
        }

        return hashes;
    }

    public static IReadOnlyList<DuplicateTorrentRow> Analyze(IReadOnlyList<TorrentListItemDto> torrents)
    {
        var rows = new List<DuplicateTorrentRow>();
        var list = torrents ?? [];

        AddCommentDuplicates(list, rows);
        AddNameAndSizeDuplicates(list, rows);

        return rows
            .OrderBy(r => r.Kind)
            .ThenBy(r => r.GroupDisplay, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.TorrentName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.Hash, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    internal static long? GetEffectiveUnixSeconds(TorrentListItemDto t)
    {
        if (t.CreationDateUnix is { } c && c > 0)
            return c;
        if (t.AddedOn > 0)
            return t.AddedOn;
        return null;
    }

    private static void AddHash(HashSet<string> hashes, string? hash)
    {
        if (!string.IsNullOrWhiteSpace(hash))
            hashes.Add(hash.Trim());
    }

    private static void AddCommentDuplicates(IReadOnlyList<TorrentListItemDto> list, List<DuplicateTorrentRow> rows)
    {
        foreach (var g in list
                     .GroupBy(t => (t.Comment ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
                     .Where(g => g.Count() > 1)
                     .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
        {
            var count = g.Count();
            var highlights = ResolveGroupHighlights(g);
            foreach (var t in g.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            {
                var hash = (t.Hash ?? "").Trim();
                rows.Add(BuildRow(DuplicateMatchKind.Comment, g.Key, count, t,
                    highlights.TryGetValue(hash, out var h) ? h : DuplicateRecencyHighlight.None));
            }
        }
    }

    private static void AddNameAndSizeDuplicates(IReadOnlyList<TorrentListItemDto> list, List<DuplicateTorrentRow> rows)
    {
        foreach (var g in list
                     .GroupBy(t => ((t.Name ?? "").Trim(), t.Size))
                     .Where(g => g.Count() > 1)
                     .OrderBy(g => g.Key.Item1, StringComparer.OrdinalIgnoreCase))
        {
            var count = g.Count();
            var label = $"{g.Key.Item1} · {FormatSize(g.Key.Item2)}";
            var highlights = ResolveGroupHighlights(g);
            foreach (var t in g.OrderBy(x => x.Hash, StringComparer.OrdinalIgnoreCase))
            {
                var hash = (t.Hash ?? "").Trim();
                rows.Add(BuildRow(DuplicateMatchKind.NameAndSize, label, count, t,
                    highlights.TryGetValue(hash, out var h) ? h : DuplicateRecencyHighlight.None));
            }
        }
    }

    private static DuplicateTorrentRow BuildRow(
        DuplicateMatchKind kind,
        string groupKey,
        int copiesInGroup,
        TorrentListItemDto t,
        DuplicateRecencyHighlight highlight)
    {
        var effective = GetEffectiveUnixSeconds(t);
        return new DuplicateTorrentRow
        {
            Kind = kind,
            GroupKey = groupKey,
            CopiesInGroup = copiesInGroup,
            TorrentName = t.Name ?? "",
            Hash = t.Hash ?? "",
            SizeBytes = t.Size,
            SavePath = t.SavePath ?? "",
            EffectiveUnixSeconds = effective,
            RecencyHighlight = highlight
        };
    }

    private static Dictionary<string, DuplicateRecencyHighlight> ResolveGroupHighlights(
        IEnumerable<TorrentListItemDto> groupMembers)
    {
        var members = groupMembers as IList<TorrentListItemDto> ?? groupMembers.ToList();
        if (HasSavePathConflict(members))
            return BuildUniformHighlight(members, DuplicateRecencyHighlight.SavePathConflict);

        return ComputeRecencyHighlights(members);
    }

    private static bool HasSavePathConflict(IEnumerable<TorrentListItemDto> groupMembers)
    {
        var paths = groupMembers
            .Select(t => (t.SavePath ?? "").Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        return paths.Count > 1;
    }

    private static Dictionary<string, DuplicateRecencyHighlight> BuildUniformHighlight(
        IEnumerable<TorrentListItemDto> groupMembers,
        DuplicateRecencyHighlight value)
    {
        var dict = new Dictionary<string, DuplicateRecencyHighlight>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in groupMembers)
        {
            var hash = (t.Hash ?? "").Trim();
            if (hash.Length > 0)
                dict[hash] = value;
        }

        return dict;
    }

    /// <summary>
    /// Within one duplicate group (same save paths), marks newest timestamps green and strictly older amber.
    /// Ties for maximum stay <see cref="DuplicateRecencyHighlight.Newest"/>; unknown timestamps stay neutral.
    /// </summary>
    private static Dictionary<string, DuplicateRecencyHighlight> ComputeRecencyHighlights(
        IEnumerable<TorrentListItemDto> groupMembers)
    {
        var dict = new Dictionary<string, DuplicateRecencyHighlight>(StringComparer.OrdinalIgnoreCase);
        var pairs = groupMembers
            .Select(t => (Hash: (t.Hash ?? "").Trim(), Ts: GetEffectiveUnixSeconds(t)))
            .Where(p => p.Hash.Length > 0)
            .ToList();

        var knownTs = pairs.Where(p => p.Ts.HasValue).Select(p => p.Ts!.Value).ToList();
        if (knownTs.Count < 2 || knownTs.Distinct().Count() <= 1)
        {
            foreach (var (hash, _) in pairs)
                dict[hash] = DuplicateRecencyHighlight.None;
            return dict;
        }

        var maxTs = knownTs.Max();
        foreach (var (hash, ts) in pairs)
        {
            if (!ts.HasValue)
                dict[hash] = DuplicateRecencyHighlight.None;
            else if (ts.Value == maxTs)
                dict[hash] = DuplicateRecencyHighlight.Newest;
            else
                dict[hash] = DuplicateRecencyHighlight.Older;
        }

        return dict;
    }

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
