namespace ConferenceBookingSystem.DTOs
{
    
    /// Standardized error response for all API errors
    
    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string? Message { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
    }
}