using System.ComponentModel.DataAnnotations;

namespace ConferenceBookingSystem.DTOs
{
    
    /// Response DTO - What the API returns to clients when listing rooms
    
    public class RoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string? Location { get; set; }
        public List<string> Amenities { get; set; } = new(); // ["Projector", "Whiteboard"]
    }
    
    
    /// Detailed room info with current availability
    
    public class RoomDetailDto : RoomDto
    {
        public List<TimeSlotDto> AvailableSlots { get; set; } = new();
    }
    
    public class TimeSlotDto
    {
        public DateTimeOffset  StartTime { get; set; }
        public DateTimeOffset  EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}