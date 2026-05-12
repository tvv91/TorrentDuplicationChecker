# Torrent Duplication Checker

Other languages: [Українська](README.uk.md) · [Русский](README.ru.md)

Desktop app for **Windows** (.NET 10, WPF) that connects to your torrent client over the **Web UI API**, loads the torrent list, and highlights **duplicate groups**: entries that likely refer to the same release by **matching comment** or by **matching name and size**.

It helps spot redundant torrents (e.g. several uploads of the same thing) and risky overlaps when the **same logical duplicate** uses **different save paths**.

## Features

- **Analyze** pulls torrents from the client and lists rows that belong to a duplicate group (with cancel support).
- **Compared date**: prefers `.torrent` metadata creation time (`creation_date` from the API when loaded); otherwise uses **added_on** (when the torrent was added to the client). Used only to rank rows inside a group when paths match.
- **Row colors**: green — newest in the group; amber — older in the group (same save paths); red — **different save paths** inside one duplicate group (whole group highlighted).
- **Grouping** in the grid: each duplicate cluster is grouped; the group header shows the torrent name.
- **Settings**: Web UI base URL, optional login/password, **interface language** (English / Ukrainian / Russian).
- **Help** (“?”): short legend for the colors.
- **Open save folder**: opens File Explorer with the save folder **selected** (`explorer /select`).
- Settings are stored under `%AppData%\TorrentDuplicationChecker\settings.json`.

## Supported torrent clients

| Client | Status |
|--------|--------|
| [**qBittorrent**](https://www.qbittorrent.org/) (Web UI / API v2) | Supported |

**Only qBittorrent is implemented at the moment.** The code uses a pluggable client abstraction so other clients could be added later—they are not implemented yet.

## Requirements

- Windows  
- [.NET 10](https://dotnet.microsoft.com/download) SDK (to build) or runtime (to run published builds)  
- qBittorrent with **Web UI** enabled (and credentials if you use them)

## Build

```bash
dotnet build -c Release
```

Run `TorrentDuplicationChecker.exe` from `bin\Release\net10.0-windows\`.

## Licence

This project is provided as-is for your own use; add a licence file if you redistribute it.
