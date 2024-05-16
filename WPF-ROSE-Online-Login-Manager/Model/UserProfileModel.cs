using System.Security;



namespace ROSE_Online_Login_Manager.Model
{
    public class UserProfileModel
    {
        public bool?           ProfileStatus    { get; set; }
        public required string ProfileName      { get; set; }
        public required string ProfileEmail     { get; set; }
        public required string ProfilePassword  { get; set; }
        public required string ProfileIV        { get; set; }

        public UserProfileModel()
        {
            
        }

    }
}
