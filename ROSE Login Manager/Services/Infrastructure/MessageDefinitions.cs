


namespace ROSE_Login_Manager.Services.Infrastructure
{
    /// <summary>
    ///     Represents a message indicating that a setting has changed.
    /// </summary>
    /// <param name="key">The key of the setting that has changed.</param>
    /// <param name="value">The new value of the setting.</param>
    public class SettingChangedMessage<T>(string key, T value)
    {
        public string Key { get; } = key;
        public T Value { get; } = value;
    }



    /// <summary>
    ///     Represents a message indicating a change in the status of the "Display Email" checkbox.
    /// </summary>
    /// <param name="isChecked">True if the "Display Email" checkbox is checked; otherwise, false.</param>
    public class DisplayEmailCheckedMessage(bool isChecked)
    {
        public bool IsChecked { get; } = isChecked;
    }



    /// <summary>
    ///     Represents a message indicating a change in the status of the "Mask Email" checkbox.
    /// </summary>
    /// <param name="isChecked">True if the "Mask Email" checkbox is checked; otherwise, false.</param>
    public class MaskEmailCheckedMessage(bool isChecked)
    {
        public bool IsChecked { get; } = isChecked;
    }



    /// <summary>
    ///     Represents a message indicating a change in the status of the "Skip Planet Cutscene" checkbox.
    /// </summary>
    /// <param name="isChecked">True if the "Mask Email" checkbox is checked; otherwise, false.</param>
    public class SkipPlanetCutsceneCheckedMessage(bool isChecked)
    {
        public bool IsChecked { get; } = isChecked;
    }



    /// <summary>
    ///     Initializes a new instance of the <see cref="ProgressMessage"/> class with the specified progress percentage and current file name.
    /// </summary>
    /// <param name="progressPercentage">The progress percentage of the task.</param>
    /// <param name="currentFileName">The name of the current file being processed.</param>
    public class ProgressMessage(int progressPercentage, string currentFileName)
    {
        public int ProgressPercentage { get; } = progressPercentage;
        public string CurrentFileName { get; } = currentFileName;
    }



    /// <summary>
    ///     Represents a message indicating a change in the view model being displayed.
    /// </summary>
    public class ViewChangedMessage(string viewModelName)
    {
        public string ViewModelName { get; } = viewModelName;
    }



    /// <summary>
    ///     Represents a message indicating a change in the game folder.
    /// </summary>
    public class GameFolderChanged()
    {
        // This class does not contain any properties or methods.
        // It serves as a marker class to indicate a change in the game folder.
    }



    /// <summary>
    ///     Message class used to request the progress value.
    /// </summary>
    public class ProgressRequestMessage
    {
        // This class does not contain any properties or methods.
        // It serves as a marker class to indicate a request.
    }



    /// <summary>
    ///     Initializes a new instance of the ProgressResponseMessage class with the specified progress value.
    /// </summary>
    /// <param name="progress">The progress value to be sent.</param>
    public class ProgressResponseMessage(int progress)
    {
        public int Progress { get; set; } = progress;
    }



    /// <summary>
    ///     Represents a message indicating a new event has been added to the logs.
    /// </summary>
    public class EventLogAddedMessage()
    {
        // This class does not contain any properties or methods.
        // It serves as a marker class to indicate a request.
    }
}
