using Api.Converter;
using Api.Manager;
using DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private ILocationManager _locationManager;

        public LocationController(ILocationManager locationManager)
        {
            _locationManager = locationManager;
        }

        [HttpGet("search")]
        public Task<IEnumerable<PlacesAutocompleteDTO>> SearchLocation([FromQuery] string searchText)
        {
            return _locationManager.Search(searchText);
        }

        [HttpGet("search/{placeId}")]
        public Task<PlacesPlaceDetailDTO> SearchPlace([FromRoute] string placeId)
        {
            return _locationManager.SearchPlace(placeId);
        }

        [HttpGet("{departmentId}")]
        public async Task<List<LocationDTO>> GetAll([FromRoute] string departmentId)
        {
            var locations = await _locationManager.GetAll(departmentId);
            return LocationConverter.Convert(locations);
        }

        [HttpGet("{departmentId}/{locationId}")]
        public async Task<LocationDTO> GetById([FromRoute] string departmentId,[FromRoute] string locationId)
        {
            var location = await _locationManager.GetById(departmentId, locationId);
            return LocationConverter.Convert(location);
        }

        [HttpDelete("{departmentId}/{locationId}")]
        public Task DeleteLocation([FromRoute] string departmentId, [FromRoute] string locationId) 
        {
            return _locationManager.Delete(departmentId, locationId);
        }

        [HttpPost]
        public Task UpdateOrCreate([FromBody] UpdateLocationDTO updateLocation)
        {
            return _locationManager.UpdateOrCreate(updateLocation);
        }
    }
}
