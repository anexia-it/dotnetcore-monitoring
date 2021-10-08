using System;
using Newtonsoft.Json;

namespace Anexia.Monitoring.Models
{
    /// <summary>
    ///     Dto for a single json array object returned by netcore release page
    /// </summary>
    internal class ReleasesJson
    {
        /// <summary>
        ///     Gets or sets the release's main version number
        /// </summary>
        [JsonProperty(PropertyName = "channel-version")]
        public string MainVersion { get; set; }

        /// <summary>
        ///     Gets or sets the release's product name
        /// </summary>
        [JsonProperty(PropertyName = "product")]
        public string Product { get; set; }

        /// <summary>
        ///     Gets or sets the release's date
        /// </summary>
        [JsonProperty(PropertyName = "latest-release-date")]
        public DateTime LatestReleaseDate { get; set; }

        /// <summary>
        ///     Gets or sets the release's support phase
        /// </summary>
        [JsonProperty(PropertyName = "support-phase")]
        public string SupportPhase { get; set; }
    }
}