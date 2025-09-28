using Api.DatabaseModels;
using DTO;

namespace Api.Manager
{
    public interface ILocationManager
    {
        public Task<List<Location>> GetAll(string departmentId);

        public Task<IEnumerable<PlacesAutocompleteDTO>> Search(string query);

        public Task<PlacesPlaceDetailDTO> SearchPlace(string placeId);

        public Task<Location> GetById(string departmentId, string locationId);

        public Task UpdateOrCreate(UpdateLocationDTO updateLocation);

        public Task Delete(string departmentId, string locationId);
    }
}
