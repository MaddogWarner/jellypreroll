using System;
using System.Collections.Generic;
using Jellyfin.Plugin.PreRoll.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.PreRoll;

/// <summary>
/// The PreRoll and Trailers plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        ILibraryManager libraryManager)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        LibraryManager = libraryManager;
    }

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <summary>
    /// Gets the library manager. Intro providers are constructed by Jellyfin's DI
    /// separately from the plugin, so the provider reaches the library through here.
    /// </summary>
    public static ILibraryManager? LibraryManager { get; private set; }

    /// <inheritdoc />
    public override string Name => "PreRoll & Trailers";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("eaabce77-c959-4d03-aed2-8fd17fad17d0");

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{0}.Configuration.configPage.html",
                    GetType().Namespace),
            },
        };
    }
}
