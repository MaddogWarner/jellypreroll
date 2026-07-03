# Jellyfin PreRoll & Trailers

A minimal, **deterministic** cinema-mode intro provider for Jellyfin 10.11.

Before the selected movie it plays, in order:

1. Up to **2 trailers** (configurable 0–2), and
2. **1 preroll**,

then the feature itself. It **never substitutes an unrelated random video** to reach the
trailer count — if only one trailer is available, it plays one. This is the key difference
from the CherryFloors cinema-mode plugin, whose random fallback caused the "plays the wrong
video" behaviour.

## What it does / doesn't do

- ✅ Movies-only option (skips TV and everything else).
- ✅ Excludes the **selected movie's own trailer**.
- ✅ Trailers from **nested per-movie local trailers** and/or a **dedicated trailers library**.
- ✅ Single preroll from a dedicated preroll library.
- ❌ It cannot inject loose video files. Every trailer/preroll **must be an indexed Jellyfin
  library item** (Jellyfin design constraint — see Library setup).
- ⚠️ Intros are rendered **client-side**. Playback depends on each client supporting Cinema
  Mode. Web is best-supported; Android TV / some mobile clients have historically been flaky.
  No server plugin can change that — verify on your own clients (see Testing).

## Requirements

- Jellyfin server **10.11.x** (built and package-verified against 10.11.11).
- Each viewing user must enable **Cinema Mode** in Playback settings.

## Library setup (required)

Jellyfin has no "preroll" content type, so:

1. Create a **library of type "Movies"** pointing at a folder of preroll clips (e.g. `Prerolls/`).
2. (Optional) Create another **"Movies"-type library** for a shared trailers folder.
3. For nested trailers, store each movie's trailer using Jellyfin's
   [local trailer convention](https://jellyfin.org/docs/general/server/media/movies/#movie-extras)
   (a `trailers/` subfolder inside the movie folder, or a `<movie>-trailer.mkv` file).

Keep prerolls/trailers in their **own** libraries — do not mix them into your main Movies library.

## Install

### Option A — plugin repository (recommended, supports updates)

1. Push this project to a GitHub repo and cut a release (see [Releasing](#releasing)).
2. In Jellyfin: **Dashboard → Plugins → Repositories → +** and add:
   `https://raw.githubusercontent.com/<owner>/<repo>/main/manifest.json`
3. **Catalog → General → PreRoll & Trailers → Install**, then restart Jellyfin.

Future versions appear in the catalog automatically once you tag a new release.

### Option B — manual copy

1. Build (see below) or take `dist/PreRoll_1.0.0.0/`.
2. Copy the `PreRoll_1.0.0.0` folder (containing `Jellyfin.Plugin.PreRoll.dll` + `meta.json`)
   into your server's plugin directory:
   - Linux/Docker: `/config/plugins/`
   - Windows: `%ProgramData%\Jellyfin\Server\plugins\`
   - macOS: `~/.local/share/jellyfin/plugins/`
3. Restart Jellyfin.

### After installing (either option)

- Dashboard → Plugins → **PreRoll & Trailers** → configure libraries and options.
- Ensure each viewing user has **Cinema Mode** enabled in Playback settings.

## Configuration

| Setting | Default | Notes |
|---|---|---|
| Enable | on | Master switch. |
| Movies only | on | Off = also provide intros for other item types. |
| Number of trailers | 2 | 0–2. |
| Use nested local trailers | on | Pull trailers belonging to other movies. |
| Dedicated trailers library | None | Optional Movies-type library of trailer files. |
| Preroll library | None | Movies-type library; empty = no preroll. |

## Build

Requires the .NET 9 SDK.

```bash
dotnet build -c Release Jellyfin.Plugin.PreRoll/Jellyfin.Plugin.PreRoll.csproj
# output: Jellyfin.Plugin.PreRoll/bin/Release/net9.0/Jellyfin.Plugin.PreRoll.dll
```

## Releasing

CI is in `.github/workflows/`:

- **build.yml** — compiles on every push/PR to `main`.
- **release.yml** — on a `v*` tag it builds, zips (`dll` + `meta.json`), creates a GitHub
  release with the zip, computes its MD5, and updates `manifest.json` (prepending the new
  version with the correct `sourceUrl` + `checksum`) and commits it back to `main`.

To publish a version, bump the tag to match the version you want (4-part), then push it:

```bash
git tag v1.0.0.0
git push origin v1.0.0.0
```

The tag drives the version stamped into the assembly, `meta.json`, the zip name and the
manifest, so they always stay in sync. No manual checksum editing.

## Testing checklist (on your live server — do Web first)

1. Play a movie → queue shows 1–2 trailers, then the preroll, then the movie. Confirm order.
2. Confirm **none** of the trailers is that movie's own trailer.
3. Set trailers = 1 → exactly one; set = 0 → preroll only.
4. Empty the trailer sources → preroll only, **no unrelated random video** appears.
5. Movies-only on + play a TV episode → no intros.
6. Repeat step 1 on each client (Web, JMP, Android TV, mobile) and note which honour intros.
7. Check `jellyfin.log` for lines starting `PreRoll:` and any errors.

## License

GPL-3.0 — see [LICENSE](LICENSE).
