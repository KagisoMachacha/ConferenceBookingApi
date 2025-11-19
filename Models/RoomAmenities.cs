namespace ConferenceBookingSystem.Models
{
    
    /// Join table for many-to-many relationship between Rooms and Amenities
    /// A room can have multiple amenities, an amenity can belong to multiple rooms
    
    public class RoomAmenity
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int AmenityId { get; set; }
        
        // Navigation properties
        public Room? Room { get; set; }
        public Amenity? Amenity { get; set; }
    }
}