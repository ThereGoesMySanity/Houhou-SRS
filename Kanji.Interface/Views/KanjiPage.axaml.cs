using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Kanji.Interface.Controls;
using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Views
{
    public partial class KanjiPage : UserControl
    {
        #region Constructors

        public KanjiPage()
        {
            InitializeComponent();
            DataContext = new KanjiViewModel();
            this.GetObservable(IsVisibleProperty).Subscribe(OnIsVisibleChanged);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            KanjiDetailsControl = this.FindControl<KanjiDetails>("KanjiDetailsControl");
            KanjiFilterControl = this.FindControl<KanjiFilterControl>("KanjiFilterControl");
            ClearFilterButton = this.FindControl<Button>("ClearFilterButton");
            ApplyFilterButton = this.FindControl<Button>("ApplyFilterButton");
            CloseKanjiPageButton = this.FindControl<Button>("CloseKanjiPageButton");
        }
        #endregion

        #region Properties

        public KanjiDetails KanjiDetailsControl { get; private set; }
        public KanjiFilterControl KanjiFilterControl { get; private set; }
        public Button ClearFilterButton { get; private set; }
        public Button ApplyFilterButton { get; private set; }
        public Button CloseKanjiPageButton { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Since a <see cref="GalaSoft.MvvmLight.Command.RelayCommand"/> does not accept keyboard shortcuts,
        /// we have to manually invoke the commands on a keyboard event.
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (KanjiDetailsControl.IsVisible)
                return;

            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                switch (e.Key)
                {
                    case Key.R:
                        KanjiFilterControl.RadicalNameFilter.Focus();
                        e.Handled = true;
                        break;
                    case Key.W:
                        KanjiFilterControl.WkLevelFilter.LevelSlider.Focus();
                        e.Handled = true;
                        break;
                    case Key.J:
                        KanjiFilterControl.JlptLevelFilter.LevelSlider.Focus();
                        e.Handled = true;
                        break;
                    case Key.T:
                        KanjiFilterControl.TextFilter.Focus();
                        e.Handled = true;
                        break;
                    case Key.F:
                        KanjiFilterControl.Filter.Focus();
                        e.Handled = true;
                        break;
                    case Key.C:
                        if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                            break;
                        ClearFilterButton.Command.Execute(null);
                        e.Handled = true;
                        break;
                }
            }

            switch (e.Key)
            {
                case Key.Enter:
                    ApplyFilterButton.Command.Execute(null);
                    e.Handled = true;
                    break;
            }
        }

        private void OnIsVisibleChanged(bool obj)
        {
            // Focus the page once it becomes visible.
            // This is so that the navigation bar does not keep the focus, which would prevent shortcut keys from working.
            if (obj)
            {
                Focus();
            }
        }

        #endregion
    }
}
