using DTO;
using Web.Models;

namespace Web.Converter
{
    public class LocationConverter
    {
        public static List<Location> Convert(List<LocationDTO> locations)
        {
            return locations.Select(Convert).ToList();
        }

        public static Location Convert(LocationDTO location)
        {
            if (location == null)
                return null;
            return new Location
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
