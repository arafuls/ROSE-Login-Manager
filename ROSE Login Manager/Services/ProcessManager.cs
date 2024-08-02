﻿using NLog;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Resources.Util;
using ROSE_Login_Manager.Services.Memory_Scanner;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace ROSE_Login_Manager.Services
{
    /// <summary>
    ///     Manages the processes related to the ROSE Online client and their associated user profiles.
    /// </summary>
    public partial class ProcessManager
    {
        #region Native Methods

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;

        /// <summary>
        ///     Sets the text of the specified window's title bar.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose text is to be set.</param>
        /// <param name="lpString">The new title or text for the window.</param>
        /// <returns><see langword="true"/> if the function succeeds, otherwise <see langword="false"/>.</returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time



        /// <summary>
        ///     Changes the position, size, and Z order of a child, pop-up, or top-level window.
        /// </summary>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="hWndInsertAfter">Handle to the window to precede the positioned window in the Z order.</param>
        /// <param name="X">New position of the left side of the window.</param>
        /// <param name="Y">New position of the top of the window.</param>
        /// <param name="cx">New width of the window.</param>
        /// <param name="cy">New height of the window.</param>
        /// <param name="uFlags">Window positioning flags.</param>
        /// <returns>True if the function succeeds; otherwise, false.</returns>
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);



        /// <summary>
        ///     Enumerates all non-child windows associated with a thread.
        /// </summary>
        /// <param name="dwThreadId">Identifier of the thread whose windows are to be enumerated.</param>
        /// <param name="lpfn">Pointer to an application-defined callback function.</param>
        /// <param name="lParam">Application-defined value to be passed to the callback function.</param>
        /// <returns>True if the function succeeds; otherwise, false.</returns>
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool EnumThreadWindows(uint dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);



        /// <summary>
        /// Delegate type used for the callback function called by EnumThreadWindows for each window associated with the thread.
        /// </summary>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="lParam">Application-defined value passed to the callback function.</param>
        /// <returns>True to continue enumeration; otherwise, false to stop.</returns>
        private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        #endregion



        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly List<ActiveProcessInfo> _activeProcesses = new();
        private static readonly DatabaseManager _db = new();
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        private const uint ELAPSED_TIME_MILLISECONDS = 5000;

        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private Task? _backgroundTask;



        /// <summary>
        ///     Represents information about an active process, including its process ID, associated email, and process exit handling.
        /// </summary>
        private class ActiveProcessInfo
        {
            public CharacterInfo CharacterInfo { get; set; }
            public Process Process { get; set; }
            public int ProcessId { get; set; }
            public string Email { get; set; }
            public EventHandler ProcessExited; // Event handler for process exit

            public ActiveProcessInfo(Process process, int processId, string email)
            {
                Process = process;
                ProcessId = processId;
                Email = email;

                // Subscribe to the process exited event
                ProcessExited = (sender, e) =>
                {
                    OnProcessExited();
                };

                Process.EnableRaisingEvents = true;
                Process.Exited += ProcessExited;
            }

            /// <summary>
            /// Event handler called when the associated process exits.
            /// </summary>
            private void OnProcessExited()
            {
                try
                {
                    // Ensure thread-safe access to _activeProcesses list
                    lock (_activeProcesses)
                    {
                        // Find the ActiveProcessInfo object corresponding to the exited process
                        ActiveProcessInfo? activeProcess = _activeProcesses.FirstOrDefault(p => p.ProcessId == ProcessId);
                        if (activeProcess != null)
                        {
                            _db.UpdateProfileStatus(Email, false);
                            _activeProcesses.Remove(activeProcess);
                            Logger.Info($"ROSE Online client process {activeProcess.ProcessId} has exited.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error cleaning up after process {ProcessId}: {ex.Message}");
                }
            }
        }



        /// <summary>
        ///     Gets the singleton instance of the ProcessManager class.
        /// </summary>
        private static readonly Lazy<ProcessManager> lazyInstance = new(() => new ProcessManager());
        public static ProcessManager Instance => lazyInstance.Value;



        /// <summary>
        ///     Initializes a new instance of the ProcessManager class.
        /// </summary>
        private ProcessManager()
        {
            _db.ClearAllProfileStatus();
            StartBackgroundTasks();
        }



        /// <summary>
        ///     Starts a background task that periodically performs several actions:
        ///     The task continues running until a cancellation request is made.
        /// </summary>
        private void StartBackgroundTasks()
        {
            _backgroundTask = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        HandleUntrackedProcesses();
                        HandleInactiveProfileInToml();
                        FindProcessData();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "An error occurred during background tasks.\n");
                    }
                    finally
                    {
                        _semaphore.Release();
                    }

                    await Task.Delay((int)ELAPSED_TIME_MILLISECONDS, _cancellationTokenSource.Token);
                }
            }, _cancellationTokenSource.Token);
        }



        /// <summary>
        ///     Stops the background tasks initiated by <see cref="StartBackgroundTasks"/>.
        ///     Cancels the ongoing background task and waits for its completion to ensure proper shutdown.
        /// </summary>
        public void StopBackgroundTasks()
        {
            _cancellationTokenSource.Cancel();
            _backgroundTask?.Wait();
        }



        /// <summary>
        ///     Handles the discovery of new instances of the process named "trose".
        ///     Adds new instances of the process to the list of active processes if they are not already tracked.
        ///     Logs the addition of new processes.
        /// </summary>
        public static void HandleUntrackedProcesses()
        {
            try
            {
                Process[] existingProcesses = Process.GetProcessesByName("trose");
                foreach (Process process in existingProcesses)
                {
                    bool isProcessAlreadyTracked = _activeProcesses.Any(p => p.ProcessId == process.Id);
                    if (!isProcessAlreadyTracked)
                    {
                        _activeProcesses.Add(new ActiveProcessInfo(process, process.Id, ""));
                        Logger.Info($"Untracked ROSE Online client process {process.Id} is now being tracked.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An exception occurred while handling active trose processes.\n");
            }
        }



        /// <summary>
        ///     Updates the status of a profile based on the email retrieved from the configuration file (`rose.toml`).
        ///     Checks if the email corresponds to an existing profile in the database. If the profile is found but
        ///     its associated process is not active, the profile's status is updated to inactive.
        ///     Logs warnings if the email is not found or if the profile does not exist in the database.
        /// </summary>
        private static void HandleInactiveProfileInToml()
        {
            object? value = GlobalVariables.Instance.GetTomlValue("game", "last_account_name");
            string? email = value as string;

            if (string.IsNullOrEmpty(email))
            {
                Logger.Warn("Value from 'last_account_name' within rose.toml could not be found.");
                return;
            }

            if (!_db.ProfileExists(email))
            {
                Logger.Warn($"Value from 'last_account_name' {email} does not correspond to a profile in the database.");
                return;
            }

            bool isProcessActive = _activeProcesses.Any(p => p.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (!isProcessActive)
            {
                _db.UpdateProfileStatus(email, false);
            }
        }



        /// <summary>
        ///     Launches the ROSE Online game client process with the provided start information.
        ///     Updates the profile status in the database and optionally moves the client process
        ///     to the background based on application settings.
        /// </summary>
        /// <param name="startInfo">ProcessStartInfo object containing information to start the client process.</param>
        /// <exception cref="ArgumentException">Thrown when the process arguments are missing in <paramref name="startInfo"/>.</exception>
#pragma warning disable CA1822 // Do not make static as it will override singleton and will cause threading issues
        public void LaunchROSE(ProcessStartInfo startInfo)
#pragma warning restore CA1822 
        {
            try
            {
                string[] arguments = startInfo.Arguments.Split(" ");
                Process? process = Process.Start(startInfo);

                // The process was launched with additional arguments, assuming they are login credentials
                if (arguments.Length > 3 && process != null)
                {
                    string emailArg = arguments.ElementAtOrDefault(4) ?? throw new ArgumentException("The process arguments are missing in ProcessStartInfo.");
                    _activeProcesses.Add(new ActiveProcessInfo(process, process.Id, emailArg));
                    _db.UpdateProfileStatus(emailArg, true);

                    Logger.Info($"ROSE Online client process {process.Id} has started.");
                }

                if (GlobalVariables.Instance.LaunchClientBehind && process != null)
                {
                    MoveToBackground(process);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An exception occured while launching ROSE Online client.\n");
            }
        }



        #region Methods to Move Process Window

        /// <summary>
        ///     Moves the main window of the specified process to the background.
        /// </summary>
        /// <param name="process">The process whose main window should be moved.</param>
        private static void MoveToBackground(Process process)
        {
            IntPtr hWnd = IntPtr.Zero;
            int maxAttempts = 50;
            int delay = 100; // Polling interval in milliseconds

            try
            {
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    hWnd = FindMainWindowHandle(process.Id);
                    if (hWnd != IntPtr.Zero)
                    {
                        break;
                    }
                    Thread.Sleep(delay);
                }

                if (hWnd == IntPtr.Zero)
                {
                    string message = $"Failed to find the main window of process {process.Id}.";
                    throw new Exception(message);
                }

                // Move the process window behind your application's window
                IntPtr mainAppHandle = Process.GetCurrentProcess().MainWindowHandle;
                SetWindowPos(hWnd, mainAppHandle, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Unable to move process window to background.\n");
            }
        }



        /// <summary>
        ///     Finds the main window handle of the process with the specified ID.
        /// </summary>
        /// <param name="processId">The ID of the process.</param>
        /// <returns>The main window handle of the process, or IntPtr.Zero if not found.</returns>
        private static IntPtr FindMainWindowHandle(int processId)
        {
            IntPtr hWnd = IntPtr.Zero;
            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
            {
                hWnd = FindWindowByThreadId((uint)thread.Id);
                if (hWnd != IntPtr.Zero)
                    break;
            }
            return hWnd;
        }



        /// <summary>
        ///     Finds the window handle associated with the specified thread ID.
        /// </summary>
        /// <param name="threadId">The ID of the thread.</param>
        /// <returns>The window handle associated with the specified thread ID, or IntPtr.Zero if not found.</returns>
        private static IntPtr FindWindowByThreadId(uint threadId)
        {
            IntPtr hWnd = IntPtr.Zero;
            EnumThreadWindows(threadId, (hWndEnum, lParam) =>
            {
                hWnd = hWndEnum;
                return false;
            }, IntPtr.Zero);
            return hWnd;
        }



        /// <summary>
        ///     Changes the title of the main window of the specified process.
        /// </summary>
        /// <param name="process">The process whose title is to be changed.</param>
        /// <param name="newTitle">The new title to set for the process.</param>
        public static void ChangeProcessTitle(Process process, string newTitle)
        {
            Logger.Debug($"Changing title of process {process.ProcessName} to '{newTitle}'.");

            IntPtr mainWindowHandle = process.MainWindowHandle;
            if (mainWindowHandle != IntPtr.Zero)
            {
                SetWindowText(mainWindowHandle, newTitle);
            }
            else
            {
                Logger.Warn($"Process {process.ProcessName} does not have a main window handle.");
            }
        }

        #endregion



        /// <summary>
        ///     Scans the active processes to retrieve and update data related to user profiles and character information.
        /// </summary>
        public static void FindProcessData()
        {
            try
            {
                foreach (ActiveProcessInfo? activeProcess in _activeProcesses.ToList())
                {
                    using MemoryScanner memscan = new(activeProcess.Process);

                    // If an email is found and it exists in the database, update the profile status to active
                    string email = memscan.GetActiveEmail();
                    if (!string.IsNullOrEmpty(email) && _db.EmailExists(email))
                    {
                        activeProcess.Email = email;
                        _db.UpdateProfileStatus(email, true);
                    }

                    if (GlobalVariables.Instance.ToggleCharDataScanning)
                    {
                        CharacterInfo characterInfo = new()
                        {
                            CharacterName = memscan.GetCharacterName()
                        };

                        if (characterInfo.IsValid())
                        {
                            ChangeProcessTitle(activeProcess.Process, characterInfo.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while scanning process data.\n");
            }
        }
    }
}
