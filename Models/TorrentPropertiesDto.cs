using System.Text.Json.Serialization;

namespace TorrentDuplicationChecker.Models;

/// <summary>Subset of qBittorrent <c>/api/v2/torrents/properties</c> response.</summary>
public sealed class TorrentPropertiesDto
{
    [JsonPropertyName("creation_date")]
    public long CreationDate { get; set; }
}
