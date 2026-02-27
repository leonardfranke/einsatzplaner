using Api.DataMigrations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class MigrationController : ControllerBase
    {
        private MembersMigration _membersMigration;

        public MigrationController(MembersMigration membersMigration)
        {
            _membersMigration = membersMigration;
        }

        [HttpGet("MigrateMembers")]
        public Task MigrateMembers()
        {
            return _membersMigration.Migrate();
        }
    }
}
