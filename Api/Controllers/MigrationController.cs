using Api.DataMigrations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MigrationController : ControllerBase
    {
        private Migration _migration;

        public MigrationController(Migration migration)
        {
            _migration = migration;
        }

        [HttpGet("MigrateRoles")]
        public Task MigrateRoles()
        {
            return _migration.MigrateRoles();
        }
    }
}
