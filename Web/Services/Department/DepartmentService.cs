﻿using Api.Models;
using DTO;
using System.Net.Http.Json;
using System.Web;
using Web.Converter;
using Web.Helper;
using Web.Manager;
using Web.Models;

namespace Web.Services
{
    public class DepartmentService : IDepartmentService
    {
        private IBackendManager _backendManager;

        public DepartmentService(IBackendManager backendManager)
        {
            _backendManager = backendManager;
        }

        public async Task<List<Department>> GetAll()
        {
            try
            {
                var response = await _backendManager.HttpClient.GetAsync(new Uri("/api/Department", UriKind.Relative));
                var departmentDTOs = await response.Content.ReadFromJsonAsync<List<DepartmentDTO>>();
                return DepartmentConverter.Convert(departmentDTOs);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                throw new Exception();
            }
            
        }

        public async Task<Department> GetById(string departmentId)
        {
            var response = await _backendManager.HttpClient.GetAsync(new Uri($"/api/Department/{departmentId}", UriKind.Relative));
            try
            {
                var departmentDTO = await response.Content.ReadFromJsonAsync<DepartmentDTO>();
                return DepartmentConverter.Convert(departmentDTO);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> IsMemberInDepartment(string memberId, string departmentId)
        {
            var query = QueryBuilder.Build(("memberId", memberId), ("departmentId", departmentId));
            var response = await _backendManager.HttpClient.GetAsync(new Uri($"/api/Department/IsMemberInDepartment{query}", UriKind.Relative));
            var departments = await response.Content.ReadFromJsonAsync<bool>();
            return departments;
        }

        public async Task<bool> RequestMembership(string departmentId, string userId)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("userId", userId));
            var response = await _backendManager.HttpClient.GetAsync(new Uri($"/api/Department/RequestMembership{query}", UriKind.Relative));
            var requested = await response.Content.ReadFromJsonAsync<bool>();
            return requested;
        }

        public async Task<bool> MembershipRequested(string departmentId, string userId)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("userId", userId));
            var response = await _backendManager.HttpClient.GetAsync(new Uri($"/api/Department/MembershipRequested{query}", UriKind.Relative));
            var requested = await response.Content.ReadFromJsonAsync<bool>();
            return requested;
        }

        public async Task<List<MembershipRequest>> MembershipRequests(string departmentId)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId));
            var response = await _backendManager.HttpClient.GetAsync(new Uri($"/api/Department/MembershipRequests{query}", UriKind.Relative));
            var requestDTOs = await response.Content.ReadFromJsonAsync<List<MembershipRequestDTO>>();
            return MembershipRequestConverter.Convert(requestDTOs);
        }

        public async Task AnswerRequest(string departmentId, string requestId, bool accept)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("requestId", requestId), ("acceptString", accept.ToString()));
            var response = await _backendManager.HttpClient.GetAsync(new Uri($"/api/Department/AnswerRequest{query}", UriKind.Relative));            
        }

        public Task RemoveMember(string departmentId, string memberId)
        {
            var query = QueryBuilder.Build(("departmentId", departmentId), ("memberId", memberId));
            return _backendManager.HttpClient.DeleteAsync(new Uri($"/api/Department{query}", UriKind.Relative));
        }
    }
}
