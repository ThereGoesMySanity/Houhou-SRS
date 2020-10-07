using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Kanji.Interface.Controls;
using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Views
{
    public partial class SrsPage : UserControl
    {
        #region Constructors

        public SrsPage()
        {
            InitializeComponent();
            DataContext = new SrsViewModel();
            this.GetObservable(IsVisibleProperty).Subscribe(OnIsVisibleChanged);
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            FilterControl = this.FindControl<SrsEntryFilterControl>("FilterControl");
            Navigation = this.FindControl<SrsPageNavigationControl>("Navigation");
            ReviewControl = this.FindControl<SrsReviewControl>("ReviewControl");
        }
        public SrsEntryFilterControl FilterControl { get; private set; }
        public SrsPageNavigationControl Navigation { get; private set; }
        public SrsReviewControl ReviewControl { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Since a <see cref="GalaSoft.MvvmLight.Command.RelayCommand"/> does not accept keyboard shortcuts,
        /// we have to manually invoke the commands on a keyboard event.
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            SrsViewModel viewModel = ((SrsViewModel)DataContext);

            switch (viewModel.ControlMode)
            {
                case SrsViewModel.ControlModeEnum.Dashboard:
                    HandleDashboardInput(e);
                    break;
                case SrsViewModel.ControlModeEnum.SimpleFilter:
                    HandleSimpleFilterInput(e);
                    break;
            }

            HandleSharedInput(e);
        }

        private void HandleDashboardInput(KeyEventArgs e)
        {
            SrsViewModel viewModel = ((SrsViewModel)DataContext);
            bool isCtrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);

            switch (e.Key)
            {
                case Key.Enter:
                    if (isCtrlDown)
                        viewModel.StartReviewsCommand.Execute(null);
                    break;
                case Key.B:
                    if (isCtrlDown)
                    {
                        viewModel.SwitchToSimpleFilterCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void HandleSimpleFilterInput(KeyEventArgs e)
        {

            SrsViewModel viewModel = ((SrsViewModel)DataContext);
            bool isCtrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool isAltDown = e.KeyModifiers.HasFlag(KeyModifiers.Alt);

            switch (e.Key)
            {
                case Key.Home:
                    // Apparently, the CommandTextBox eats CTRL+Home, so we have to add an Alt.
                    if (isCtrlDown && isAltDown)
                    {
                        viewModel.SwitchToDashboardCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;
                case Key.F5:
                    viewModel.FilterVm.RefreshCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.A:
                    // We can't just use CTRL+A here, because that would not work if a text box had focus.
                    if (isCtrlDown && isAltDown)
                    {
                        viewModel.FilterVm.BrowseAllItemsCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;
                case Key.M:
                {
                    if (isCtrlDown)
                    {
                        //from https://github.com/AvaloniaUI/Avalonia/issues/2505
                        var filterTextBox =
                            ((IControl)FilterControl.MeaningFilter.GetVisualChildren().FirstOrDefault())?.FindControl<CommandTextBox>("FilterTextBox");

                        filterTextBox.Focus();
                        e.Handled = true;
					}
                    break;
                }
                case Key.R:
                {
                    if (isCtrlDown)
                    {
                        var filterTextBox =
                            ((IControl)FilterControl.ReadingFilter.GetVisualChildren().FirstOrDefault())?.FindControl<CommandTextBox>("FilterTextBox");
                        filterTextBox.Focus();
                        e.Handled = true;
					}
                    break;
                }
                case Key.T:
                {
                    if (isCtrlDown)
                    {
                        var filterTextBox =
                            ((IControl)FilterControl.TagFilter.GetVisualChildren().FirstOrDefault())?.FindControl<CommandTextBox>("FilterTextBox");

                        filterTextBox.Focus();
                        e.Handled = true;
					}
                    break;
                }
                case Key.K:
                    if (isCtrlDown && isAltDown)
                    {
                        viewModel.FilterVm.TypeFilterVm.IsKanjiItemEnabled = !viewModel.FilterVm.TypeFilterVm.IsKanjiItemEnabled;
                    }
                    break;
                case Key.V:
                    if (isCtrlDown && isAltDown)
                    {
                        viewModel.FilterVm.TypeFilterVm.IsVocabItemEnabled = !viewModel.FilterVm.TypeFilterVm.IsVocabItemEnabled;
                    }
                    break;
            }
        }

        private void HandleSharedInput(KeyEventArgs e)
        {

            SrsViewModel viewModel = ((SrsViewModel)DataContext);
            bool isCtrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool isAltDown = e.KeyModifiers.HasFlag(KeyModifiers.Alt);

            switch (e.Key)
            {
                case Key.K:
                    if (isCtrlDown && !isAltDown)
                        viewModel.AddKanjiItemCommand.Execute(null);
                    break;
                case Key.V:
                    if (isCtrlDown && !isAltDown)
                        viewModel.AddVocabItemCommand.Execute(null);
                    break;
                case Key.I:
                    if (isCtrlDown)
                        viewModel.ImportCommand.Execute(null);
                    break;
            }
        }

        private void OnIsVisibleChanged(bool newValue)
        {
            // Focus the page once it becomes visible.
            // This is so that the navigation bar does not keep the focus, which would prevent shortcut keys from working.
            if (newValue)
            {
                Focus();
            }
        }

        #endregion
    }
}
