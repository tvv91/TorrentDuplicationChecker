using System.Net;
using System.Net.Http.Json;
using TorrentDuplicationChecker.Localization;
using TorrentDuplicationChecker.Models;

namespace TorrentDuplicationChecker.Services;

[TorrentClientKind(QBitTorrentDuplicateClient.QBitTorrentClientId, "qBittorrent")]
public sealed class QBitTorrentDuplicateClient : ITorrentDuplicateClient
{
    public const string QBitTorrentClientId = TorrentClientRegistry.QBitTorrentId;

    private readonly HttpClient _http;
    private readonly HttpClientHandler _handler;
    private readonly string? _userName;
    private readonly string? _password;
    private readonly SemaphoreSlim _authGate = new(1, 1);
    private bool _loggedIn;

    public QBitTorrentDuplicateClient(string baseUrl, string? userName, string? password)
    {
        _userName = string.IsNullOrWhiteSpace(userName) ? null : userName.Trim();
        _password = password ?? "";

        var uri = NormalizeBaseUri(baseUrl);
        _handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };
        _http = new HttpClient(_handler)
        {
            BaseAddress = uri
        };
    }

    private static Uri NormalizeBaseUri(string baseUrl)
    {
        var t = baseUrl.Trim();
        if (t.Length == 0)
            throw new ArgumentException(Strings.Err_BaseUrlMissing, nameof(baseUrl));

        if (!t.EndsWith('/'))
            t += "/";

        return new Uri(t, UriKind.Absolute);
    }

    public async Task<IReadOnlyList<TorrentListItemDto>> GetTorrentsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
        var list = await _http.GetFromJsonAsync<List<TorrentListItemDto>>("/api/v2/torrents/info", cancellationToken)
            .ConfigureAwait(false);
        list ??= [];

        var duplicateHashes = DuplicateTorrentAnalyzer.GetHashesInDuplicateGroups(list);
        await EnrichCreationDatesAsync(list, duplicateHashes, cancellationToken).ConfigureAwait(false);

        return list;
    }

    private async Task EnrichCreationDatesAsync(
        List<TorrentListItemDto> torrents,
        HashSet<string> duplicateHashes,
        CancellationToken cancellationToken)
    {
        if (duplicateHashes.Count == 0)
            return;

        using var gate = new SemaphoreSlim(8, 8);
        await Task.WhenAll(duplicateHashes.Select(hash =>
                FetchCreationDateAsync(torrents, hash, gate, cancellationToken)))
            .ConfigureAwait(false);
    }

    private async Task FetchCreationDateAsync(
        List<TorrentListItemDto> torrents,
        string hash,
        SemaphoreSlim gate,
        CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var uri = "/api/v2/torrents/properties?hash=" + Uri.EscapeDataString(hash);
            var props = await _http.GetFromJsonAsync<TorrentPropertiesDto>(uri, cancellationToken)
                .ConfigureAwait(false);
            if (props is not { CreationDate: > 0 })
                return;

            foreach (var t in torrents)
            {
                if (string.Equals(t.Hash, hash, StringComparison.OrdinalIgnoreCase))
                    t.CreationDateUnix = props.CreationDate;
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Ignore missing properties or transient errors for individual torrents.
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        if (_userName is null)
        {
            _loggedIn = true;
            return;
        }

        if (_loggedIn)
            return;

        await _authGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_loggedIn)
                return;

            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["username"] = _userName,
                ["password"] = _password ?? ""
            });

            using var response = await _http.PostAsync("/api/v2/auth/login", content, cancellationToken)
                .ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(Strings.Err_LoginHttp_Format((int)response.StatusCode, body));

            if (body is "Fails." or "Fail.")
                throw new InvalidOperationException(Strings.Err_InvalidCredentials);

            _loggedIn = true;
        }
        finally
        {
            _authGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _http.Dispose();
        _handler.Dispose();
        _authGate.Dispose();
    }
}
