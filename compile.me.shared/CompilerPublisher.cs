using System.Threading.Tasks;
using Compile.Me.Shared.Modals;
using Newtonsoft.Json;
using PureNSQSharp;

namespace Compile.Me.Shared
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

            if (!string.IsNullOrWhiteSpace(this._address)) this.Connect();
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

        /// <summary>
        /// Publishes a request to compile and execute the code.
        /// A standard compile process.
        /// </summary>
        /// <param name="request">The request that would be compiled.</param>
        public async Task PublishCompileSourceRequest(CompileSourceRequest request)
        {
            await this._producer.PublishAsync("compiling", JsonConvert.SerializeObject(request));
        }
        
        

        /// <summary>
        /// Publishes a response to a compile request.
        /// </summary>
        /// <param name="response">The response that has been compiled.</param>
        public async Task PublishCompileSourceResponse(CompileSourceResponse response)
        {
            await this._producer.PublishAsync("compiled", JsonConvert.SerializeObject(response));
        }
    }
}