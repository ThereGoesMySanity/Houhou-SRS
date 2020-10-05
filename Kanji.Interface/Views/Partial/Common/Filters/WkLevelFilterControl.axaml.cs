using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GalaSoft.MvvmLight.Command;
using Kanji.Database.Dao;
using Kanji.Database.Entities;
using Kanji.Database.Models;
using Kanji.Interface.Converters;
using Kanji.Interface.Internationalization;

namespace Kanji.Interface.Controls
{
    public partial class WkLevelFilterControl : UserControl
    {
        public WkLevelFilterControl()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            LevelSlider = this.FindControl<Slider>("LevelSlider");
        }
        public Slider LevelSlider { get; private set; }
    }
}
