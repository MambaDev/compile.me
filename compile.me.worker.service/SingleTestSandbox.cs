using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Compile.Me.Shared.Types;
using Compile.Me.Worker.Service.Events;
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
        private readonly SandboxSingleTestCreationRequest _singleRequest;

        public SingleTestSandbox(ILogger logger, DockerClient dockerClient,
            SandboxSingleTestCreationRequest singleTestCreationRequest) : base(logger, dockerClient,
            singleTestCreationRequest)
        {
            this._singleRequest = singleTestCreationRequest;

            // Raise our completion event when the base is raised.
            base.CompletedEvent += this.HandleBaseSandboxCompletionEvent;
        }

        /// <summary>
        /// Handles the base event from the under lining compile and raises the test simple completion event.
        /// With the related results.
        /// </summary>
        private void HandleBaseSandboxCompletionEvent(object sender, CompileSandboxCompletionEventArgs args)
        {
            this.RaiseCompletedChangeEvent(this.GetResponse(args.Response));
        }

        /// <inheritdoc cref="CompileSandbox.GetResponse"/>
        /// <summary>
        ///  Gets the standard response including the test result.
        /// </summary>
        private new SandboxSingleTestResponse GetResponse(SandboxCompileResponse baseResponse)
        {
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
                Id = this._singleRequest.TestCase.Id,
                StandardOutput = standardOut,
                StandardErrorOutput = errorOut,
            };

            // Mark that no tests have been run since no tests where provided.
            if (this._singleRequest.TestCase == null) return null;

            // If we did not get the same amount of output expected as the test cases
            // then we don't need to check since it has already failed.
            // -1 since we echo out the end of the line.
            if (standardOut.Count - 1 != this._singleRequest.TestCase.ExpectedStandardDataOut.Count)
            {
                response.Result = CompilerTestResult.Failed;
                return response;
            }

            // ensure that all entry points, match the given standard out entries.
            // performed on the test cases and not the output to ensure we don't check against
            // the final line of the standard output.
            var passState = this._singleRequest.TestCase.ExpectedStandardDataOut
                .Where((t, i) => !t.Equals(standardOut[i])).Any()
                ? CompilerTestResult.Failed
                : CompilerTestResult.Passed;

            response.Result = passState;
            return response;
        }

        #region Events

        /// <summary>
        /// The delegate event handler for the completed event.
        /// </summary>
        /// <param name="sender">The sending sandbox</param>
        /// <param name="args"></param>
        public delegate void CompletedEventHandler(object sender, CompileSingleTestSandboxCompletionEventArgs args);

        /// <summary>
        ///  The process has completed.
        /// </summary>
        public new event CompletedEventHandler CompletedEvent;

        /// <summary>
        /// Raised when the multiple test case wrapper has completed all its tests or a test failed.
        /// </summary>
        private void RaiseCompletedChangeEvent(SandboxSingleTestResponse response) => this.CompletedEvent?.Invoke(this,
            new CompileSingleTestSandboxCompletionEventArgs()
            {
                Id = this._singleRequest.Id, ContainerId = this.ContainerId,
                Response = response,
            });

        #endregion
    }
}