﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using compile.me.api.Types;
using compile.me.api.Types.Events;
using compile.me.api.Types.Requests;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace compile.me.api.Services.Compiler
{
    public class MultipleTestSandboxWrapper
    {
        /// <summary>
        /// The logger
        /// </summary>
        private ILogger _logger;

        /// <summary>
        /// The related docker client used to communicate with docker for creating or updating of the containers.
        /// </summary>
        private readonly DockerClient _dockerClient;


        /// <summary>
        /// The compiler service that will update the sandboxes event stream.
        /// </summary>
        private CompilerService _compilerService;

        /// <summary>
        /// The request of the service.
        /// </summary>
        private CompileMultipleTestsSourceRequest _request;

        /// <summary>
        /// The resulting sandbox results for the given sandboxes.
        /// </summary>
        private List<CompileTestSourceResponse> _executedTestResults = new List<CompileTestSourceResponse>();

        /// <summary>
        /// The current executing test position, starting at base -1 since the next case and has next case
        /// will increment this number to ensure that its within the bounds of number of test cases.
        /// </summary>
        private int _currentTestPosition = -1;

        /// <summary>
        /// The compiler being used during the execution.
        /// </summary>
        private readonly Types.Compiler _compiler;

        /// <summary>
        /// Creates a new instance of the multiple test sandbox wrapper.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dockerClient"></param>
        /// <param name="compilerService"></param>
        /// <param name="request"></param>
        public MultipleTestSandboxWrapper(ILogger logger, DockerClient dockerClient, CompilerService compilerService,
            CompileMultipleTestsSourceRequest request, Types.Compiler compiler)
        {
            this._logger = logger;
            this._dockerClient = dockerClient;
            this._compilerService = compilerService;
            this._request = request;
            this._compiler = compiler;
        }

        /// <summary>
        ///  Start the multiple test case execution process. This will first check if there is a test to be executed
        /// and then execute the following test case. Otherwise if no tests are provided, the completion event will
        /// be raised.
        /// </summary>
        public async Task Start()
        {
            if (this.HashNext() && !this._request.RunAllParallel)
            {
                // Get the next sandbox, configured and setup ready with its following test case.
                var sandbox = this.Next();

                // Add the sandbox to its executing sandbox listings.
                this._compilerService.AddSandbox(sandbox);

                try
                {
                    // Run the sandbox.
                    this._logger.LogWarning($"'{this._request.Id}: executing first test case");
                    await sandbox.Run();
                }
                catch (Exception e)
                {
                    this.HandleSandboxFailedStartExecution(e);
                }
            }
            else if (this.HashNext() && this._request.RunAllParallel)
            {
                while (this.HashNext())
                {
                    var sandbox = this.Next();
                    this._compilerService.AddSandbox(sandbox);

                    try
                    {
                        // Run the sandbox.
                        this._logger.LogWarning($"'{this._request.Id}': executing sandbox in parallel.");
                        await sandbox.Run();

                        // Ensure to wait a tiny amount to stop the stream poole from being overloaded.
                        Thread.Sleep(50);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            else
            {
                this._logger.LogWarning(
                    $"Multiple test request: '{this._request.Id}' did not provide any test cases");
                this.RaiseCompletedChangeEvent();
            }
        }

        /// <summary>
        /// Gets the response of the given multiple sandbox execution.
        /// </summary>
        private CompileMultipleTestsSourceResponse GetResponse()
        {
            // If all tests have been completed, then return out, otherwise we are going to fulfil the complete
            // request count by marking them as not ran.
            if (this._executedTestResults.Count != this._request.TestCases.Count)
            {
                // fill out the remaining not executed test-cases as not ran.
                for (var i = this._executedTestResults.Count - 1; i < this._request.TestCases.Count - 1; i++)
                {
                    var test = this._request.TestCases[i];

                    var response = new CompileTestSourceResponse(this._request.Id, CompilerResult.Failed,
                        SandboxResponseStatus.Finished, new CompilerTestCaseResult(test.Id, CompilerTestResult.NotRan,
                            new List<string>(), new List<string>()));

                    this._executedTestResults.Add(response);
                }
            }

            var anyTestsFailed = this._executedTestResults.Any(e =>
                e.TestCaseResult.Result == CompilerTestResult.Failed || e.Result == CompilerResult.Failed);

            // get the last executed status that was not a test that did not ran. e.g what was the last failed results
            // reason and for one that did not also pass.
            var lastExecutedResult =
                this._executedTestResults.LastOrDefault(e =>
                    e.TestCaseResult.Result != CompilerTestResult.NotRan &&
                    e.Result != CompilerResult.Succeeded &&
                    e.Status != SandboxResponseStatus.Finished);

            var compiledResult = anyTestsFailed ? CompilerResult.Failed : CompilerResult.Succeeded;
            var compiledStatus = lastExecutedResult?.Status ?? SandboxResponseStatus.Finished;

            return new CompileMultipleTestsSourceResponse(this._request.Id, compiledResult,
                compiledStatus, this._executedTestResults);
        }

        /// <summary>
        /// Get the next sandbox for the following test case.
        /// </summary>
        /// <returns></returns>
        private SingleTestSandbox Next()
        {
            // increment the test position for the given request.
            this._currentTestPosition += 1;

            var test = this._request.TestCases[this._currentTestPosition];

            var sandbox = new SingleTestSandbox(this._logger, this._dockerClient,
                new CompileTestSourceRequest(this._request.Id, this._request.TimeoutSeconds,
                    this._request.MemoryConstraint, this._request.SourceCode, this._compiler.Language, test),
                this._compiler);

            sandbox.StatusChangeEvent += this.HandleSandboxStatusEvent;
            sandbox.CompletedEvent += this.SandboxOnCompletedEvent;

            return sandbox;
        }


        /// <summary>
        /// Determines if there is another sandbox in the queue or not.
        /// </summary>
        /// <returns></returns>
        private bool HashNext()
        {
            Trace.Assert(this._request.TestCases != null,
                "multiple test process cannot execute without tests set.");
            return this._request.TestCases.Count > this._currentTestPosition + 1;
        }

        /// <summary>
        /// Returns true if we can continue with the next sandbox, e.g the last sandbox request did actually
        /// pass its related tests or we are in run all run all mode.
        /// </summary>
        /// <returns></returns>
        private bool CanContinue()
        {
            return this._request.RunAll || this._request.RunAllParallel ||
                   this._executedTestResults.Last().TestCaseResult.Result == CompilerTestResult.Passed;
        }

        /// <summary>
        /// Handles the status event from the sandbox.
        /// </summary>
        private void HandleSandboxStatusEvent(object sender, SandboxStatusChangeEventArgs args)
        {
            if (args.Status != ContainerStatus.Removed) return;
        }

        private void SandboxOnCompletedEvent(object sender, CompileSingleTestSandboxCompletionEventArgs args)
        {
            // If the sandbox has been removed and thus completed, then lets go and handle the completion
            // for this selected test, process onto the following test or completing overall.
            var sandbox = (SingleTestSandbox) this._compilerService.GetSandboxByContainerId(args.ContainerId);
            this.HandleSandboxCompletion(sandbox, args.Response)
                .FireAndForgetSafeAsync(this.HandleSandboxCompletionFailedExecution);
        }

        /// <summary>
        /// Handles the case in which the sandbox has completed.
        /// </summary>
        private async Task HandleSandboxCompletion(SingleTestSandbox completedSandbox,
            CompileTestSourceResponse response)
        {
            Trace.Assert(completedSandbox != null, "completedSandbox must be defined to be handled.");

            // Get the response for the the executed single test case sandbox. Ensuring to state
            // that we don't want to clean up the path since this will be done when we are fully
            // complete. Since we dont know about the result as of yet and it could of failed and
            // thus want to cleanup.
            this._executedTestResults.Add(response);
            this._compilerService.RemoveSandbox(completedSandbox);

            this._logger.LogInformation(
                $"multiple test (single): {JsonConvert.SerializeObject(response.TestCaseResult)}");

            // If we have another test to be executed, go get the next sandbox and start the process.
            // Ensuring that we are waiting between at least  100 milliseconds between each execution.
            // just to not buffer out the event stream.
            // 
            // If ran in parallel, all containers have already been created, and thus this should not 
            // create anymore.
            if (this.HashNext() && this.CanContinue() && !this._request.RunAllParallel)
            {
                var sandbox = this.Next();
                this._compilerService.AddSandbox(sandbox);

                try
                {
                    this._logger.LogWarning($"'{this._request.Id}: executing test " +
                                            $"case: {this._currentTestPosition + 1}");
                    await sandbox.Run();
                }
                catch (Exception e)
                {
                    this.HandleSandboxFailedStartExecution(e);
                }
            }
            else if ((!this._request.RunAllParallel) ||
                     (!this.HashNext() && this._executedTestResults.Count == this._request.TestCases.Count))
            {
                // If we don't have any more sandboxes, then we must have completed all the tests and thus
                // can handle the completion of the entire process. Raising the event allows the parent
                // compiler service to get the response and publish the finalized results.
                this.RaiseCompletedChangeEvent();
            }
        }

        #region Events

        /// <summary>
        /// Handles the case in which the sandboxes could not be started / ran.
        /// </summary>
        /// <param name="exception">The exception that was caused.</param>
        private void HandleSandboxFailedStartExecution(Exception exception) =>
            this._logger.LogError($"Failed to start sandbox, error={exception}");

        /// <summary>
        /// Handles the case in which the sandbox failed the completion process.
        /// </summary>
        /// <param name="exception">The execution that was caused.</param>
        private void HandleSandboxCompletionFailedExecution(Exception exception) =>
            this._logger.LogError($"Failed to complete sandbox, error={exception}");

        /// <summary>
        /// The delegate event handler for the completed event.
        /// </summary>
        /// <param name="sender">The sending sandbox</param>
        /// <param name="args"></param>
        public delegate void CompletedEventHandler(object sender, MultipleSandboxCompletionEventArgs args);

        /// <summary>
        ///  The process has completed.
        /// </summary>
        public event CompletedEventHandler CompletedEvent;

        /// <summary>
        /// Raised when the multiple test case wrapper has completed all its tests or a test failed.
        /// </summary>
        private void RaiseCompletedChangeEvent() => this.CompletedEvent?.Invoke(this,
            new MultipleSandboxCompletionEventArgs {Id = this._request.Id, Response = this.GetResponse()});

        #endregion
    }
}