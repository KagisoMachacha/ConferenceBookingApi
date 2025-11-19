using System.ComponentModel.DataAnnotations;

namespace ConferenceBookingSystem.Models
{
    public class Room
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Range(1, 500)]
        public int Capacity { get; set; }
        
        [MaxLength(200)]
        public string? Location { get; set; }
        
        public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}