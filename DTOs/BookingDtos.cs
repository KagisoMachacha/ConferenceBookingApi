using System.ComponentModel.DataAnnotations;

namespace ConferenceBookingSystem.DTOs
{
    
    /// Request DTO - What clients send when creating a booking
    /// Notice: No Id, CreatedAt, UpdatedAt - those are server-generated
    
    public class CreateBookingRequest
    {
        [Required]
        public int RoomId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
    }
    
    
    /// Request DTO - What clients send when updating a booking
    /// All fields optional - only update what's provided
    
    public class UpdateBookingRequest
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        
        [MaxLength(200)]
        public string? Title { get; set; }
    }
    
    
    /// Response DTO - What the API returns about a booking
    
    public class BookingDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty; // Denormalized for convenience
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
    
    
    /// Response for availability check
    
    public class AvailabilityResponse
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public DateTimeOffset  Date { get; set; }
        public List<BookingSlotDto> Bookings { get; set; } = new();
        public bool IsAvailable { get; set; } // Is there ANY availability today?
        public bool HasAnyBookings { get; set; } // True if at least one booking exists; false means completely free
    }
    
    public class BookingSlotDto
    {
        public DateTimeOffset  StartTime { get; set; }
        public DateTimeOffset  EndTime { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}