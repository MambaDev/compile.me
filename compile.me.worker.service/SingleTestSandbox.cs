using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Compile.Me.Shared.Types;
using Compile.Me.Worker.Service.Types.Compile;
using Compile.Me.Worker.Service.Types.SingleTest;
using Docker.DotNet;
using Microsoft.Extensions.Logging;

namespace Compile.Me.Worker.Service
{
    public class SingleTestSandbox : CompileSandbox
    {
        /// <summary>
        /// The request that will be processed for the given sandbox. Including all the required code and compiler
        /// information that will be used.
        /// </summary>
        private new readonly SandboxSingleTestCreationRequest _sandboxCompileRequest;

        public SingleTestSandbox(ILogger<CompilerService> logger, DockerClient dockerClient,
            SandboxSingleTestCreationRequest sandboxSingleTestCreationRequest) : base(logger, dockerClient,
            sandboxSingleTestCreationRequest)
        {
            this._sandboxCompileRequest = sandboxSingleTestCreationRequest;
        }

        /// <inheritdoc cref="CompileSandbox.GetResponse"/>
        /// <summary>
        ///  Gets the standard response including the test result.
        /// </summary>
        public new async Task<SandboxSingleTestResponse> GetResponse()
        {
            var baseResponse = await base.GetResponse();

            var singleTestResponse = SandboxSingleTestResponse.FromSandboxCompileResponse(
                baseResponse, this.GetTestCaseResponse());

            return singleTestResponse;
        }

        /// <summary>
        /// Performs the checks to ensure that the given tests have passed or failed, returning the status.
        /// </summary>
        private CompilerTestCaseResult GetTestCaseResponse()
        {
            Trace.Assert(this.SandboxCompileResponse.StandardOutput != null);

            var response = new CompilerTestCaseResult
            {
                Id = this._sandboxCompileRequest.TestCase.Id,
                StandardOutput = this.SandboxCompileResponse.StandardOutput,
                StandardErrorOutput = this.SandboxCompileResponse.StandardErrorOutput,
            };

            // Mark that no tests have been run since no tests where provided.
            if (this._sandboxCompileRequest.TestCase == null) return null;

            // If we did not get the same amount of output expected as the test cases
            // then we don't need to check since it has already failed.
            // -1 since we echo out the end of the line.
            if (this.SandboxCompileResponse.StandardOutput.Count - 1 !=
                this._sandboxCompileRequest.TestCase.ExpectedStandardDataOut.Count)
            {
                response.Result = CompilerTestResult.Failed;
                return response;
            }

            // ensure that all entry points, match the given standard out entries.
            // performed on the test cases and not the output to ensure we don't check against
            // the final line of the standard output.
            var passState = this._sandboxCompileRequest.TestCase.ExpectedStandardDataOut
                .Where((t, i) => !t.Equals(this.SandboxCompileResponse.StandardOutput[i])).Any()
                ? CompilerTestResult.Failed
                : CompilerTestResult.Passed;

            response.Result = passState;
            return response;
        }
    }
}