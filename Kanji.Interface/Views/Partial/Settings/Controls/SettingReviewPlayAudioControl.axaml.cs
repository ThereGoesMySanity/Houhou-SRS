﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;







using Avalonia.Controls;
using Avalonia.Markup.Xaml;



namespace Kanji.Interface.Controls
{
    public partial class SettingReviewPlayAudioControl : SettingControl
    {
        public SettingReviewPlayAudioControl()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
