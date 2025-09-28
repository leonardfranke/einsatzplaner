using Api.DatabaseModels;
using DTO;
using GoogleApi.Entities.Common.Enums;
using GoogleApi.Entities.PlacesNew.AutoComplete.Request;
using GoogleApi.Entities.PlacesNew.Common.Enums;
using GoogleApi.Entities.PlacesNew.Details.Request;
using Microsoft.IdentityModel.Tokens;
using Supabase;

namespace Api.Manager
{
    public class LocationManager : ILocationManager
    {
        private Client _supabaseClient;
        private string _placesKey;

        public LocationManager(Client supabaseClient, IHttpClientFactory httpClientFactory)
        {
            _supabaseClient = supabaseClient;
            _placesKey = Environment.GetEnvironmentVariable("PLACES_API_KEY");
        }

        public async Task Delete(string departmentId, string locationId)
        {
            await _supabaseClient.From<Location>().Where(loc => loc.DepartmentId == departmentId).Where(loc => loc.Id == locationId).Delete();
        }

        public async Task<List<Location>> GetAll(string departmentId)
        {
            var response = await _supabaseClient.From<Location>().Get();
            return response.Models;
        }

        public Task<Location> GetById(string departmentId, string locationId)
        {
            return _supabaseClient.From<Location>().Where(loc => loc.DepartmentId == departmentId).Where(loc => loc.Id == locationId).Single();            
        }

        public Task UpdateOrCreate(UpdateLocationDTO updateLocation)
        {
            if(updateLocation.LocationId.IsNullOrEmpty())
            {
                var loc = new Location
                {
                    DepartmentId = updateLocation.DepartmentId,
                    Name = updateLocation.Name,
                    OriginalName = updateLocation.OriginalName,
                    Latitude = updateLocation.Latitude.Value,
                    Longitude = updateLocation.Longitude.Value
                };    
                return _supabaseClient.From<Location>().Insert(loc);
            }
            else
            {
                var loc = new Location
                {
                    DepartmentId = updateLocation.DepartmentId,
                    Id = updateLocation.LocationId,
                };
                if (!string.IsNullOrEmpty(updateLocation.Name))
                    loc.Name = updateLocation.Name;
                if (!string.IsNullOrEmpty(updateLocation.OriginalName))
                    loc.OriginalName = updateLocation.OriginalName;
                if (updateLocation.Latitude != null)
                    loc.Latitude = updateLocation.Latitude.Value;
                if (updateLocation.Longitude != null)
                    loc.Longitude = updateLocation.Longitude.Value;
                return _supabaseClient.From<Location>().Update(loc);
            }
        }

        public async Task<IEnumerable<PlacesAutocompleteDTO>> Search(string query)
        {
            var request = new PlacesNewAutoCompleteRequest
            {
                Input = query,
                Language = Language.German,
                Key = _placesKey,
                FieldMask = "suggestions.placePrediction.text.text,suggestions.placePrediction.placeId"
            };
            var response = await GoogleApi.GooglePlacesNew.AutoComplete.QueryAsync(request);
            var data = response.Suggestions.Select(result =>
            {
                var placePrediction = result.PlacePrediction;
                return new PlacesAutocompleteDTO
                {
                    PlaceId = placePrediction.PlaceId,
                    Text = placePrediction.Text.Text
                };
                
            });
            return data;
        }

        public async Task<PlacesPlaceDetailDTO> SearchPlace(string placeId)
        {
            var request = new PlacesNewDetailsRequest
            {
                Language = Language.German,
                Key = _placesKey,
                PlaceId = placeId,
                FieldMask = "location"
            };
            var response = await GoogleApi.GooglePlacesNew.Details.QueryAsync(request);
            var location = response.Place.Location;
            return new PlacesPlaceDetailDTO
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };
        }
    }
}
