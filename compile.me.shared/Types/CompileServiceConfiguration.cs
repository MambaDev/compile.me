namespace Compile.Me.Shared.Types
{
    public class CompileServiceConfiguration
    {
        /// <summary>
        /// Gets or sets the consumer url.
        /// </summary>
        public string Consumer { get; set; }

        /// <summary>
        /// Gets or sets the publisher url.
        /// </summary>
        public string Publisher { get; set; }

        /// <summary>
        /// Gets or sets the docker demon url.
        /// </summary>
        public string Docker { get; set; }
    }
}