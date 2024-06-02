


namespace ROSE_Login_Manager.Services.Infrastructure
{
    /// <summary>
    ///     Represents a message indicating a change in a setting value.
    /// </summary>
    /// <typeparam name="T">The type of the setting value.</typeparam>
    public class SettingChangedMessage<T>
    {
        public string Key { get; }
        public T Value { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SettingChangedMessage{T}"/> class.
        /// </summary>
        /// <param name="key">The key of the setting that changed.</param>
        /// <param name="value">The new value of the setting.</param>
        public SettingChangedMessage(string key, T value)
        {
            Key = key;
            Value = value;
        }
    }




    /// <summary>
    ///     Represents a message indicating a change in the status of the "Display Email" checkbox.
    /// </summary>
    public class DisplayEmailCheckedMessage
    {
        public bool IsChecked { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayEmailCheckedMessage"/> class with the specified checked status.
        /// </summary>
        /// <param name="isChecked">True if the "Display Email" checkbox is checked; otherwise, false.</param>
        public DisplayEmailCheckedMessage(bool isChecked)
        {
            IsChecked = isChecked;
        }
    }




    /// <summary>
    ///     Represents a message indicating a change in the status of the "Mask Email" checkbox.
    /// </summary>
    public class MaskEmailCheckedMessage
    {
        public bool IsChecked { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayEmailCheckedMessage"/> class with the specified checked status.
        /// </summary>
        /// <param name="isChecked">True if the "Mask Email" checkbox is checked; otherwise, false.</param>
        public MaskEmailCheckedMessage(bool isChecked)
        {
            IsChecked = isChecked;
        }
    }




    public class ProgressMessage
    {
        public int ProgressPercentage { get; }

        public ProgressMessage(int progressPercentage)
        {
            ProgressPercentage = progressPercentage;
        }
    }
}
