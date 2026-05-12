namespace TorrentDuplicationChecker.Models;

public enum DuplicateRecencyHighlight
{
    None,
    Newest,
    /// <summary>Older release of what is likely the same torrent (same save location pattern).</summary>
    Older,
    /// <summary>Duplicate group spans more than one save path — overlapping old and new data risk.</summary>
    SavePathConflict
}
