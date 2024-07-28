


using System.Text.RegularExpressions;

namespace ROSE_Login_Manager.Model
{
    /// <summary>
    ///     Represents information about a character, including job title, level, and character name.
    /// </summary>
    public struct CharacterInfo
    {
        private static readonly string[] JOBS =
        [
            "Visitor",
            "Dealer",   "Bourg",    "Artisan",
            "Hawker",   "Raider",   "Scout",
            "Muse",     "Mage",     "Cleric",
            "Soldier",  "Champion", "Knight"
        ];

        private const ushort MIN_LEVEL = 1;
        private const ushort MAX_LEVEL = 250;

        /// <summary>
        ///     Gets or sets the job title of the character.
        /// </summary>
        public string JobTitle { get; set; }

        /// <summary>
        ///     Gets or sets the level of the character.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        ///     Gets or sets the name of the character.
        /// </summary>
        public string CharacterName { get; set; }

        /// <summary>
        ///     Gets or sets the email of the account.
        /// </summary>
        public string AccountEmail { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CharacterInfo"/> struct.
        /// </summary>
        /// <param name="jobTitle">The job title of the character.</param>
        /// <param name="level">The level of the character.</param>
        /// <param name="characterName">The name of the character.</param>
        /// <param name="AccountEmail">The email of the account.</param>
        public CharacterInfo(string jobTitle, int level, string characterName, string accountEmail)
        {
            JobTitle = jobTitle;
            Level = level;
            CharacterName = characterName;
            AccountEmail = accountEmail;
        }

        /// <summary>
        ///     Returns a string representation of the <see cref="CharacterInfo"/> struct.
        /// </summary>
        /// <returns>A string that represents the character information.</returns>
        public override readonly string ToString()
        {
            return $"{CharacterName} | Level {Level}) - {JobTitle}";
        }

        /// <summary>
        ///     Validates the current instance based on the character name, job title, and level.
        /// </summary>
        /// <returns><c>true</c> if all properties are valid; otherwise, <c>false</c>.</returns>
        public readonly bool IsValid()
        {
            if (!IsValidCharacterName(CharacterName))
            {
                return false;
            }

            if (!IsValidJobTitle(JobTitle))
            {
                return false;
            }

            if (!IsValidLevel(Level))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Checks if the specified character name is valid.
        ///     A valid character name must be between 4 and 16 characters long and contain only alphanumeric characters.
        /// </summary>
        /// <param name="characterName">The character name to validate.</param>
        /// <returns><c>true</c> if the character name is valid; otherwise, <c>false</c>.</returns>
        public static bool IsValidCharacterName(string characterName)
        {
            if (string.IsNullOrEmpty(characterName))
            { 
                return false; 
            }

            string pattern = @"^[A-Za-z0-9]{4,16}$";
            return Regex.IsMatch(characterName, pattern);
        }

        /// <summary>
        ///     Validates that the given job title is one of the predefined valid job titles.
        /// </summary>
        /// <param name="jobTitle">The job title to validate.</param>
        /// <returns><c>true</c> if the job title is valid; otherwise, <c>false</c>.</returns>
        public static bool IsValidJobTitle(string jobTitle)
        {
            if (string.IsNullOrEmpty(jobTitle))
            {
                return false;
            }

            foreach (var job in JOBS)
            {
                if (string.Equals(jobTitle, job, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks if the specified player's level is valid.
        ///     A valid player's level must be between <see cref="MIN_LEVEL"/> and <see cref="MAX_LEVEL"/>, inclusive.
        /// </summary>
        /// <param name="level">The player's level to validate.</param>
        /// <returns><c>true</c> if the player's level is valid; otherwise, <c>false</c>.</returns>
        public static bool IsValidLevel(int level)
        {
            return level >= MIN_LEVEL && level <= MAX_LEVEL;
        }
    }
}
