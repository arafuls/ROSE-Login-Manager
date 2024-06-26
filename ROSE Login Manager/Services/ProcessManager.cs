﻿using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Resources.Util;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;



namespace ROSE_Login_Manager.Services
{
    /// <summary>
    ///     Manages the processes related to the ROSE Online client and their associated user profiles.
    /// </summary>
    public class ProcessManager  // Make sure this is `public` so it can be accessed from other namespaces
    {
        #region Native Methods
        /// <summary>
        ///     Finds a window by its class name or window name.
        /// </summary>
        /// <param name="lpClassName">The class name of the window (optional).</param>
        /// <param name="lpWindowName">The title of the window (optional).</param>
        /// <returns>A handle to the window if found; otherwise, IntPtr.Zero.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

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
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <summary>
        ///     Retrieves the identifier of the thread that created the specified window and optionally the process identifier.
        /// </summary>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="lpdwProcessId">Output parameter that receives the process ID.</param>
        /// <returns>The identifier of the thread that created the window.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        ///     Enumerates all non-child windows associated with a thread.
        /// </summary>
        /// <param name="dwThreadId">Identifier of the thread whose windows are to be enumerated.</param>
        /// <param name="lpfn">Pointer to an application-defined callback function.</param>
        /// <param name="lParam">Application-defined value to be passed to the callback function.</param>
        /// <returns>True if the function succeeds; otherwise, false.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumThreadWindows(uint dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        /// <summary>
        /// Delegate type used for the callback function called by EnumThreadWindows for each window associated with the thread.
        /// </summary>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="lParam">Application-defined value passed to the callback function.</param>
        /// <returns>True to continue enumeration; otherwise, false to stop.</returns>
        private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        // Constants
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        #endregion

        private readonly Dictionary<int, string> _activeProcesses = [];
        private readonly Timer _cleanupTimer;
        private readonly DatabaseManager _db = new();



        /// <summary>
        ///     Represents information about an active process, including its process ID and associated email.
        /// </summary>
        /// <param name="processId">The process ID of the active process.</param>
        /// <param name="email">The email associated with the active process.</param>
        private class ActiveProcessInfo(int processId, string email)
        {
            public int ProcessId { get; set; } = processId;
            public string Email { get; set; } = email;
        }



        /// <summary>
        ///     Gets the singleton instance of the ProcessManager.
        /// </summary>
        private static readonly Lazy<ProcessManager> lazyInstance = new(() => new ProcessManager());
        public static ProcessManager Instance => lazyInstance.Value;



        /// <summary>
        ///     Initializes a new instance of the ProcessManager class.
        /// </summary>
        private ProcessManager()
        {
            // Initialize and configure the timer to call the TimerCallback method every 5 seconds (5000 ms)
            _cleanupTimer = new Timer(TimerCallback, null, 0, 5000);

            HandleExistingTRoseProcesses();
        }



        private void TimerCallback(object o)
        {
            CleanUpExitedProcesses();
        }



        /// <summary>
        ///     Handles existing TRose processes by updating profile statuses and adding them to the active processes list.
        /// </summary>
        public void HandleExistingTRoseProcesses()
        {
            _db.ClearAllProfileStatus();

            try
            {
                Process[] existingProcesses = Process.GetProcessesByName("trose");
                foreach (Process process in existingProcesses)
                {   
                    // Add the PID of each existing trose process to the active processes list
                    _activeProcesses.Add(process.Id, "");

                    // TODO: Find a way to determine which profile is in use at this time
                }
            }
            catch (Exception ex)
            {   // Handle or log any exceptions as needed
                Console.WriteLine($"An error occurred while handling existing trose processes: {ex.Message}");
            }
        }



        /// <summary>
        ///     Cleans up exited processes by removing them from the active processes list and updating their associated profile statuses.
        /// </summary>
        public void CleanUpExitedProcesses()
        {
            // Iterate over a copy of the active process IDs to avoid issues with modification during enumeration
            foreach (int processId in _activeProcesses.Keys.ToList())
            {
                try
                {
                    Process.GetProcessById(processId);
                }
                catch (ArgumentException)
                {
                    if (_activeProcesses.TryGetValue(processId, out string email))
                    {
                        _activeProcesses.Remove(processId);
                        _db.UpdateProfileStatus(email, false);
                    }
                }
            }
        }



        public void LaunchROSE(ProcessStartInfo startInfo)
        {
            try
            {
                string[] arguments = startInfo.Arguments.Split(" ");
                Process? process = Process.Start(startInfo);

                if (arguments.Length == 3)
                {
                    // TODO: Figure out how to determine active profile info from active process
                    //       not launched from this Login Manager.
                }
                else if (process != null)
                {
                    string emailArg = arguments.ElementAtOrDefault(4) ?? throw new ArgumentException("The process arguments are missing in ProcessStartInfo.");
                    _activeProcesses.Add(process.Id, emailArg);
                    _db.UpdateProfileStatus(emailArg, true);
                }

                if (GlobalVariables.Instance.LaunchClientBehind && process != null)
                {
                    MoveToBackground(process);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred in LaunchROSE: {ex.Message}");
            }
        }



        #region Methods to Move Process Window
        /// <summary>
        ///     Moves the main window of the specified process to the background.
        /// </summary>
        /// <param name="process">The process whose main window should be moved.</param>
        private static void MoveToBackground(Process process)
        {
            // Wait for the process to initialize its main window
            IntPtr hWnd = IntPtr.Zero;
            int maxAttempts = 50;
            int delay = 100; // Polling interval in milliseconds

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                hWnd = FindMainWindowHandle(process!.Id);
                if (hWnd != IntPtr.Zero)
                {
                    break;
                }
                Thread.Sleep(delay);
            }

            if (hWnd == IntPtr.Zero)
            {
                throw new Exception("Failed to find the main window of the process.");
            }

            // Move the process window behind your application's window
            IntPtr mainAppHandle = Process.GetCurrentProcess().MainWindowHandle;
            SetWindowPos(hWnd, mainAppHandle, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
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
                return false; // Stop enumeration
            }, IntPtr.Zero);
            return hWnd;
        }
        #endregion
    }
}
