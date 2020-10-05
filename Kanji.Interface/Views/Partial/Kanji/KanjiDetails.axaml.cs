using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;






using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Kanji.Interface.Models;

namespace Kanji.Interface.Controls
{
    public partial class KanjiDetails : UserControl
    {
        #region Constructors

        public KanjiDetails()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            VocabFilter = this.FindControl<VocabFilterControl>("VocabFilter");
            VocabList = this.FindControl<VocabList>("VocabList");
        }

        #endregion

        #region Properties
        public VocabFilterControl VocabFilter { get; private set; }
        public VocabList VocabList { get; private set; }

        #endregion

        #region Methods

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
                    case Key.R:
                        VocabFilter.ReadingFilter.Focus();
                        e.Handled = true;
                        break;
                    case Key.M:
                        VocabFilter.MeaningFilter.Focus();
                        e.Handled = true;
                        break;
                    case Key.W:
                        VocabFilter.WkLevelFilter.LevelSlider.Focus();
                        e.Handled = true;
                        break;
                    case Key.J:
                        VocabFilter.JlptLevelFilter.LevelSlider.Focus();
                        e.Handled = true;
                        break;
                    case Key.C:
                        // We can't just use CTRL+C here, because that would not work if a text box had focus.
                        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
                        {
                            VocabFilter.CategoryFilter.ComboBox.Focus();
                            e.Handled = true;
                        }
                        break;
                }
            }

            switch (e.Key)
            {
                case Key.Enter:
                    VocabFilter.ApplyFilterButton.Command.Execute(null);
                    e.Handled = true;
                    break;
            }
        }

        #endregion
    }
}
