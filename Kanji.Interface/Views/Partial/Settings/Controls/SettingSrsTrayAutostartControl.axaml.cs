using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;







namespace Kanji.Interface.Controls
{
    public partial class SettingSrsTrayAutostartControl : SettingControl
    {
        public SettingSrsTrayAutostartControl()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
