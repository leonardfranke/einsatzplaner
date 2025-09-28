using DTO;
using Web.Models;

namespace Web.Services.Locations
{
    public interface ILocationsService
    {
        public Task<List<Location>> GetAll(string departmentId);

        public Task<List<PlacesAutocompleteDTO>> SearchLocation(string searchText);

        public Task<PlacesPlaceDetailDTO> SearchPlace(string placeId);

        //public Task<Location> GetById(string departmentId, string locationId);

        public Task DeleteLocation(string departmentId, string locationId);

        public Task UpdateOrCreate(string departmentId, string? locationId, string? name, string? originalName, double? latitude, double? longitude);
    }
}
