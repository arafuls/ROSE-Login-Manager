using ROSE_Login_Manager.Resources.Util;
using System.Diagnostics;



namespace ROSE_Login_Manager.Services
{
    /// <summary>
    ///     Manages the processes related to the ROSE Online client and their associated user profiles.
    /// </summary>
    public class ProcessManager  // Make sure this is `public` so it can be accessed from other namespaces
    {

        private readonly Dictionary<int, string> _activeProcesses = [];
        private readonly Timer _cleanupTimer;
        private readonly DatabaseManager _db = new();



        /// <summary>
        ///     Represents information about an active process, including its process ID and associated email.
        /// </summary>
        private class ActiveProcessInfo
        {
            public int ProcessId { get; set; }
            public string Email { get; set; }

            /// <summary>
            ///     Initializes a new instance of the ActiveProcessInfo class with the specified process ID and email.
            /// </summary>
            /// <param name="processId">The process ID of the active process.</param>
            /// <param name="email">The email associated with the active process.</param>
            public ActiveProcessInfo(int processId, string email)
            {
                ProcessId = processId;
                Email = email;
            }
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



        /// <summary>
        ///     Launches the ROSE Online client process with the provided startInfo and updates the associated profile status.
        /// </summary>
        /// <param name="startInfo">The ProcessStartInfo containing the arguments to start the ROSE Online client.</param>
        public void LaunchROSE(ProcessStartInfo startInfo)
        {   
            // Extract the email from the startInfo arguments
            string[] arguments = [];
            string emailArg;

            try
            {
                arguments = startInfo.Arguments.Split(" ");
                emailArg = arguments[4];

                // Start the ROSE Online client process
                Process? process = Process.Start(startInfo);
                if (process != null)
                {
                    _activeProcesses.Add(process.Id, emailArg);
                    if (!string.IsNullOrEmpty(emailArg))
                    {
                        // Update the profile status in the database
                        _db.UpdateProfileStatus(emailArg, true);
                    }
                    else
                    {
                        // Handle the case where the --username argument is missing
                        throw new ArgumentException("The process arguments are missing in ProcessStartInfo.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"An error occurred in LaunchROSE: {ex.Message}");
            }
            finally
            {
                // Clear the arguments array to remove sensitive information
                Array.Clear(arguments, 0, arguments.Length);
            }
        }
    }
}
