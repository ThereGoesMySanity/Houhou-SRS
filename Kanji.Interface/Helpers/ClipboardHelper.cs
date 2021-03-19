using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanji.Interface.Helpers
{
    static class ClipboardHelper
    {
        /// <summary>
        /// Attempts to copy the given string to the clipboard.
        /// Returns a value indicating whether the operation succeeded or failed.
        /// </summary>
        /// <param name="value">Value to copy to the clipboard.</param>
        /// <returns>True if the operation succeeded. False otherwise.</returns>
        public static async Task<bool> SetText(string value)
        {
            await Avalonia.Application.Current.Clipboard.SetTextAsync(value);
            return true;
        }
    }
}
