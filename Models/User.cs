using System.ComponentModel.DataAnnotations;

namespace ConferenceBookingSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}