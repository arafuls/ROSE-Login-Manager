using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services.Memory_Scanner;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;



namespace ROSE_Login_Manager.Services
{
    /// <summary>
    ///     Provides methods to scan memory of a specified process for a given signature.
    /// </summary>
    internal class MemoryScanner : IDisposable
    {
        private static readonly IntPtr CHAR_NAME_ADDRESS = new(0x7FF758C15CE0);

        private const string JOB_LEVEL_SIGNATURE = "?? ?? ?? ?? ?? ?? ?? 20 2D 20 4C 65 76 65 6C 20 ?? ?? ??";

        private static Process _process;
        private bool disposed = false;

        private CharacterInfo _characterInfo = new();



        /// <summary>
        ///     Initializes a new instance of the <see cref="MemoryScanner"/> class with the specified process.
        /// </summary>
        /// <param name="process">The process to scan.</param>
        public MemoryScanner(Process process)
        {
            try
            {
                _process = process ?? throw new ArgumentNullException(nameof(process));
            }
            catch (ArgumentNullException ex)
            {
                // TODO: Log the exception or handle it, ArgumentNullException in MemoryScanner constructor
            }
            catch (Exception ex)
            {
                // TODO: Log the exception or handle it, Exception in MemoryScanner constructor
            }
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
        ///     Scans the character information signature in the process memory.
        /// </summary>
        /// <returns>The scanned character information.</returns>
        public CharacterInfo ScanCharacterInfoSignature()
        {
            if (!GetCharacterName()) { return _characterInfo; }

            byte[] jobLevelSignature = ConvertStringToBytes(JOB_LEVEL_SIGNATURE);
            ScanMemory(jobLevelSignature);

            return _characterInfo;
        }



        /// <summary>
        ///     Retrieves the character name from the specified memory address.
        /// </summary>
        /// <returns><see langword="true"/> if the character name is successfully retrieved; otherwise, <see langword="false"/>.</returns>
        private bool GetCharacterName()
        {
            const int bufferSize = 16; // Adjust as needed
            byte[] buffer = new byte[bufferSize];

            if (ReadProcessMemory(_process.Handle, CHAR_NAME_ADDRESS, buffer, buffer.Length, out int bytesRead) && bytesRead > 0)
            {
                string characterName = Encoding.ASCII.GetString(buffer).TrimEnd('\0');
                _characterInfo.CharacterName = characterName;
                return true;
            }
            int error = Marshal.GetLastWin32Error();
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
            const int chunkSize = 8192; // Adjusted chunk size for performance

            while (VirtualQueryEx(_process.Handle, currentAddress, out MEMORY_BASIC_INFORMATION mbi, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) != 0)
            {
                // Check if the memory region is committed and has the necessary protection
                if (mbi.State == AllocationState.MEM_COMMIT &&
                    (mbi.Protect == AllocationProtect.PAGE_READWRITE || mbi.Protect == AllocationProtect.PAGE_READONLY))
                {
                    long baseAddress = mbi.BaseAddress.ToInt64();
                    long regionSize = mbi.RegionSize.ToInt64();
                    long remainingSize = regionSize;

                    while (remainingSize > 0)
                    {
                        int bufferSize = (int)Math.Min(remainingSize, chunkSize);
                        byte[] buffer = new byte[bufferSize];

                        // Read the process memory into the buffer
                        if (ReadProcessMemory(_process.Handle, new IntPtr(baseAddress), buffer, buffer.Length, out int bytesRead) && bytesRead > 0)
                        {
                            // Scan the buffer for the signature
                            for (int i = 0; i <= bufferSize - signature.Length; i++)
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

            return IntPtr.Zero;
        }



        /// <summary>
        ///     Checks if the given buffer at the specified start index matches the signature.
        /// </summary>
        /// <param name="buffer">The buffer to check.</param>
        /// <param name="startIndex">The start index in the buffer.</param>
        /// <param name="signature">The signature to match.</param>
        /// <returns><see langword="true"/> if the buffer matches the signature; otherwise, <see langword="false"/>.</returns>
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
                var result = SignatureValidators.IsValidJobLevelSignature(buffer, startIndex, signature);
                _characterInfo.JobTitle = result.JobTitle;
                _characterInfo.Level = result.Level;
            }    

            return true;
        }



        /// <summary>
        ///     Converts a hexadecimal string signature into a byte array.
        /// </summary>
        /// <param name="signature">The hexadecimal string signature.</param>
        /// <returns>The byte array representation of the signature.</returns>
        private byte[] ConvertStringToBytes(string signature)
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

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

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
