using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Data
{
    public class Hotel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set;  }
        public double Rating { get; set; }
        public decimal PerNightRate { get; set; }

        // Foreign Key for Country
        public int CountryId { get; set; }
        
        // Reference to the parent Country entity (Navigation Property)
        public Country? Country { get; set; }

        public ICollection<HotelAdmin> Admins { get; set; } = [];
        public ICollection<Booking> Bookings { get; set; } = [];
    }
}
