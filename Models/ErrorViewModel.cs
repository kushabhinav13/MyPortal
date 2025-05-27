namespace Myportal.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExceptionDetails { get; set; }
    public int StatusCode { get; set; } = StatusCodes.Status500InternalServerError;
    public string? StatusCodeDescription { get; set; }
    public string? ExceptionType { get; set; }
    
    public DateTime ErrorTime { get; set; } = DateTime.UtcNow;
    public string? Path { get; set; }
    public string? StackTrace { get; set; }
    public Dictionary<string, string>? AdditionalData { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public bool ShowDetails => !string.IsNullOrEmpty(ExceptionDetails);
    public bool ShowStackTrace => !string.IsNullOrEmpty(StackTrace);
    
    // Helper method for development environment
    public bool ShouldShowDetails(bool isDevelopmentEnvironment) 
        => isDevelopmentEnvironment && StatusCode >= StatusCodes.Status500InternalServerError;
}