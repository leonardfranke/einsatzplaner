using DTO;
using Microsoft.AspNetCore.Components;
using Web.Services.Locations;

namespace Web.Views
{
    public class LocationAutocompleteBase : ComponentBase
    {
        [Inject]
        private ILocationsService _locationService { get; set; }
        
        [Parameter]
        public Action<object> LocationChangedFunc { get; set; }

        [Parameter]
        public List<Models.Location> Locations { get; set; }

        public async Task<IEnumerable<object>> Search(string searchText, CancellationToken cancellationToken) 
        {
            var filteredLocations = new List<object>();
            if(Locations != null)
            {
                if (string.IsNullOrEmpty(searchText))
                    filteredLocations.AddRange(Locations);
                else
                    filteredLocations.AddRange(Locations.Where(loc => loc.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
            }
            if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 6)
                return filteredLocations;
            var searchResult = await _locationService.SearchLocation(searchText);
            filteredLocations.AddRange(searchResult);
            return filteredLocations;
        }

        public string ItemToString(object item)
        {
            if (item is Models.Location loc)
                return loc.Name;
            else if (item is PlacesAutocompleteDTO place)
                return place.Text;
            else
                return item.ToString();
        }

        public async Task ValueChanged(object value)
        {
            if(value is PlacesAutocompleteDTO placeAuto)
            {
                var place = await _locationService.SearchPlace(placeAuto.PlaceId);
                var locationInfo = new LocationInfo
                {
                    Text = placeAuto.Text,
                    Latitude = place.Latitude,
                    Longitude = place.Longitude,
                };
                LocationChangedFunc.Invoke(locationInfo);
            }
            else
            {
                LocationChangedFunc.Invoke(value);
            }
        }
    }

    public class LocationInfo
    {
        public string Text { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
