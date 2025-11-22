using DTO;
using System.Net.Http.Json;
using Web.Converter;
using Web.Helper;
using Web.Models;

namespace Web.Services.Locations
{
    public class LocationsService : ILocationsService
    {
        private HttpClient _httpClient;

        public LocationsService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BACKEND");
        }

        public Task DeleteLocation(string departmentId, string locationId)
        {
            return _httpClient.DeleteAsync(new Uri($"/api/Location/{departmentId}/{locationId}", UriKind.Relative));
        }

        public async Task<List<Location>> GetAll(string departmentId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/Location/{departmentId}", UriKind.Relative));
            var locations = await response.Content.ReadFromJsonAsync<List<LocationDTO>>();
            return LocationConverter.Convert(locations);
        }

        public async Task<Location> GetById(string departmentId, string locationId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/Location/{departmentId}/{locationId}", UriKind.Relative));
            var locations = await response.Content.ReadFromJsonAsync<LocationDTO>();
            return LocationConverter.Convert(locations);            
        }

        public async Task<List<PlacesAutocompleteDTO>> SearchLocation(string searchText)
        {
            var query = QueryBuilder.Build(("searchText", searchText));
            var response = await _httpClient.GetAsync(new Uri($"/api/Location/search{query}", UriKind.Relative));
            var stringCon = await response.Content.ReadAsStringAsync();
            return await response.Content.ReadFromJsonAsync<List<PlacesAutocompleteDTO>> ();
        }

        public async Task<PlacesPlaceDetailDTO> SearchPlace(string placeId)
        {
            var response = await _httpClient.GetAsync(new Uri($"/api/Location/search/{placeId}", UriKind.Relative));
            var stringCon = await response.Content.ReadAsStringAsync();
            return await response.Content.ReadFromJsonAsync<PlacesPlaceDetailDTO> ();
        }

        public async Task UpdateOrCreate(string departmentId, string? locationId, string? name, string? originalName, double? latitude, double? longitude)
        {
            var updateCategoryDTO = new UpdateLocationDTO
            {
                DepartmentId = departmentId,
                LocationId = locationId,
                Name = name,
                Latitude = latitude,
                Longitude = longitude,
                OriginalName = originalName
            };
            var content = JsonContent.Create(updateCategoryDTO);
            var response = await _httpClient.PostAsync(new Uri($"/api/Location", UriKind.Relative), content);
        }
    }
}
