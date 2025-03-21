using System;
using Avalonia.Threading;
using Kanji.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace Kanji.Interface.Helpers
{
    public static class DispatcherHelper
    {
        private static readonly string LogName = "DispatcherHelper";
        private static readonly string LogEntryFormat = "Could not execute an action on the dispatcher.";

        /// <summary>
        /// Safely invokes the given synchronous action on the dispatcher.
        /// </summary>
        /// <param name="action">Action to invoke.</param>
        public static void Invoke(Action action)
        {
            if (Dispatcher.UIThread.CheckAccess())
                action.Invoke();
            else
                Dispatcher.UIThread.InvokeAsync(action).Wait();
        }

        /// <summary>
        /// Safely invokes the given synchronous action on the dispatcher.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="action">Function returning a value of type T.</param>
        /// <returns>The value returned by the function,
        /// or the default value of T if the call fails.</returns>
        public static T Invoke<T>(Func<T> action)
        {
            try
            {
                return Dispatcher.UIThread.InvokeAsync<T>(action).Result;
            }
            catch (Exception ex)
            {
                LogHelper.Factory.CreateLogger(LogName).LogWarning(ex, LogEntryFormat);
                return default;
            }
        }

        /// <summary>
        /// Safely invokes the given asynchronous action on the dispatcher.
        /// </summary>
        /// <param name="action">Action to invoke.</param>
        public static void InvokeAsync(Action action)
        {
            try
            {
                Dispatcher.UIThread.InvokeAsync(action);
            }
            catch (Exception ex)
            {
                LogHelper.Factory.CreateLogger(LogName).LogWarning(ex, LogEntryFormat);
            }
        }
    }
}
