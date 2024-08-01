using NLog;
using ROSE_Login_Manager.Services.Memory_Scanner;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;



namespace ROSE_Login_Manager.Services
{
    /// <summary>
    ///     Provides functionality to scan and read memory from a specified process.
    ///     This class allows you to open and close handles to processes, read memory, and retrieve specific data.
    ///     It handles memory access errors gracefully and supports resource cleanup through the <see cref="IDisposable"/> interface.
    /// </summary>
    internal partial class MemoryScanner : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Process _process; // The process from which memory will be scanned.
        private readonly IntPtr _baseAddress; // The base address of the process's memory from which scanning starts.
        private bool _disposed = false; // Indicates whether the object has been disposed.



        /// <summary>
        ///     Initializes a new instance of the <see cref="MemoryScanner"/> class with the specified process.
        /// </summary>
        /// <param name="process">The process from which memory will be scanned. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="process"/> parameter is <see langword="null"/>.</exception>
        public MemoryScanner(Process process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
            _baseAddress = GetBaseAddress(process);
        }



        #region IDisposable Implementation & Deconstructor

        /// <summary>
        ///     Releases all resources used by the <see cref="MemoryScanner"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }



        /// <summary>
        ///     Releases the resources used by the <see cref="MemoryScanner"/> object.
        /// </summary>
        /// <param name="disposing">Indicates whether to release both managed and unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                }

