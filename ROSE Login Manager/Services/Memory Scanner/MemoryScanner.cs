using NLog;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services.Memory_Scanner;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;



namespace ROSE_Login_Manager.Services
{
    /// <summary>
    ///     Provides methods to scan memory of a specgzified process for a given signature.
    /// </summary>
    internal partial class MemoryScanner : IDisposable
    {
        private static readonly IntPtr CHAR_NAME_ADDRESS = new(0x00007FF64C0F5CE0);
        private static readonly string JOB_LEVEL_SIGNATURE = "?? ?? ?? ?? ?? ?? ?? 20 2D 20 4C 65 76 65 6C 20 ?? ?? ??";
        private static readonly string LOGIN_STR_SIGNATURE = "2D 2D 73 65 72 76 65 72 20 63 6F 6E 6E 65 63 74 2E 72 6F 73 65 6F 6E 6C 69 6E 65 67 61 6D 65 2E 63 6F 6D 20 2D 2D 75 73 65 72 6E 61 6D 65 20";

        private readonly Process _process;

        private bool disposed = false;
        private CharacterInfo _characterInfo = new();



        /// <summary>
        ///     Initializes a new instance of the <see cref="MemoryScanner"/> class with the specified process.
        /// </summary>
        /// <param name="process">The process to scan.</param>
        public MemoryScanner(Process process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }



        #region IDisposable Implementation

        /// <summary>
        ///     Releases all resources used by the <see cref="MemoryScanner"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                }

