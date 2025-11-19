using System.ComponentModel.DataAnnotations;

namespace ConferenceBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }
        
        public int RoomId { get; set; }
        public Room? Room { get; set; }
        
        public int UserId { get; set; }
        public User? User { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        
        public DateTimeOffset  StartTime { get; set; }
        public DateTimeOffset  EndTime { get; set; }
        
        [MaxLength(20)]
        public string Status { get; set; } = BookingStatus.Confirmed; // "confirmed", "cancelled", "rescheduled", "booking updated"
        
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
    
    public static class BookingStatus
    {
        public const string Confirmed = "confirmed";
        public const string Cancelled = "cancelled";

        public const string Rescheduled = "rescheduled";
        public const string BookingUpdated = "booking updated";
    }
}