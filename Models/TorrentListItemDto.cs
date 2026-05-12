using System.Text.Json.Serialization;

namespace TorrentDuplicationChecker.Models;

/// <summary>Subset of qBittorrent <c>/api/v2/torrents/info</c> fields used for duplicate detection.</summary>
public sealed class TorrentListItemDto
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = "";

    [JsonPropertyName("save_path")]
    public string SavePath { get; set; } = "";

    /// <summary>Unix time (seconds) when the torrent was added to the client (<c>added_on</c>).</summary>
    [JsonPropertyName("added_on")]
    public long AddedOn { get; set; }

    /// <summary>Unix time (seconds) from the .torrent metadata, if loaded via <c>torrents/properties</c>.</summary>
    public long? CreationDateUnix { get; set; }
}
