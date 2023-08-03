using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kanji.Interface.Utilities;

namespace Kanji.Interface.ViewModels
{
    public class ViewModel : NotifyPropertyChanged, IAsyncDisposable
    {
        public virtual ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
