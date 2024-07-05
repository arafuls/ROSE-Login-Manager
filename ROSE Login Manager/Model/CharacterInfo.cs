


namespace ROSE_Login_Manager.Model
{
    /// <summary>
    ///     Represents information about a character, including job title, level, and character name.
    /// </summary>
    public struct CharacterInfo
    {
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
        ///     Initializes a new instance of the <see cref="CharacterInfo"/> struct.
        /// </summary>
        /// <param name="jobTitle">The job title of the character.</param>
        /// <param name="level">The level of the character.</param>
        /// <param name="characterName">The name of the character.</param>
        public CharacterInfo(string jobTitle, int level, string characterName)
        {
            JobTitle = jobTitle;
            Level = level;
            CharacterName = characterName;
        }

        /// <summary>
        ///     Returns a string representation of the <see cref="CharacterInfo"/> struct.
        /// </summary>
        /// <returns>A string that represents the character information.</returns>
        public override readonly string ToString()
        {
            return $"{CharacterName} ({JobTitle}) - Level {Level}";
        }

        /// <summary>
        ///     Checks if the stored data for job title, character name, and level are valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> If all conditions are met: job title is not null or empty,
        ///   character name is not null or empty, and level is greater than zero;
        ///   otherwise, <c>false</c>.
        /// </returns>
        public readonly bool ValidData()
        {
            if (string.IsNullOrEmpty(JobTitle) || string.IsNullOrEmpty(CharacterName) || 0 == Level)
            {
                return false;
            }

            return true;
        }
    }
}
