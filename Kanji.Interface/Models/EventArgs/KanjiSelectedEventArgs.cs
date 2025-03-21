﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanji.Interface.Models
{
    public class KanjiSelectedEventArgs : EventArgs
    {
        public ExtendedKanji SelectedKanji { get; set; }

        public KanjiSelectedEventArgs(ExtendedKanji selectedKanji)
        {
            SelectedKanji = selectedKanji;
        }
    }
}
