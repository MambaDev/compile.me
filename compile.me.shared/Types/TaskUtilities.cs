using System;
using System.Threading.Tasks;

namespace Compile.Me.Shared.Types
{
    /// <summary>
    /// Error handler for async fire and forget processes.
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// Handles the error.
        /// </summary>
        /// <param name="ex">The ex.</param>
        void HandleError(Exception ex);
    }

    /// <summary>
    /// Task Utilities provide functionality to help with the creation of async functionality through out the code base.
    /// </summary>
    public static class TaskUtilities
    {
        /// <summary>
        /// Fires the and forget safe asynchronous.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="action">The handler.</param>
        public static async void FireAndForgetSafeAsync(this Task task, Action<Exception> action = null)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                action?.Invoke(ex);
            }
        }
    }
}