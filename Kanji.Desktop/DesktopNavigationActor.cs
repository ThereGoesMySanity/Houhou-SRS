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
    class DesktopNavigationActor : NotifyPropertyChanged, INavigationActor
    {
        #region Fields

        private NavigationPageEnum _currentPage;

        private object _mainWindowLock;

        #endregion

        #region Events

        public delegate void MainWindowCloseHandler();
        public event MainWindowCloseHandler MainWindowClose;

        #endregion

        #region Property

        /// <summary>
        /// Gets the currently active page.
        /// </summary>
        public NavigationPageEnum CurrentPage
        {
            get { return _currentPage; }
            private set
            {
                if (value != _currentPage)
                {
                    _currentPage = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the reference to the kanji view model
        /// to enable kanji navigation.
        /// </summary>
        public KanjiViewModel KanjiVm { get; set; }

        /// <summary>
        /// Gets or sets the reference to the SRS view model
        /// to enable SRS module navigation.
        /// </summary>
        public SrsViewModel SrsVm { get; set; }

        /// <summary>
        /// Gets or sets the reference to the Settings view model
        /// to enable settings page navigation.
        /// </summary>
        public SettingsViewModel SettingsVm { get; set; }

        /// <summary>
        /// Gets or sets a reference to the main window.
        /// </summary>
        public ContentControl MainWindow { get; private set; }

        /// <summary>
        /// Gets or sets the current modal window.
        /// </summary>
        public ContentControl ActiveWindow { get; set; }

        #endregion

        #region Constructors

        public DesktopNavigationActor()
        {
            _mainWindowLock = new object();
            CurrentPage = NavigationPageEnum.Home;
        }

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Navigates to the page referred by the given enum value.
        /// </summary>
        /// <param name="page">Page enum value.</param>
        public void Navigate(NavigationPageEnum page)
        {
            lock (_mainWindowLock)
            {
                if (CurrentPage == page)
                    return;

                RequireMainWindow();
                CurrentPage = page;
            }
        }

        /// <summary>
        /// Navigates to the page referred by the start page setting.
        /// </summary>
        public void NavigateToStartPage()
        {
            Navigate(Properties.Settings.Default.StartPage.ToNavigationPage());
        }

        /// <summary>
        /// Navigates to the SRS page and starts a review session.
        /// </summary>
        public void NavigateToReviewSession()
        {
            lock (_mainWindowLock)
            {
                RequireMainWindow();
                CurrentPage = NavigationPageEnum.Srs;
                SrsVm.StartReviewSession();
            }
        }

        /// <summary>
        /// Navigates to the kanji page, and performs an intra-navigation
        /// to the kanji referred by the given character.
        /// </summary>
        /// <param name="character">Character driving the navigation.</param>
        public void NavigateToKanji(KanjiWritingCharacter character)
        {
            lock (_mainWindowLock)
            {
                RequireMainWindow();
                CurrentPage = NavigationPageEnum.Kanji;
                KanjiVm.Navigate(character);
            }
        }

        /// <summary>
        /// Navigates to the settings page, and performs an intra-navigation
        /// to the specified settings page.
        /// </summary>
        /// <param name="page">Page to navigate to.</param>
        public void NavigateToSettings(SettingsCategoryEnum page)
        {
            lock (_mainWindowLock)
            {
                RequireMainWindow();
                CurrentPage = NavigationPageEnum.Settings;
                SettingsVm.Navigate(page);
            }
        }

        public async Task<SrsEntryEditedEventArgs> OpenSrsEditWindow(SrsEntry entry)
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
                CurrentPage = Properties.Settings.Default.StartPage.ToNavigationPage();
                DoOpenWindow();
            }
        }

        /// <summary>
        /// Closes the Main Window.
        /// </summary>
        public void CloseMainWindow()
        {
            var MainWindow = (this.MainWindow as MainWindow);
            lock (_mainWindowLock)
            {
                if (MainWindow != null)
                {
                    MainWindow.Close();
                }
            }
        }

        /// <summary>
        /// Opens the main window. If it is already open, focuses it.
        /// </summary>
        public void OpenOrFocus()
        {
            var MainWindow = (this.MainWindow as MainWindow);
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
        public void SendMainWindowCloseEvent()
        {
            if (MainWindowClose != null)
            {
                MainWindowClose();
            }

            if (Properties.Settings.Default.WindowCloseAction == WindowCloseActionEnum.Exit)
            {
                Program.Shutdown();
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Makes sure the main window is open.
        /// </summary>
        private void RequireMainWindow()
        {
            if (MainWindow == null)
            {
                DoOpenWindow();
            }
        }

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
        public void SetMainWindow(ContentControl window)
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
