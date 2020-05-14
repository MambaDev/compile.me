using Newtonsoft.Json;

namespace compile.me.api.Types
{
    public class CompileServiceConfiguration
    {
        /// <summary>
        /// Gets or sets the consumer url.
        /// </summary>
        [JsonProperty("consumer")]
        public string Consumer { get; set; }

        /// <summary>
        /// Gets or sets the publisher url.
        /// </summary>
        [JsonProperty("publisher")]
        public string Publisher { get; set; }

        /// <summary>
        /// Gets or sets the docker demon url.
        /// </summary>
        [JsonProperty("docker")]
        public string Docker { get; set; }
    }
}