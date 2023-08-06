using System;
using Kanji.Interface.Utilities;

namespace Kanji.Interface.ViewModels
{
    public class ViewModel : NotifyPropertyChanged, IDisposable
    {
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
