using DTO;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Web.Services.Locations;

namespace Web.Views
{
    public class ChangeLocationBase : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance _dialog { get; set; }

        [Parameter]
        public Models.Location? Location { get; set; }

        [Parameter]
        public string DepartmentId { get; set; }

        [Inject]
        public ILocationsService _locationService { get; set; }

        public string NewName { get; set; }
        public string NewOriginalName { get; set; }
        public double NewLatitude { get; set; }
        public double NewLongitude { get; set; }

        public bool IsSaving { get; set; }

        protected override void OnParametersSet()
        {
            if(Location != null)
            {
                NewName = Location.Name;
                NewLatitude = Location.Latitude;
                NewLongitude = Location.Longitude;
                NewOriginalName = Location.OriginalName;
            }
        }

        public void PositionChanged(object value)
        {
            if(value is LocationInfo locationInfo)
            {
                NewOriginalName = locationInfo.Text;
                NewLatitude = locationInfo.Latitude;
                NewLongitude = locationInfo.Longitude;
            }
        }

        public async Task SaveLocation()
        {
            IsSaving = true;
            var nameToTransmit = NewName != Location?.Name ? NewName : null;
            var originalNameToTransmit = NewOriginalName != Location?.OriginalName ? NewOriginalName : null;
            double? latitudeToTransmit = NewLatitude != Location?.Latitude ? NewLatitude : null;
            double? longitudeToTransmit = NewLongitude != Location?.Longitude ? NewLongitude : null;
            await _locationService.UpdateOrCreate(DepartmentId, Location?.Id, nameToTransmit, originalNameToTransmit, latitudeToTransmit, longitudeToTransmit);
            IsSaving = false;
            _dialog.Close(DialogResult.Ok(true));
        }
    }
}