                // Dispose unmanaged resources
                disposed = true;
            }
        }

        ~MemoryScanner()
        {
            Dispose(false);
        }
        #endregion



        /// <summary>
        ///     Scans the game's memory to retrieve character information, including the character name and job level.
        /// </summary>
        /// <returns>
        ///     A <see cref="CharacterInfo"/> object containing the scanned character's information if successful;
        ///     otherwise, returns an empty <see cref="CharacterInfo"/> object.
        /// </returns>
        public CharacterInfo ScanCharacterInfoSignature()
        {
            byte[] signature = ConvertStringToBytes(JOB_LEVEL_SIGNATURE);
            if (!GetCharacterName() || (ScanMemory(signature) == IntPtr.Zero))
            {
                return new CharacterInfo();
            }

            return _characterInfo;
        }



        /// <summary>
        ///     Scans the memory of the current process for the active email signature.
        /// </summary>
        /// <returns>
        ///     The email address if found; otherwise, an empty string.
        /// </returns>
        public string ScanActiveEmailSignature()
        {
            byte[] signature = ConvertStringToBytes(LOGIN_STR_SIGNATURE);
            if (ScanMemory(signature) == IntPtr.Zero)
            {
                return string.Empty;
            }

            return _characterInfo.AccountEmail;
        }



        /// <summary>
        ///     Retrieves the character name from the specified memory address.
        /// </summary>
        /// <returns><see langword="true"/> if the character name is successfully retrieved; otherwise, <see langword="false"/>.</returns>
        private bool GetCharacterName()
        {
            const int bufferSize = 17;
            byte[] buffer = new byte[bufferSize];

            try
            {
                if (_process == null || _process.HasExited)
                {
                    return false;
                }

                if (ReadProcessMemory(_process.Handle, CHAR_NAME_ADDRESS, buffer, buffer.Length, out int bytesRead) && bytesRead > 0)
                {
                    string characterName = Encoding.ASCII.GetString(buffer).TrimEnd('\0');
                    _characterInfo.CharacterName = characterName;
                    return true;
                }
                else
                {
                    LogManager.GetCurrentClassLogger().Error($"Failed to read process memory.");
                }
            }
            catch (InvalidOperationException ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
            }

            return false;
        }



        /// <summary>
        ///     Scans the process memory for a given signature.
        /// </summary>
        /// <param name="signature">The signature to scan for.</param>
        /// <returns>The memory address where the signature is found, or IntPtr.Zero if not found.</returns>
        private IntPtr ScanMemory(byte[] signature)
        {
            IntPtr currentAddress = IntPtr.Zero;
            const int chunkSize = 4096; // Adjusted chunk size for performance
            byte[] buffer = new byte[chunkSize]; // Reusable buffer

            try
            {
                while (VirtualQueryEx(_process.Handle, currentAddress, out MEMORY_BASIC_INFORMATION mbi, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) != 0)
                {
                    // Check if the memory region is committed and has the necessary protection
                    if (mbi.State == AllocationState.MEM_COMMIT &&
                        (mbi.Protect == AllocationProtect.PAGE_READWRITE || mbi.Protect == AllocationProtect.PAGE_READONLY))
                    {
                        long baseAddress = mbi.BaseAddress.ToInt64();
                        long regionSize = mbi.RegionSize.ToInt64();
                        long remainingSize = regionSize;

                        // Read and scan in chunks
                        while (remainingSize > 0)
                        {
                            int bufferSize = (int)Math.Min(remainingSize, chunkSize);

                            // Read the process memory into the buffer
                            if (ReadProcessMemory(_process.Handle, new IntPtr(baseAddress), buffer, bufferSize, out int bytesRead) && bytesRead > 0)
                            {
                                // Scan the buffer for the signature
                                for (int i = 0; i <= bytesRead - signature.Length; i++)
                                {
                                    if (IsValidMatch(buffer, i, signature))
                                    {
                                        return new IntPtr(baseAddress + i);
                                    }
                                }
                            }

                            remainingSize -= bufferSize;
                            baseAddress += bufferSize;
                        }
                    }

                    currentAddress = new IntPtr(mbi.BaseAddress.ToInt64() + mbi.RegionSize.ToInt64());
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
            }

            return IntPtr.Zero;
        }



        /// <summary>
        ///     Validates if a given signature matches a specific pattern in the provided buffer starting from the specified index.
        /// </summary>
        /// <param name="buffer">The byte array buffer to search within.</param>
        /// <param name="startIndex">The starting index in the buffer to check for the signature.</param>
        /// <param name="signature">The byte array signature to match against.</param>
        /// <returns>
        ///     True if the signature matches the pattern in the buffer; otherwise, false.
        /// </returns>
        /// <remarks>
        ///     This method checks if the signature matches a specific pattern in the buffer starting from the given index. 
        ///     It performs the following steps:
        ///     1. Validates that the buffer has enough length to accommodate the signature plus additional bytes.
        ///     2. Compares each byte in the signature with the corresponding byte in the buffer, ignoring null bytes.
        ///     3. If the signature matches the job level signature, it validates the job title and level and updates the character info.
        ///     4. If the signature matches the login string signature, it validates the login email and updates the character info.
        /// </remarks>
        private bool IsValidMatch(byte[] buffer, int startIndex, byte[] signature)
        {
            if (startIndex + signature.Length + 3 > buffer.Length)
                return false;

            for (int j = 0; j < signature.Length; j++)
            {
                if (signature[j] != 0x00 && buffer[startIndex + j] != signature[j])
                {
                    return false;
                }
            }

            if (signature.SequenceEqual(ConvertStringToBytes(JOB_LEVEL_SIGNATURE)))
            {
                var (JobTitle, Level) = SignatureValidators.IsValidJobLevelSignature(buffer, startIndex, signature);
                _characterInfo.JobTitle = JobTitle;
                _characterInfo.Level = Level;
            }
            else if (signature.SequenceEqual(ConvertStringToBytes(LOGIN_STR_SIGNATURE)))
            {
                string foundEmail = SignatureValidators.IsValidLoginEmailSignature(buffer, startIndex, signature);
                _characterInfo.AccountEmail = foundEmail;
            }

            return true;
        }



        /// <summary>
        ///     Converts a hexadecimal string signature into a byte array.
        /// </summary>
        /// <param name="signature">The hexadecimal string signature.</param>
        /// <returns>The byte array representation of the signature.</returns>
        private static byte[] ConvertStringToBytes(string signature)
        {
            string[] parts = signature.Split(' ');
            byte[] bytes = new byte[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Contains('?'))
                {
                    bytes[i] = 0x00; // Wildcard Byte
                }
                else
                {
                    bytes[i] = Convert.ToByte(parts[i], 16);
                }
            }

            return bytes;
        }



        #region Native Methods, Structs, and Variables

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
