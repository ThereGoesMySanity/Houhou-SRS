using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Kanji.Interface.Actors;

namespace Kanji.Interface.Helpers
{
    static class ClipboardHelper
    {
        /// <summary>
        /// Attempts to copy the given string to the clipboard.
        /// Returns a value indicating whether the operation succeeded or failed.
        /// </summary>
        /// <param name="value">Value to copy to the clipboard.</param>
        public static async Task SetText(string value)
        {
            await NavigationActor.Instance.MainWindow?.Clipboard.SetTextAsync(value);
        }
    }
}
