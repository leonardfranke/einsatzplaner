using Microsoft.AspNetCore.Components;

namespace Web.Layout
{
    public class NavMenuBase : LayoutComponentBase
    {
        [Inject]
        public NavigationManager _navigationManager { get; set; }

        public string DepartmentUrl { get; set; }

        protected override void OnInitialized()
        {
            var relativePath = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
            var segments = relativePath.Split('/');
            DepartmentUrl = segments[0];
        }
    }
}
