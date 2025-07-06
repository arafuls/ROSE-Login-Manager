using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace ROSE_Login_Manager.ViewModels;

/// <summary>
/// Base class for all ViewModels providing common MVVM functionality
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    protected readonly ILogger _logger;

    protected ViewModelBase(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Indicates whether the ViewModel is currently loading data
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Indicates whether the ViewModel has encountered an error
    /// </summary>
    [ObservableProperty]
    private bool _hasError;

    /// <summary>
    /// The error message to display
    /// </summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Sets an error state with the specified message
    /// </summary>
    /// <param name="message">The error message</param>
    protected void SetError(string message)
    {
        HasError = true;
        ErrorMessage = message;
        _logger.LogError("ViewModel error: {Message}", message);
    }

    /// <summary>
    /// Clears the error state
    /// </summary>
    protected void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Executes an async operation with loading and error handling
    /// </summary>
    /// <param name="operation">The async operation to execute</param>
    protected async Task ExecuteAsync(Func<Task> operation)
    {
        try
        {
            IsLoading = true;
            ClearError();
            await operation();
        }
        catch (Exception ex)
        {
            SetError($"An error occurred: {ex.Message}");
            _logger.LogError(ex, "Error executing async operation");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Executes an async operation with loading and error handling, returning a result
    /// </summary>
    /// <typeparam name="T">The type of the result</typeparam>
    /// <param name="operation">The async operation to execute</param>
    /// <param name="defaultValue">Default value to return on error</param>
    /// <returns>The operation result or default value</returns>
    protected async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, T defaultValue = default!)
    {
        try
        {
            IsLoading = true;
            ClearError();
            return await operation();
        }
        catch (Exception ex)
        {
            SetError($"An error occurred: {ex.Message}");
            _logger.LogError(ex, "Error executing async operation");
            return defaultValue;
        }
        finally
        {
            IsLoading = false;
        }
    }
} 