using ROSE_Online_Login_Manager.Model;



namespace ROSE_Online_Login_Manager.Services.Infrastructure
{
    public class ProfileAddedUpdateMessage(UserProfileModel newProfile)
    {
        public UserProfileModel Profile { get; set; } = newProfile;
    }



    public class ProfileDeletedUpdateMessage(UserProfileModel deletedProfile)
    {
        public UserProfileModel Profile { get; set; } = deletedProfile;
    }
}
