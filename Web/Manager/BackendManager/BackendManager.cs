
namespace Web.Manager
{
    public class BackendManager : IBackendManager
    {
        private IConfiguration _configuration;

        public BackendManager(IConfiguration configuration) 
        {
            _configuration = configuration;
            var backendAddress = _configuration["BACKEND_ADDRESS"];
            if(backendAddress == null)
            {
                throw new ApplicationException("Backend address could not be found in configuration file");
            }
            HttpClient = new HttpClient()
            {
                BaseAddress = new Uri(backendAddress)
            };
        }

        public HttpClient HttpClient { get; set;}
    }
}
