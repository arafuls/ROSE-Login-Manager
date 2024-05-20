



namespace ROSE_Online_Login_Manager.Model
{
    /// <summary>
    ///     Represents a user profile model containing information such as status, name, email, password, and initialization vector.
    /// </summary>
    public class UserProfileModel
    {
        public bool?           ProfileStatus    { get; set; }
        public required string ProfileName      { get; set; }
        public required string ProfileEmail     { get; set; }
        public required string ProfilePassword  { get; set; }
        public required string ProfileIV        { get; set; }



        /// <summary>
        ///     Initializes a new instance of the <see cref="UserProfileModel"/> class.
        /// </summary>
        public UserProfileModel() { }
    }
}
