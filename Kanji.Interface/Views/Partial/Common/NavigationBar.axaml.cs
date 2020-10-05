using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;










namespace Kanji.Interface.Controls
{
    public partial class NavigationBar : UserControl
    {
        public NavigationBar()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            NavigationPanelToolbar = this.FindControl<Grid>("NavigationPanelToolbar");
            SrsTab = this.FindControl<Grid>("SrsTab");
            KanjiTab = this.FindControl<Grid>("KanjiTab");
            VocabTab = this.FindControl<Grid>("VocabTab");
            SettingsTab = this.FindControl<Grid>("SettingsTab");
        }
        public Grid NavigationPanelToolbar { get; private set; }
        public Grid SrsTab { get; private set; }
        public Grid KanjiTab { get; private set; }
        public Grid VocabTab { get; private set; }
        public Grid SettingsTab { get; private set; }
    }
}
