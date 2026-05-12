using System.Collections.Immutable;
using TorrentDuplicationChecker.Models;

namespace TorrentDuplicationChecker.Services;

public sealed record TorrentClientOption(string Id, string DisplayName, Type ImplementationType);

public static class TorrentClientRegistry
{
    public const string QBitTorrentId = "qbittorrent";

    private static readonly ImmutableArray<TorrentClientOption> Options = BuildOptions();

    public static IReadOnlyList<TorrentClientOption> All => Options;

    public static ITorrentDuplicateClient Create(AppSettings settings)
    {
        var opt = Options.FirstOrDefault(o => o.Id == settings.SelectedTorrentClientKindId)
                  ?? Options.FirstOrDefault(o => o.Id == QBitTorrentId)
                  ?? throw new InvalidOperationException("No torrent Web UI clients are registered.");

        var instance = Activator.CreateInstance(opt.ImplementationType, settings.WebUiBaseUrl, settings.UserName,
            settings.Password);
        if (instance is not ITorrentDuplicateClient client)
            throw new InvalidOperationException($"Client '{opt.DisplayName}' could not be created.");

        return client;
    }

    private static ImmutableArray<TorrentClientOption> BuildOptions()
    {
        var list = new List<TorrentClientOption>();
        foreach (var t in typeof(ITorrentDuplicateClient).Assembly.GetTypes())
        {
            var attr = (TorrentClientKindAttribute?)Attribute.GetCustomAttribute(t, typeof(TorrentClientKindAttribute));
            if (attr is null || t.IsAbstract)
                continue;
            if (!typeof(ITorrentDuplicateClient).IsAssignableFrom(t))
                continue;
            list.Add(new TorrentClientOption(attr.Id, attr.DisplayName, t));
        }

        return [.. list.OrderBy(o => o.DisplayName, StringComparer.CurrentCultureIgnoreCase)];
    }
}