                // Dispose unmanaged resources
                _disposed = true;
            }
        }



        /// <summary>
        ///     Finalizer for the <see cref="MemoryScanner"/> class to ensure unmanaged resources are released.
        /// </summary>
        ~MemoryScanner()
        {
            Dispose(false);
        }
        #endregion



        #region Memory Access Methods

        /// <summary>
        ///     Opens a handle to the process with the specified access rights.
        /// </summary>
        /// <param name="desiredAccess">The access rights to request for the process handle.</param>
        /// <returns>The handle to the process.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the process handle cannot be opened.</exception>
        private IntPtr OpenProcessHandle(uint desiredAccess)
        {
            IntPtr processHandle = OpenProcess(desiredAccess, false, _process.Id);
            if (processHandle == IntPtr.Zero)
            {
                var errorCode = Marshal.GetLastWin32Error();
                Logger.Error($"Failed to open process with ID {_process.Id}. Error code: {errorCode}.");
                throw new InvalidOperationException($"Failed to open process. Error code: {errorCode}");
            }
            return processHandle;
        }



        /// <summary>
        ///     Closes the handle to the process if it is valid.
        /// </summary>
        /// <param name="handle">The handle to the process to be closed.</param>
        private static void CloseProcessHandle(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                CloseHandle(handle);
            }
        }



        /// <summary>
        ///     Reads a specified number of bytes from the memory of the process at a given address.
        /// </summary>
        /// <param name="address">The address in the process memory to read from.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>A byte array containing the data read from memory.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the memory cannot be read correctly.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to the memory is denied.</exception>
        /// <exception cref="Win32Exception">Thrown when a Win32 error occurs while reading memory.</exception>
        private byte[] ReadMemory(IntPtr address, int length)
        {
            // TODO: Find a way to determine if user is in avatar selection to prevent scanning
            IntPtr processHandle = IntPtr.Zero;
            byte[] buffer = new byte[length];

            try
            {
                processHandle = OpenProcessHandle(PROCESS_VM_READ);
                if (!ReadProcessMemory(processHandle, address, buffer, buffer.Length, out int bytesRead) || bytesRead != length)
                {
                    Logger.Warn($"Failed to read {length} bytes from address {address}.");

                    // Fail silently, this could be caused by attempting to scan while in avatar selection
                    //throw new InvalidOperationException(errorMessage);
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is Win32Exception)
            {
                // Handle specific exceptions
                Logger.Error(ex, $"Exception occurred while reading memory from address {address}.");
                throw;
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                Logger.Error(ex, $"An unexpected error occurred while reading memory from address {address}.");
                throw;
            }
            finally
            {
                CloseProcessHandle(processHandle);
            }

            return buffer;
        }



        /// <summary>
        ///     Gets the base address of the first module in the specified process.
        /// </summary>
        /// <param name="process">The process from which to retrieve the base address.</param>
        /// <returns>The base address of the first module in the process.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the process handle cannot be opened or modules cannot be enumerated.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to the process is denied.</exception>
        /// <exception cref="Win32Exception">Thrown when a Win32 error occurs while interacting with the process.</exception>
        private static IntPtr GetBaseAddress(Process process)
        {
            IntPtr processHandle = IntPtr.Zero;
            IntPtr baseAddress = IntPtr.Zero;

            try
            {
                processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, process.Id);
                if (processHandle == IntPtr.Zero)
                {
                    throw new InvalidOperationException($"Failed to open process. Error code: {Marshal.GetLastWin32Error()}");
                }

                // Initial buffer size for module handles
                const int initialBufferSize = 1024;
                IntPtr[] moduleHandles = new IntPtr[initialBufferSize];

                // Attempt to enumerate process modules
                if (!EnumProcessModules(processHandle, moduleHandles, (uint)(moduleHandles.Length * IntPtr.Size), out uint bytesNeeded))
                {
                    int errorCode = Marshal.GetLastWin32Error();

                    // Check if the process was terminated
                    if (errorCode == ERROR_INVALID_PARAMETER || errorCode == ERROR_ACCESS_DENIED)
                    {
                        Logger.Error(new InvalidOperationException($"Failed to enumerate process modules. Error code: {errorCode}"),
                            "Process might have been terminated or access was denied for process {ProcessName}:{ProcessId}.", process.ProcessName, process.Id);
                        return IntPtr.Zero;
                    }

                    throw new InvalidOperationException($"Failed to enumerate process modules. Error code: {errorCode}");
                }

                // Check if we have received valid data
                int moduleCount = (int)(bytesNeeded / (uint)IntPtr.Size);
                if (moduleCount == 0 || bytesNeeded == 0 || bytesNeeded < IntPtr.Size)
                {
                    throw new InvalidOperationException($"No modules found for the process {process.ProcessName}:{process.Id}.");
                }

                baseAddress = moduleHandles[0];
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error(ex, "UnauthorizedAccessException occurred while getting base address for process {ProcessName}:{ProcessId}.", process.ProcessName, process.Id);
            }
            catch (Win32Exception ex)
            {
                Logger.Error(ex, "Win32Exception occurred while getting base address for process {ProcessName}:{ProcessId}.", process.ProcessName, process.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unexpected error while getting base address for process {ProcessName}:{ProcessId}.", process.ProcessName, process.Id);
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                {
                    CloseHandle(processHandle);
                }
            }

            return baseAddress;
        }




        /// <summary>
        ///     Applies an offset to a base address to calculate a new address.
        /// </summary>
        /// <param name="baseAddress">The base address to which the offset is added.</param>
        /// <param name="offset">The offset to add to the base address.</param>
        /// <returns>The new address calculated by adding the offset to the base address.</returns>
        private static IntPtr ApplyOffset(IntPtr baseAddress, int offset)
        {
            return new IntPtr(baseAddress.ToInt64() + offset);
        }



        /// <summary>
        ///     Reads a pointer (8 bytes) from memory and returns it as an IntPtr.
        /// </summary>
        /// <param name="address">The address in memory from which to read the pointer.</param>
        /// <returns>The pointer read from memory as an IntPtr.</returns>
        public IntPtr ReadPointerFromMemory(IntPtr address)
        {
            byte[] buffer = ReadMemory(address, 8);
            return new IntPtr(BitConverter.ToInt64(buffer, 0));
        }




        /// <summary>
        ///     Reads a string of a specified length from memory and converts it to a string.
        /// </summary>
        /// <param name="address">The address in memory from which to read the string.</param>
        /// <param name="length">The length of the string to read from memory.</param>
        /// <returns>The string read from memory.</returns>
        public string ReadStringFromMemory(IntPtr address, int length)
        {
            byte[] buffer = ReadMemory(address, length);
            return Encoding.ASCII.GetString(buffer).TrimEnd('\0');
        }

        #endregion



        #region Get Active Email Methods

        /// <summary>
        ///     Retrieves the active email from memory by following a series of pointer dereferences and applying an offset.
        ///     The retrieved email is validated against a specific signature pattern to ensure it is in the correct format.
        /// </summary>
        /// <returns>
        ///     A string containing the validated email if found; otherwise, an empty string.
        /// </returns>
        public string GetActiveEmail()
        {
            // TOML 0x015B7C10 + 0x0000
            // ARGS 0x015D7428 + 0x0810 + 0x0110

            // Read the base address and apply offsets to get the final address
            IntPtr address = ApplyOffset(_baseAddress, 0x015D7428);
            address = ReadPointerFromMemory(ApplyOffset(address, 0x0810));
            address = ApplyOffset(ReadPointerFromMemory(address), 0x0110);

            // Read and validate the email signature from the memory address
            string email = ReadStringFromMemory(address, 320);
            return SignatureValidators.IsValidLoginEmailSignature(email);
        }

        #endregion



        #region Get Character Name Methods

        /// <summary>
        ///     Gets the character's name based on the character length.
        ///     Determines the offset to use based on the length and retrieves the name.
        /// </summary>
        /// <returns>The character's name as a string.</returns>
        public string GetCharacterName()
        {
            int length = GetCharacterLength();
            int baseOffset;

            if (length <= 15)
            {
                baseOffset = 0x159EF28;
            }
            else
            {
                baseOffset = 0x159EF80;
            }

            return GetCharacterNameFromOffset(baseOffset, 0x09A8);
        }



        /// <summary>
        ///     Retrieves the character's name from a given base offset and name offset.
        ///     Reads the name from memory based on the calculated address.
        /// </summary>
        /// <param name="baseOffset">The base offset used to calculate the starting address.</param>
        /// <param name="nameOffset">The offset added to the base address to locate the character's name.</param>
        /// <returns>The character's name as a string.</returns>
        private string GetCharacterNameFromOffset(int baseOffset, int nameOffset)
        {
            IntPtr address = ApplyOffset(_baseAddress, baseOffset);
            address = ReadPointerFromMemory(address);
            address = ApplyOffset(address, nameOffset);
            return ReadStringFromMemory(address, 16);
        }



        /// <summary>
        ///     Gets the length of the character's name.
        ///     Reads a byte from memory to determine the length of the name.
        /// </summary>
        /// <returns>The length of the character's name as an integer.</returns>
        private int GetCharacterLength()
        {
            int offset = 0x015AC990;
            IntPtr address = ApplyOffset(_baseAddress, offset);

            byte[] buffer = ReadMemory(address, 1);
            return buffer[0];
        }
        #endregion



        #region Native Methods, Structs, and Variables

        private const int ERROR_ACCESS_DENIED = 5;
        private const int ERROR_INVALID_PARAMETER = 87;

        private const uint PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint PROCESS_VM_READ = 0x0010;

        [LibraryImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool EnumProcessModules(IntPtr hProcess, [Out] IntPtr[] lphModule, uint cb, out uint lpcbNeeded);


        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool CloseHandle(IntPtr hObject);

        [LibraryImport("kernel32.dll")]
        public static partial IntPtr OpenProcess(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [LibraryImport("kernel32.dll")]
        private static partial int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public AllocationProtect AllocationProtect;
            public IntPtr RegionSize;
            public AllocationState State;
            public AllocationProtect Protect;
            public AllocationType Type;
        }

        private enum AllocationProtect : uint
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        [Flags]
        private enum AllocationState : uint
        {
            MEM_COMMIT = 0x1000,
            MEM_RESERVE = 0x2000,
            MEM_FREE = 0x10000
        }

        private enum AllocationType : uint
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000
        }
        #endregion
    }
}
