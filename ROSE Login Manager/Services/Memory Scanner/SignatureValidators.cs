using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace ROSE_Login_Manager.Services.Memory_Scanner
{
    /// <summary>
    ///     Provides methods for validating job titles and levels based on signature patterns.
    /// </summary>
    internal static class SignatureValidators
    {
        private static readonly string[] ValidJobTitles = 
        { 
            "Visitor", 
            "Dealer",   "Bourg",    "Artisan", 
            "Hawker",   "Raider",   "Scout", 
            "Muse",     "Mage",     "Cleric", 
            "Soldier",  "Champion", "Knight" 
        };

        private const ushort MAX_LEVEL = 250;



        /// <summary>
        ///     Validates a job level signature within a buffer based on predefined job titles.
        /// </summary>
        /// <param name="buffer">Byte array buffer containing the signature data.</param>
        /// <param name="startIndex">Starting index within the buffer to check.</param>
        /// <param name="signature">Byte array representing the signature pattern to match.</param>
        /// <returns>
        ///     A tuple containing:
        ///     - <c>true</c> if a valid job level signature is found;
        ///     - The job title as a string if found;
        ///     - The level as an integer if valid and within bounds.
        /// </returns>
        public static (bool IsValid, string? JobTitle, int Level) IsValidJobLevelSignature(byte[] buffer, int startIndex, byte[] signature)
        {
            // Convert the portion of the buffer to a string based on the provided startIndex and signature length
            string foundString = Encoding.ASCII.GetString(buffer, startIndex, signature.Length);

            // Iterate through each valid job title to check if the foundString matches the job level signature criteria
            foreach (var jobTitle in ValidJobTitles)
            {
                // Create a regex pattern dynamically for the current jobTitle
                string pattern = $@"\b{Regex.Escape(jobTitle)} - Level (\d{{1,3}})\b";
                Match match = Regex.Match(foundString, pattern);

                if (match.Success)
                {
                    string levelStr = match.Groups[1].Value;

                    if (int.TryParse(levelStr, out int level) && level > 0 && level <= MAX_LEVEL)
                    {
                        return (true, jobTitle, level); // Valid job level signature found
                    }
                }
            }

            return (false, null, 0); // No valid job level signature found
        }
    }
}
