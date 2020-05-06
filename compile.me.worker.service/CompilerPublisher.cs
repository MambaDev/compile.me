using PureNSQSharp;

namespace Compile.Me.Worker.Service
{
    public class CompilerPublisher
    {
        /// <summary>
        /// The producer
        /// </summary>
        private Producer _producer;

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        private string _address;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerPublisher"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        public CompilerPublisher(string address)
        {
            this._address = address;

            // if (!string.IsNullOrWhiteSpace(this._address)) this.Connect();
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        private void Connect()
        {
            this._producer = new Producer(this._address, new Config() { });
            this._producer.Connect();
        }

        /// <summary>
        /// Updates the address.
        /// </summary>
        /// <param name="address">The address.</param>
        public void UpdateAddress(string address)
        {
            this._address = address;

            if (!string.IsNullOrWhiteSpace(this._address))
                this.Connect();
        }
    }
}