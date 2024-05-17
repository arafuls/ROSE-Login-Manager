using System.Management;
using System.Security.Cryptography;
using System.Text;



namespace ROSE_Online_Login_Manager.Resources.Util
{
    internal class HWIDGenerator
    {
        /// <summary>
        ///     Retrieves the Hardware ID (HWID) of the system by combining various hardware identifiers.
        /// </summary>
        /// <returns>The HWID of the system as a byte[].</returns>
        public static byte[] GetHWID()
        {
            string cpuId         = GetProcessorID();
            string motherboardId = GetMotherboardID();
            string diskId        = GetDiskID();

            // Concatenate and hash the hardware IDs to generate the HWID
            string combinedIds = cpuId + motherboardId + diskId;
            return ComputeSHA256Hash(combinedIds);
        }



        /// <summary>
        ///     Retrieves the Processor ID of the system.
        /// </summary>
        /// <returns>The Processor ID of the system.</returns>
        private static string GetProcessorID()
        {
            using ManagementObjectSearcher searcher = new("SELECT ProcessorId FROM Win32_Processor");
            using ManagementObjectCollection collection = searcher.Get();

            StringBuilder result = new();
            foreach (ManagementBaseObject obj in collection)
            {
                result.Append(obj["ProcessorId"]);
            }

            return result.ToString();
        }



        /// <summary>
        ///     Retrieves the Motherboard Serial Number of the system.
        /// </summary>
        /// <returns>The Motherboard Serial Number of the system.</returns>
        private static string GetMotherboardID()
        {
            using ManagementObjectSearcher searcher = new("SELECT SerialNumber FROM Win32_BaseBoard");
            using ManagementObjectCollection collection = searcher.Get();

            StringBuilder result = new();
            foreach (ManagementBaseObject obj in collection)
            {
                result.Append(obj["SerialNumber"]);
            }

            return result.ToString();
        }



        /// <summary>
        ///     Retrieves the Disk Drive Serial Number of the system.
        /// </summary>
        /// <returns>The Disk Drive Serial Number of the system.</returns>
        private static string GetDiskID()
        {
            using ManagementObjectSearcher searcher = new("SELECT SerialNumber FROM Win32_DiskDrive WHERE MediaType='Fixed hard disk media'");
            using ManagementObjectCollection collection = searcher.Get();
            StringBuilder result = new();

            foreach (ManagementBaseObject obj in collection)
            {
                result.Append(obj["SerialNumber"]);
            }

            return result.ToString();
        }



        /// <summary>
        ///     Computes the SHA256 hash of the input string.
        /// </summary>
        /// <param name="input">The input string to be hashed.</param>
        /// <returns>The SHA256 hash of the input string as a byte[].</returns>
        private static byte[] ComputeSHA256Hash(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return SHA256.HashData(bytes);
        }
    }
}
