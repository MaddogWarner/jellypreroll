using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.PreRoll;

/// <summary>
/// Deterministic cinema-mode intro provider: up to two trailers followed by a
/// single preroll, played before the selected feature. Never substitutes
/// unrelated random video to reach a target count.
/// </summary>
public class IntroProvider : IIntroProvider
{
    private const int MaxTrailers = 2;

    private readonly ILogger<IntroProvider> _logger;
    private readonly Random _random = new Random();

    /// <summary>
    /// Initializes a new instance of the <see cref="IntroProvider"/> class.
    /// </summary>
    /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
    public IntroProvider(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<IntroProvider>();
    }

    /// <inheritdoc />
    public string Name => "PreRoll & Trailers";

    /// <inheritdoc />
    public Task<IEnumerable<IntroInfo>> GetIntros(BaseItem item, User user)
    {
        try
        {
            return Task.FromResult(Build(item));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "PreRoll: error building intros for {Item}", item?.Name);
            return Task.FromResult(Enumerable.Empty<IntroInfo>());
        }
    }

    private IEnumerable<IntroInfo> Build(BaseItem item)
    {
        var config = Plugin.Instance?.Configuration;
        var library = Plugin.LibraryManager;
        if (config is null || library is null || !config.Enabled)
        {
            return Enumerable.Empty<IntroInfo>();
        }

        if (config.MoviesOnly && item is not Movie)
        {
            return Enumerable.Empty<IntroInfo>();
        }

        var result = new List<IntroInfo>();

        var trailerTarget = Math.Clamp(config.TrailerCount, 0, MaxTrailers);
        if (trailerTarget > 0)
        {
            foreach (var trailer in SelectTrailers(item, config, library, trailerTarget))
            {
                result.Add(new IntroInfo { ItemId = trailer.Id, Path = trailer.Path });
            }
        }

        var preroll = SelectPreRoll(config, library);
        if (preroll is not null)
        {
            result.Add(new IntroInfo { ItemId = preroll.Id, Path = preroll.Path });
        }

        _logger.LogInformation(
            "PreRoll: providing {TrailerCount} trailer(s) and {PreRoll} preroll for {Feature}",
            result.Count - (preroll is null ? 0 : 1),
            preroll is null ? "no" : "1",
            item.Name);

        return result;
    }

    private IReadOnlyList<BaseItem> SelectTrailers(
        BaseItem feature,
        Configuration.PluginConfiguration config,
        ILibraryManager library,
        int target)
    {
        // Ids of the feature's own trailers, so we never play a trailer for the
        // movie the user just selected.
        var ownTrailerIds = feature.GetExtras()
            .Where(e => e.ExtraType == ExtraType.Trailer)
            .Select(e => e.Id)
            .ToHashSet();

        var candidates = new Dictionary<Guid, BaseItem>();

        if (config.UseNestedTrailers)
        {
            var trailers = library.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.Trailer },
                Recursive = true,
                IsVirtualItem = false,
            });

            foreach (var t in trailers)
            {
                AddCandidate(candidates, t, feature, ownTrailerIds);
            }
        }

        if (Guid.TryParse(config.TrailerLibraryId, out var trailerLibId) && !trailerLibId.Equals(Guid.Empty))
        {
            var libTrailers = library.GetItemList(new InternalItemsQuery
            {
                ParentId = trailerLibId,
                Recursive = true,
                IsVirtualItem = false,
            });

            foreach (var t in libTrailers)
            {
                AddCandidate(candidates, t, feature, ownTrailerIds);
            }
        }

        // Deterministic "no random substitution": shuffle the real candidates and
        // take up to the target. If fewer exist, we simply play fewer.
        return candidates.Values
            .OrderBy(_ => _random.Next())
            .Take(target)
            .ToList();
    }

    private static void AddCandidate(
        Dictionary<Guid, BaseItem> candidates,
        BaseItem trailer,
        BaseItem feature,
        HashSet<Guid> ownTrailerIds)
    {
        if (trailer.MediaType != MediaType.Video || string.IsNullOrEmpty(trailer.Path))
        {
            return;
        }

        if (ownTrailerIds.Contains(trailer.Id))
        {
            return;
        }

        // Safety net: drop anything whose name matches the selected feature, which
        // covers dedicated-library trailers that aren't linked as extras.
        if (!string.IsNullOrEmpty(feature.Name)
            && trailer.Name is not null
            && trailer.Name.Contains(feature.Name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        candidates[trailer.Id] = trailer;
    }

    private BaseItem? SelectPreRoll(Configuration.PluginConfiguration config, ILibraryManager library)
    {
        if (!Guid.TryParse(config.PreRollLibraryId, out var prerollLibId) || prerollLibId.Equals(Guid.Empty))
        {
            return null;
        }

        var prerolls = library.GetItemList(new InternalItemsQuery
        {
            ParentId = prerollLibId,
            Recursive = true,
            IsVirtualItem = false,
        })
            .Where(i => i.MediaType == MediaType.Video && !string.IsNullOrEmpty(i.Path))
            .ToList();

        if (prerolls.Count == 0)
        {
            return null;
        }

        return prerolls[_random.Next(prerolls.Count)];
    }
}
