



namespace ROSE_Login_Manager.Model
{
    /// <summary>
    ///     Represents a user profile model containing information such as status, name, email, password, and initialization vector.
    /// </summary>
    public class UserProfileModel : IDisposable
    {
        public bool? ProfileStatus { get; set; }
        public required string ProfileName { get; set; }
        public required string ProfileEmail { get; set; }
        public required string ProfilePassword { get; set; }
        public required string ProfileIV { get; set; }
        public int ProfileOrder { get; set; }



        /// <summary>
        ///     Releases resources used by the UserProfileModel and clears sensitive data.
        /// </summary>
        /// <remarks>
        ///     This method clears sensitive data by calling the <see cref="ClearSensitiveData"/> method and suppresses finalization.
        ///     This is part of the implementation of the IDisposable interface.
        /// </remarks>
        public void Dispose()
        {
            ClearSensitiveData();
            GC.SuppressFinalize(this);
        }



        /// <summary>
        ///     Clears sensitive data from the UserProfileModel.
        /// </summary>
        /// <remarks>
        ///     This method sets the <see cref="ProfilePassword"/> and <see cref="ProfileIV"/> properties to an empty string
        ///     to ensure that sensitive data is not left in memory.
        /// </remarks>
        private void ClearSensitiveData()
        {
            if (ProfilePassword != null)
            {
                ProfilePassword = string.Empty;
            }

            if (ProfileIV != null)
            {
                ProfileIV = string.Empty;
            }
        }



        /// <summary>
        ///     Finalizer for the UserProfileModel class.
        /// </summary>
        /// <remarks>
        ///     This destructor is called by the garbage collector when the object is being finalized.
        ///     It calls the <see cref="ClearSensitiveData"/> method to ensure that sensitive data is cleared from memory.
        /// </remarks>
        ~UserProfileModel()
        {
            ClearSensitiveData();
        }
    }
}
