using Api.DatabaseModels;
using DTO;

namespace Api.Converter
{
    public class LocationConverter
    {
        public static List<LocationDTO> Convert(List<Location> locations)
        {
            return locations.Select(Convert).ToList();
        }

        public static LocationDTO Convert(Location location)
        {
            if (location == null)
                return null;
            return new LocationDTO
            {
                DepartmentId = location.DepartmentId,
                Id = location.Id,
                Name = location.Name,
                OriginalName = location.OriginalName,
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };
        }
    }
}
