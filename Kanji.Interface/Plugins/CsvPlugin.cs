using Kanji.Interface.Controls;
using Kanji.Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kanji.Interface.Plugins
{
    class CsvPlugin : Plugin
    {
        public override string Image => "/Data/UI/CsvIcon.png";

        public override string Description =>
@"Import SRS items from a CSV file.

Most softwares should be able to export data in CSV format.";

        public override Type ViewModel => typeof(CsvImportViewModel);

        public override (Type, Type)[] Steps { get; set; } = new (Type, Type)[]
        {
            (typeof(CsvImportFileStepViewModel), typeof(ImportCsvInitial)),
            (typeof(CsvImportColumnsStepViewModel), typeof(ImportCsvAfterLoad)),
        }; 

    }
}
