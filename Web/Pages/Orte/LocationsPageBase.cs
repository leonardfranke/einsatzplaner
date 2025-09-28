using LeafletForBlazor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Web.Checks;
using Web.Models;
using Web.Services.Locations;
using Web.Views.Location;

namespace Web.Pages.Orte
{
    public class LocationsPageBase : ComponentBase
    {
        [Parameter]
        public string DepartmentUrl { get; set; }

        [Inject]
        public ILocationsService _locationsService { get; set; }

        [Inject]
        private IDepartmentUrlCheck _departmentUrlCheck { get; set; }

        [Inject]
        private ILoginCheck _loginCheck { get; set; }

        [Inject]
        private IDialogService _dialogService { get; set; }

        public List<Location> Locations { get; set; }

        private string _departmentId;

        protected override async Task OnInitializedAsync()
        {
            if (await _departmentUrlCheck.LogIntoDepartment(DepartmentUrl) is not Models.Department department)
                return;
            _departmentId = department.Id;
            if (!await _loginCheck.CheckLogin(DepartmentUrl, department, true))
                return;

            await ReloadLocations();
        }

        private async Task ReloadLocations()
        {
            Locations = await _locationsService.GetAll(_departmentId);
        }

        public async Task OpenEditLocation(Location? location)
        {
            var parameter = new DialogParameters<ChangeLocation>()
            {
                { x => x.Location, location },
                { x => x.DepartmentId, _departmentId }
            };
            var options = new DialogOptions
            {
                CloseButton = true,
                CloseOnEscapeKey = true,
            };
            var dialog = await _dialogService.ShowAsync<ChangeLocation>(null, parameter, options);
            var result = await dialog.Result;
            await ReloadLocations();
        }

        public RealTimeMap.LoadParameters GetMapParameter(Location location)
        {
            return new RealTimeMap.LoadParameters
            {
                location = new RealTimeMap.Location
                {
                    latitude = location.Latitude,
                    longitude = location.Longitude
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

        //public void InitMap(RealTimeMap.MapEventArgs args, Location loc)
        //{
        //    args.sender.movePoint([loc.Latitude, loc.Longitude]);
        //}
    }
}
