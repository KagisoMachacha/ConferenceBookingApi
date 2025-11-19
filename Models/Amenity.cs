using System.ComponentModel.DataAnnotations;

namespace ConferenceBookingSystem.Models
{
    public class Amenity
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // "Projector", "Whiteboard", "Video Conference"
        
        // Navigation properties
        public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
    }
}