using Kanji.Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kanji.Interface.Plugins
{
    public abstract class Plugin
    {
        public abstract string Image { get; }
        public abstract string Description { get; }
        public abstract Type ViewModel { get; }
        internal ImportModeViewModel ViewModelInstance { get; set; }
        public ImportModeViewModel InstantiateViewModel()
        {
            ViewModelInstance = (ImportModeViewModel)Activator.CreateInstance(ViewModel);
            return ViewModelInstance;
        }
        public abstract (Type ViewModel, Type View)[] Steps { get; set; }
    }
}
