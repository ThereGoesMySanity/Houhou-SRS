using Kanji.Interface.Controls;
using Kanji.Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kanji.Interface.Plugins
{
    class WkPlugin : Plugin
    {
        public override string Image => "/Data/UI/WaniKaniIcon.png";

        public override string Description =>
@"Import items from your WaniKani account.

WaniKani is a kanji learning web application made by Tofugu.";

        public override Type ViewModel => typeof(WkImportViewModel);

        public override (Type ViewModel, Type View)[] Steps { get; set; } = new (Type, Type)[]
        {
            (typeof(WkImportSettingsViewModel), typeof(ImportWkInitial)),
            (typeof(WkImportRequestViewModel), typeof(ImportWkRequestView)),
        };
    }
}
