﻿using Microsoft.AspNetCore.Components;
using System.Linq;
using Web.Checks;
using Web.Models;
using Web.Services;

namespace Web.Pages
{
    public class GamePageBase : ComponentBase
    {
        [Parameter]
        public string DepartmentUrl { get; set; }

        [Parameter]
        public string GameId { get; set; }

        public Models.Event Game { get; private set; }

        public Task<Event> GameTask { get; private set; }

        public List<Models.Helper> Helpers { get; private set; }


        [Inject]
        private ILoginCheck _loginCheck { get; set; }
        [Inject]
        private IEventService _gameService { get; set; }

        [Inject]
        private IRoleService _roleService { get; set; }

        [Inject]
        private IEventCategoryService _eventCategoryService { get; set; }

        [Inject]
        private IHelperService _helperService { get; set; }

        [Inject]
        private IMemberService _memberService { get; set; }

        [Inject]
        private IGroupService _groupService { get; set; }
        [Inject]
        private IDepartmentUrlCheck _departmentUrlCheck { get; set; }

        private List<Models.Member> _members;
        private List<Group> _groups;
        private List<Role> _roles;
        private Models.EventCategory _eventCategory;
        private Models.Group _group;

        protected override async Task OnParametersSetAsync()
        {
            if (await _departmentUrlCheck.LogIntoDepartment(DepartmentUrl) is not Models.Department department)
                return;

            GameTask = _gameService.GetEvent(department.Id, GameId);
            Game = await GameTask;
            if (Game == null)
                return;

            if (!await _loginCheck.CheckLogin(department))
                return;

            var helpersTask = _helperService.GetAll(department.Id, Game.Id);
            var groupsTask = _groupService.GetAll(department.Id);
            var rolesTask = _roleService.GetAll(department.Id);
            var membersTask = _memberService.GetAll(department.Id);
            _groups = await groupsTask;
            _group = _groups?.Find(group => group.Id == Game.GroupId);
            if (!string.IsNullOrEmpty(Game.EventCategoryId))
            {
                var eventCategoriesTask = _eventCategoryService.GetById(department.Id, Game.EventCategoryId);
                _eventCategory = await eventCategoriesTask;
            }            

            Helpers = await helpersTask;
            _roles = await rolesTask;
            _members = await membersTask;            
        }

        public Role GetRoleById(string roleId)
        {
            return _roles.Find(role => role.Id == roleId);
        }

        protected string GetDisplayMemberNames(List<string> memberIds)
        {
            if (memberIds == null || memberIds.Count == 0)
                return "-";
            return string.Join(", ", memberIds.Select(id => _members.FirstOrDefault(member => member.Id == id)?.Name ?? "Ohne Name"));
        }

        protected string GetDisplayGroupNames(List<string> groupIds)
        {
            if (groupIds == null || groupIds.Count == 0)
                return "-";
            return string.Join(", ", groupIds.Select(id => _groups.FirstOrDefault(group => group.Id == id)?.Name ?? "Ohne Name"));
        }

        protected string GetPageTitle()
        {
            var pageTitles = new List<string>();
            var groupId = Game?.GroupId;
            if(!string.IsNullOrEmpty(groupId))
            {
                pageTitles.Add((_group == null) ? "Unbekannte Gruppe" : _group.Name);
            }

            if (!string.IsNullOrEmpty(Game.EventCategoryId))
            {
                pageTitles.Add((_eventCategory == null) ? "Unbekannte Eventkategorie" : _eventCategory.Name);
            }

            return string.Join(" - ", pageTitles);
        }
    }
}
