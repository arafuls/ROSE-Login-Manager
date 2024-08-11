using System.Text;
using System.Text.RegularExpressions;



namespace ROSE_Login_Manager.Services.Memory_Scanner
{
    /// <summary>
    ///     Provides methods for validating job titles and levels based on signature patterns.
    /// </summary>
    internal static class SignatureValidators
    {
        /// <summary>
        ///     Validates and extracts a login email address from a specified buffer starting from the given index.
        /// </summary>
        /// <param name="buffer">The byte array buffer to search within.</param>
        /// <param name="startIndex">The starting index in the buffer to check for the signature.</param>
        /// <param name="signature">The byte array signature to match against.</param>
        /// <returns>
        ///     The extracted email address if a valid match is found; otherwise, an empty string.
        /// </returns>
        internal static string IsValidLoginEmailSignature(string email)
        {
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
            Regex regex = new(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\
                                    x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[
                                    a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]
                                    |[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1
                                    -9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[
                                    \x01-\x09\x0b\x0c\x0e-\x7f])+)\])");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

            Match match = regex.Match(email);
            if (match.Success)
            {
                return match.Value;
            }

            return string.Empty;
        }
    }
}
