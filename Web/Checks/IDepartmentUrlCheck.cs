﻿using Web.Models;

namespace Web.Checks
{
    public interface IDepartmentUrlCheck
    {
        public Task<Department> CheckDepartmentUrl(string departmentId);
    }
}
