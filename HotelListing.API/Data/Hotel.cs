namespace HotelListing.API.Data
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set;  }
        public double Rating { set; get; }

        // Foreign Key for Country
        public int CountryId { get; set; }
        // Reference to the parent Country entity (Navigation Property)
        public Country? Country { get; set; }
    }
}
