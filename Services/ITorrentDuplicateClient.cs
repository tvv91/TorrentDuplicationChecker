using TorrentDuplicationChecker.Models;

namespace TorrentDuplicationChecker.Services;

public interface ITorrentDuplicateClient : IAsyncDisposable
{
    Task<IReadOnlyList<TorrentListItemDto>> GetTorrentsAsync(CancellationToken cancellationToken = default);
}
