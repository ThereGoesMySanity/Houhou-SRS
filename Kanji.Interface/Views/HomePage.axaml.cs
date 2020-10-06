using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Views
{
    public partial class HomePage : UserControl
    {
        #region Constructors

        public HomePage()
        {
            InitializeComponent();
            DataContext = new HomeViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        #endregion

        #region Methods
        
        //TODO
        //private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        //{
        //    System.Diagnostics.Process.Start(e.Uri.ToString());
        //}

        /// <summary>
        /// Since a <see cref="GalaSoft.MvvmLight.Command.RelayCommand"/> does not accept keyboard shortcuts,
        /// we have to manually invoke the commands on a keyboard event.
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                switch (e.Key)
                {

                }
            }
        }

        private void OnIsVisibleChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            // Focus the page once it becomes visible.
            // This is so that the navigation bar does not keep the focus, which would prevent shortcut keys from working.
            if (((bool)e.NewValue))
            {
                Focus();
            }
        }

        #endregion
    }
}
