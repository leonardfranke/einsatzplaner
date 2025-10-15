using LeafletForBlazor;
using Web.Models;

namespace Web.Helper
{
    public static class MapHelper
    {
        public static RealTimeMap.LoadParameters GetMapParameter(Location location) => GetMapParameter(location.Latitude, location.Longitude);

        public static RealTimeMap.LoadParameters GetMapParameter(double latitude, double longitude)
        {
            return new RealTimeMap.LoadParameters
            {
                location = new RealTimeMap.Location
                {
                    latitude = latitude,
                    longitude = longitude
                },
                zoomLevel = 17,
                basemap = new RealTimeMap.Basemap
                {
                    basemapLayers = new List<RealTimeMap.BasemapLayer>
                    {
                        new RealTimeMap.BasemapLayer
                        {
                            url = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                            attribution = "© OpenStreetMap",
                            title = "OSM",
                            detectRetina = true
                        }
                    }
                }
            };
        }
    }
}
