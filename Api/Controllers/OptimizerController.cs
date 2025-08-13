using Api.Manager;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OptimizerController : ControllerBase
    {
        private IOptimizationManager _optimizationManager;

        public OptimizerController(IOptimizationManager optimizationManager)
        {
            _optimizationManager = optimizationManager;
        }

        [HttpGet("{departmentId}")]
        public Task OptimizeDepartment(string departmentId)
        {
            return _optimizationManager.OptimizeDepartment(departmentId);
        }
    }
}
