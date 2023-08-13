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

namespace Kanji.Interface.Actors
{
    class DesktopNavigationActor : NavigationActor
    {
        #region Fields

        private NavigationPageEnum _currentPage;

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
            return wnd.Result;
        }

        /// <summary>
        /// Opens the Main Window.
        /// </summary>
        public void OpenMainWindow()
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
        public void OpenOrFocus()
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

        /// <summary>
        /// Sends the main window close event.
        /// </summary>
        public override void SendMainWindowCloseEvent()
        {
            if (Properties.UserSettings.Instance.WindowCloseAction == WindowCloseActionEnum.Exit)
            {
                Program.Shutdown();
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Opens the Main Window without locking.
        /// </summary>
        private void DoOpenWindow()
        {
            DispatcherHelper.Invoke(() =>
            {
                var window = new MainWindow();
                MainWindow = window;
                ActiveWindow = MainWindow;
                window.Closed += OnMainWindowClosed;
                window.Show();
            });
        }

        public override void SetMainWindow(ContentControl window)
        {
            SetMainWindow(window as MainWindow);
        }

        public void SetMainWindow(MainWindow window)
        {
            MainWindow = window;
            ActiveWindow = MainWindow;
            window.Closed += OnMainWindowClosed;
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
                MainWindow = null;
                ActiveWindow = null;

                // Dispose and release main pages View Models.
                KanjiVm.Dispose();
                KanjiVm = null;

                SrsVm.Dispose();
                SrsVm = null;
            }
        }

        #endregion

        #endregion
    }
}
