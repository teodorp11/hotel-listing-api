using HotelListing.API.Data;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private static List<Hotel> hotels = new List<Hotel>
        {
            new Hotel { Id = 1, Name = "Hotel A", Address = "Street S1", Rating = 1 },
            new Hotel { Id = 2, Name = "Hotel B", Address = "Street S2", Rating = 2},
            new Hotel { Id = 3, Name = "Hotel C", Address = "Street S3", Rating = 3},
            new Hotel { Id = 4, Name = "Hotel D", Address = "Street S4", Rating = 4},
            new Hotel { Id = 5, Name = "Hotel E", Address = "Street S5", Rating = 5}
        };

        // GET: api/<HotelsController>
        [HttpGet]
        public ActionResult<IEnumerable<Hotel>> Get()
        {
            return Ok(hotels);
        }

        // GET api/<HotelsController>/5
        [HttpGet("{id}")]
        public ActionResult<Hotel> Get(int id)
        {
            var hotel = hotels.FirstOrDefault(h => h.Id == id);


            if (hotel == null)
            {
                return NotFound(new { message = "Hotel not found"});
            }

            return Ok(hotel);
        }

        // POST api/<HotelsController>
        [HttpPost]
        public ActionResult<Hotel> Post([FromBody] Hotel newHotel)
        {
            if (hotels.Any(h => h.Id == newHotel.Id))
            {
                return BadRequest("Existing hotel with the same ID");
            }

            hotels.Add(newHotel);

            return CreatedAtAction(nameof(Get), new { id = newHotel.Id }, newHotel);
        }

        // PUT api/<HotelsController>/5
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Hotel updatedHotel)
        {
            var existingHotel = hotels.FirstOrDefault(h => h.Id == id);

            if (existingHotel == null)
            {
                return NotFound(new { message = "Hotel not found" });
            }

            existingHotel.Name = updatedHotel.Name;
            existingHotel.Address = updatedHotel.Address;
            existingHotel.Rating = updatedHotel.Rating;

            return NoContent();
        }

        // DELETE api/<HotelsController>/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var hotel = hotels.FirstOrDefault(h => h.Id == id);


            if (hotel == null)
            {
                return NotFound(new { message = "Hotel not found" });
            }

            hotels.Remove(hotel);

            return NoContent();
        }
    }
}
