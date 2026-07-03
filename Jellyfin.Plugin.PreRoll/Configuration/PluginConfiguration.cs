using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.PreRoll.Configuration;

/// <summary>
/// Plugin configuration. Deliberately minimal and deterministic.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether the plugin provides intros at all.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether intros are only provided for movies.
    /// When true, TV episodes and everything else are skipped.
    /// </summary>
    public bool MoviesOnly { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of trailers to play (clamped 0-2 at runtime).
    /// </summary>
    public int TrailerCount { get; set; } = 2;

    /// <summary>
    /// Gets or sets a value indicating whether to source trailers from nested
    /// per-movie local trailers (belonging to other movies).
    /// </summary>
    public bool UseNestedTrailers { get; set; } = true;

    /// <summary>
    /// Gets or sets the id (GUID string) of an optional dedicated trailers library.
    /// Empty means "not configured".
    /// </summary>
    public string TrailerLibraryId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the id (GUID string) of the preroll library (Movies content type).
    /// Empty means no preroll is played.
    /// </summary>
    public string PreRollLibraryId { get; set; } = string.Empty;
}
