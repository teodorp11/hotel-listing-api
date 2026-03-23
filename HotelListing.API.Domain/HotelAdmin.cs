namespace HotelListing.API.Domain;

public class HotelAdmin
{
    public int Id { get; set; }

    public Hotel? Hotel { get; set; } // navigation property
    public int HotelId { get; set; } // foreign key Id

    public ApplicationUser? User { get; set; } // navigation property
    public required string UserId { get; set; } // foreign key Id
}