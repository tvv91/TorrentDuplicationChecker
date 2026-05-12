namespace TorrentDuplicationChecker.Services;

[AttributeUsage(AttributeTargets.Class)]
public sealed class TorrentClientKindAttribute(string id, string displayName) : Attribute
{
    public string Id { get; } = id;

    public string DisplayName { get; } = displayName;
}
