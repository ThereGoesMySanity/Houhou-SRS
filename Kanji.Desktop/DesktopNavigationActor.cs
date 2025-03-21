using System;
using System.Threading.Tasks;

using Kanji.Interface.Helpers;
using Kanji.Interface.Models;
using Kanji.Interface.Utilities;
using Kanji.Interface.ViewModels;
using Kanji.Interface.Views;
using Kanji.Interface.Extensions;
using Avalonia.Controls;
using Kanji.Database.Entities;
using Kanji.Interface.Business;
using Avalonia.Controls.ApplicationLifetimes;

namespace Kanji.Interface.Actors
{
    class DesktopNavigationActor : NavigationActor
    {
        #region Fields

        private object _mainWindowLock;

        #endregion

        #region Constructors

        public DesktopNavigationActor()
        {
            _mainWindowLock = new object();
        }

        #endregion

        #region Methods

        #region Public
        public override async Task<SrsEntryEditedEventArgs> OpenSrsEditWindow(SrsEntry entry)
        {
            EditSrsEntryWindow wnd = new EditSrsEntryWindow(entry.Clone());
            await wnd.ShowDialog(MainWindow as Window);

            // When it is closed, get the result.
            return wnd.Result ?? new SrsEntryEditedEventArgs(new ExtendedSrsEntry(entry), false);
        }

        public override async Task<SrsReviewViewModel> OpenReviewSession()
        {
            if (SrsVm.ReviewVm == null)
            {
                SrsVm.ReviewVm = new SrsReviewViewModel();
                await SrsVm.ReviewVm.StartSession();
            }
            return SrsVm.ReviewVm;
        }

        public override void CreateMainWindow()
        {
            var window = new MainWindow();
            MainWindow = window;
            ActiveWindow = MainWindow;
            window.Closed += OnMainWindowClosed;
            window.Closed += TrayBusiness.Instance.OnMainWindowClosed;
            (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow ??= window;
        }

        /// <summary>
        /// Opens the Main Window.
        /// </summary>
        public override void OpenMainWindow()
        {
            lock (_mainWindowLock)
            {
                CurrentPage = Properties.UserSettings.Instance.StartPage.ToNavigationPage();
                DoOpenWindow();
            }
        }

        /// <summary>
        /// Closes the Main Window.
        /// </summary>
        public void CloseMainWindow()
        {
            var MainWindow = this.MainWindow as MainWindow;
            lock (_mainWindowLock)
            {
                MainWindow?.Close();
            }
        }

        /// <summary>
        /// Opens the main window. If it is already open, focuses it.
        /// </summary>
        public override void OpenOrFocus()
        {
            var MainWindow = this.MainWindow as MainWindow;
            lock (_mainWindowLock)
            {
                if (MainWindow == null)
                {
                    DoOpenWindow();
                }
                else
                {
                    DispatcherHelper.Invoke(() =>
                    {
                        if (MainWindow.WindowState == WindowState.Minimized)
                        {
                            MainWindow.WindowState = WindowState.Normal;
                        }

                        MainWindow.Activate();
                    });
                }
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Opens the Main Window without locking.
        /// </summary>
        private void DoOpenWindow()
        {
            CreateMainWindow();
            (MainWindow as Window).Show();
        }

        /// <summary>
        /// Event handler triggered when the Main Window is closed.
        /// </summary>
        private void OnMainWindowClosed(object sender, EventArgs e)
        {
            lock (_mainWindowLock)
            {
                // Unsubscribe and release windows.
                (MainWindow as MainWindow).Closed -= OnMainWindowClosed;
                (MainWindow as MainWindow).Closed -= TrayBusiness.Instance.OnMainWindowClosed;
                MainWindow = null;
                ActiveWindow = null;

                // Dispose and release main pages View Models.
                KanjiVm.Dispose();
                KanjiVm = null;

                SrsVm.Dispose();
                SrsVm = null;
            }
        }

        public void OnShutdownRequested(object? sender, EventArgs e)
        {
            (MainWindow as MainWindow).Closed -= OnMainWindowClosed;
            (MainWindow as MainWindow).Closed -= TrayBusiness.Instance.OnMainWindowClosed;
        }

        public override void SetMainWindow(ContentControl control)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}
