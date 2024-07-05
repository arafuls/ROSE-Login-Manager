using System.Text;
using System.Text.RegularExpressions;



namespace ROSE_Login_Manager.Services.Memory_Scanner
{
    /// <summary>
    ///     Provides methods for validating job titles and levels based on signature patterns.
    /// </summary>
    internal static class SignatureValidators
    {
        private static readonly string[] ValidJobTitles =
        [
            "Visitor",
            "Dealer",   "Bourg",    "Artisan",
            "Hawker",   "Raider",   "Scout",
            "Muse",     "Mage",     "Cleric",
            "Soldier",  "Champion", "Knight"
        ];

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
        internal static (string JobTitle, int Level) IsValidJobLevelSignature(byte[] buffer, int startIndex, byte[] signature)
        {
            string foundString = Encoding.ASCII.GetString(buffer, startIndex, signature.Length);

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
                        return (jobTitle, level); // Valid job level signature found
                    }
                }
            }

            return ("", 0); // No valid job level signature found
        }



        /// <summary>
        ///     Validates and extracts a login email address from a specified buffer starting from the given index.
        /// </summary>
        /// <param name="buffer">The byte array buffer to search within.</param>
        /// <param name="startIndex">The starting index in the buffer to check for the signature.</param>
        /// <param name="signature">The byte array signature to match against.</param>
        /// <returns>
        ///     The extracted email address if a valid match is found; otherwise, an empty string.
        /// </returns>
        internal static string IsValidLoginEmailSignature(byte[] buffer, int startIndex, byte[] signature)
        {
            string foundString = Encoding.ASCII.GetString(buffer, startIndex + signature.Length, 320);

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
            Regex regex = new(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\
                                    x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[
                                    a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]
                                    |[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1
                                    -9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[
                                    \x01-\x09\x0b\x0c\x0e-\x7f])+)\])");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

            Match match = regex.Match(foundString);
            if (match.Success)
            {
                return match.Value;
            }

            return string.Empty;
        }
    }
}
