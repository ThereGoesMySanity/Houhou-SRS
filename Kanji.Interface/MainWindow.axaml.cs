using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Kanji.Common.Helpers;
using Kanji.Interface.Actors;
using Kanji.Interface.Business;
using Kanji.Interface.Models;
using Kanji.Interface.ViewModels;
using Kanji.Interface.Views;

namespace Kanji.Interface
{
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            // Initialize the components.
            InitializeComponent();
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        #endregion

        #region Methods

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            NavigationActor.Instance.SendMainWindowCloseEvent();
        }

        /// <summary>
        /// Since a <see cref="GalaSoft.MvvmLight.Command.RelayCommand"/> does not accept keyboard shortcuts,
        /// we have to manually invoke the commands on a keyboard event.
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                // Use CTRL+Numbers (starting at 1) to navigate to the respective tabs.

                NavigationPageEnum? navigationTarget = null;

                switch (e.Key)
                {
                    case Key.D1:
                    case Key.NumPad1:
                        navigationTarget = NavigationPageEnum.Home;
                        break;
                    case Key.D2:
                    case Key.NumPad2:
                        navigationTarget = NavigationPageEnum.Srs;
                        break;
                    case Key.D3:
                    case Key.NumPad3:
                        navigationTarget = NavigationPageEnum.Kanji;
                        break;
                    case Key.D4:
                    case Key.NumPad4:
                        navigationTarget = NavigationPageEnum.Vocab;
                        break;
                    case Key.D5:
                    case Key.NumPad5:
                        navigationTarget = NavigationPageEnum.Settings;
                        break;
                }

                if (navigationTarget.HasValue)
                {
                    NavigableViewModel navModel = (NavigableViewModel)this.Find<HomePage>("HomePage").DataContext;
                    navModel.NavigateCommand.Execute(navigationTarget.Value);
                    e.Handled = true;
                }
            }
        }

        #endregion
    }
}
