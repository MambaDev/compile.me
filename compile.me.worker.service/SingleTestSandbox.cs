using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Compile.Me.Shared.Types;
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
        private readonly SandboxSingleTestCreationRequest _sandboxSingleTestCompileRequest;

        public SingleTestSandbox(ILogger logger, DockerClient dockerClient,
            SandboxSingleTestCreationRequest sandboxSingleTestCreationRequest) : base(logger, dockerClient,
            sandboxSingleTestCreationRequest)
        {
            this._sandboxSingleTestCompileRequest = sandboxSingleTestCreationRequest;
        }

        /// <inheritdoc cref="CompileSandbox.GetResponse"/>
        /// <summary>
        ///  Gets the standard response including the test result.
        /// </summary>
        public new SandboxSingleTestResponse GetResponse()
        {
            var baseResponse = base.GetResponse();
            var testResult = this.GetTestCaseResponse(baseResponse.StandardOutput, baseResponse.StandardErrorOutput);
            var singleTestResponse = SandboxSingleTestResponse.FromSandboxCompileResponse(baseResponse, testResult);

            return singleTestResponse;
        }

        /// <summary>
        /// Performs the checks to ensure that the given tests have passed or failed, returning the status.
        /// </summary>
        private CompilerTestCaseResult GetTestCaseResponse(IReadOnlyList<string> standardOut,
            IReadOnlyList<string> errorOut)
        {
            Trace.Assert(standardOut != null);

            var response = new CompilerTestCaseResult
            {
                Id = this._sandboxSingleTestCompileRequest.TestCase.Id,
                StandardOutput = standardOut,
                StandardErrorOutput = errorOut,
            };

            // Mark that no tests have been run since no tests where provided.
            if (this._sandboxSingleTestCompileRequest.TestCase == null) return null;

            // If we did not get the same amount of output expected as the test cases
            // then we don't need to check since it has already failed.
            // -1 since we echo out the end of the line.
            if (standardOut.Count - 1 != this._sandboxSingleTestCompileRequest.TestCase.ExpectedStandardDataOut.Count)
            {
                response.Result = CompilerTestResult.Failed;
                return response;
            }

            // ensure that all entry points, match the given standard out entries.
            // performed on the test cases and not the output to ensure we don't check against
            // the final line of the standard output.
            var passState = this._sandboxSingleTestCompileRequest.TestCase.ExpectedStandardDataOut
                .Where((t, i) => !t.Equals(standardOut[i])).Any()
                ? CompilerTestResult.Failed
                : CompilerTestResult.Passed;

            response.Result = passState;
            return response;
        }
    }
}