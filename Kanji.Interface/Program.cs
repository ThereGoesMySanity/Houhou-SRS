﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Kanji.Common.Helpers;
using Kanji.Common.Models;
using Kanji.Interface.Actors;
using Kanji.Interface.Business;
using Kanji.Interface.Helpers;
using Kanji.Interface.Models;
using Kanji.Interface.Utilities;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Dialogs;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace Kanji.Interface
{
    class Program
    {
        #region Static fields

        // Mutex used to make sure only one instance of the application is running.
        private static Mutex RunOnceMutex = new Mutex(true, InstanceHelper.InterfaceApplicationGuid);

        public static bool RunMainWindow;

        #endregion

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().UseManagedSystemDialogs();

        [STAThread]
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));

            // This app uses a mutex to make sure it is started only one time.
            // In this context, keep in mind that the mutex is the only
            // shared resource between instances of the application.

            // Acquire parameters.
            // There is one optional parameter:
            // 
            // [0] - Boolean - Indicates if the Main Window should be started.
            // - Default is True.

            RunMainWindow = (args.Any() ? ParsingHelper.ParseBool(args[0]) : null) ?? true;

            // Check the mutex.
            if (true || RunOnceMutex.WaitOne(TimeSpan.Zero, true))
            {
                // The application is not already running. So let's start it.
                Run();

                // Once the application is shutdown, release the mutex.
                RunOnceMutex.ReleaseMutex();
            }
            else
            {
                // The application is already running.
                // Transmit a command to open/focus the main window using the pipe system.
                using (NamedPipeHandler handler = new NamedPipeHandler(InstanceHelper.InterfaceApplicationGuid))
                {
                    handler.Write(PipeMessageEnum.OpenOrFocus.ToString());
                }
            }
        }

        /// <summary>
        /// Initializes the application, runs it, and manages
        /// the resources.
        /// </summary>
        public static void Run()
        {
            // Initialize the logging system.
            LogHelper.InitializeLoggingSystem();

            // Initialize settings.
            InitializeUserSettings();

            // Initialize the configuration system.
            ConfigurationHelper.InitializeConfiguration();

            // Load the navigation actor.
            NavigationActor.Instance = new NavigationActor();

            // Start loading user resources.
            RadicalStore.Instance.InitializeAsync();
            SrsLevelStore.Instance.InitializeAsync();

            // Load the autostart configuration.
            AutostartBusiness.Instance.Load();

            // Start the version business.
            VersionBusiness.Initialize();

            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            //app.DispatcherUnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            using (NamedPipeHandler pipeHandler = new NamedPipeHandler(InstanceHelper.InterfaceApplicationGuid))
            {
                // Listen for incoming pipe messages, to allow other processes to
                // communicate with this one.
                PipeActor.Initialize(pipeHandler);
                pipeHandler.StartListening();
                var app = BuildAvaloniaApp().AfterSetup((a) =>
                {
                    // Start the SRS business.
                    SrsBusiness.Initialize();
                }).StartWithClassicDesktopLifetime(new string[] { });
            }
        }

        /// <summary>
        /// Initializes user settings if they need to be.
        /// This ensures user settings are kept after upgrading to a newer version.
        /// </summary>
        private static void InitializeUserSettings()
        {
            if (Kanji.Interface.Properties.Settings.Default.ShouldUpgradeSettings)
            {
                try
                {
                    Kanji.Interface.Properties.Settings.Default.Upgrade();
                }
                catch (Exception ex)
                {
                    LogHelper.GetLogger("Settings initialization").Error("Settings initialization failed.", ex);
                }

                Kanji.Interface.Properties.Settings.Default.ShouldUpgradeSettings = false;
                Kanji.Interface.Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Event trigger. Called when an exception occurs in some situations.
        /// </summary>
        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            OnUnhandledException(e.Exception);
        }

        /// <summary>
        /// Event trigger. Called when an unhandled exception is thrown by any thread.
        /// </summary>
        private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            OnUnhandledException((e.ExceptionObject as Exception) ?? new Exception("Unknown fatal error."));
        }

        /// <summary>
        /// Event trigger. Called when an unhandled exception is thrown by the Dispatcher thread.
        /// </summary>
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            OnUnhandledException(e.ExceptionObject as Exception ?? new Exception("Unknown fatal error."));
        }

        /// <summary>
        /// Logs an unhandled exception and terminates the application properly.
        /// </summary>
        /// <param name="ex">Unhandled exception.</param>
        private static void OnUnhandledException(Exception ex)
        {
            LogHelper.GetLogger("Main").Fatal("A fatal exception occured:", ex);

            #if DEBUG
            throw ex;
            #else

            DispatcherHelper.Invoke(async () =>
            {
                if (await MessageBoxManager.GetMessageBoxStandardWindow(
                    new MessageBoxStandardParams
                    {
                        ContentTitle = "Fatal error",
                        ContentMessage = string.Format("It appears that Houhou has been vanquished by an evil {0}.{1}"
                            + "Houhou will now shutdown. Sorry for the inconvenience.{1}"
                            + "Houhou's last words were: \"{2}\". Oh the pain it must have been.{1}{1}"
                            + "Please send me a mail report along with your log file if you think I should fix the issue.{1}"
                            + "Do you want to open the log?",
                                ex.GetType().Name,
                                Environment.NewLine,
                                ex.Message),
                        ButtonDefinitions = ButtonEnum.YesNoCancel,
                        Icon = Icon.Error
                    }).ShowDialog(NavigationActor.Instance.ActiveWindow) == ButtonResult.Yes)
                {
                    try
                    {
                        ProcessHelper.OpenUri(LogHelper.GetLogFilePath());
                    }
                    catch (Exception ex2)
                    {
                        LogHelper.GetLogger("Main").Fatal("Failed to open the log after fatal exception. Double fatal shock.", ex2);

                        await MessageBoxManager.GetMessageBoxStandardWindow(
                            new MessageBoxStandardParams
                            {
                                ContentMessage = string.Format("Okay, so... the log file failed to open.{0}"
                                + "Um... I'm sorry. Now that's embarrassing...{0}"
                                + "Hey, listen, the log file should be there:{0}"
                                + "\"C:/Users/<YourUserName>/AppData/Local/Houhou SRS/Logs\"{0}"
                                + "If you still cannot get it, well just contact me without a log.{0}",
                                    Environment.NewLine),
                                ContentTitle = "Double fatal error shock",
                                ButtonDefinitions = ButtonEnum.Ok,
                                Icon = Icon.Error
                            }).ShowDialog(NavigationActor.Instance.ActiveWindow);
                    }
                }
            });

            Environment.Exit(1);
            #endif
        }

        /// <summary>
        /// Causes the application to shutdown.
        /// </summary>
        public static void Shutdown()
        {
            Kanji.Interface.Properties.Settings.Default.Save();
            DispatcherHelper.Invoke(() => { (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown(0); });
        }
    }
}
