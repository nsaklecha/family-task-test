using FamilyTask.Shared.Components;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using WebClient.Abstractions;
using WebClient.Services;
using AutoMapper;
using Domain.ViewModel;
using Domain.Queries;
using System.Collections;

namespace WebClient.Pages
{
    public class ManageTasksBase : ComponentBase
    {
        public ManageTasksBase()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MemberVm, FamilyMember>()
                .ForMember(dst => dst.avtar, src => src.MapFrom(e => e.Avatar))
                .ReverseMap();
            });
            Mapper = config.CreateMapper();
        }

        protected List<TaskModel> tasks = new List<TaskModel>();
        protected List<FamilyMember> familyMembers = new List<FamilyMember>();
        protected FamilyMember[] members;
        protected MenuItem[] leftMenuItem;
        protected TaskModel[] tasksToShow;
        protected TaskModel[] allTasks;
        protected bool isLoaded;
        protected bool showLister;
        protected bool showCreator;
        protected string taskstring = string.Empty;
        protected static IMapper Mapper { get; private set; }
        TaskModel dragTaskModel = new TaskModel();
        [Inject]
        public ITaskDataService TaskDataService { get; set; }
        [Inject]
        public IMemberDataService MemberDataService { get; set; }
        protected override async Task OnInitializedAsync()
        {
            #region Members

            var memberResult = await MemberDataService.GetAllMembers();

            if (memberResult != null && memberResult.Payload != null && memberResult.Payload.Any())
            {
                foreach (var item in memberResult.Payload)
                {
                    if (item.Id != Guid.Empty)
                    {
                        familyMembers.Add(Mapper.Map<FamilyMember>(item));
                    }
                }
            }

            members = familyMembers.Cast<FamilyMember>().ToArray();

            leftMenuItem = new MenuItem[members.Length + 1];
            leftMenuItem[0] = new MenuItem
            {
                label = "All Tasks",
                referenceId = Guid.Empty,
                isActive = true
            };
            leftMenuItem[0].ClickCallback += showAllTasks;
            for (int i = 1; i < members.Length + 1; i++)
            {
                leftMenuItem[i] = new MenuItem
                {
                    iconColor = members[i - 1].avtar,
                    label = members[i - 1].firstname,
                    referenceId = members[i - 1].id,
                    isActive = false
                };
                leftMenuItem[i].ClickCallback += onItemClick;
            }

            #endregion

            #region Tasks

            allTasks = await GetAllTasks();

            showAllTasks(null, leftMenuItem[0]);

            #endregion

            isLoaded = true;
        }

        protected async Task<TaskModel[]> GetAllTasks()
        {
            var result = await TaskDataService.GetAllTasks();

            if (result != null && result.Payload != null && result.Payload.Any())
            {
                foreach (var item in result.Payload)
                {
                    tasks.Add(new TaskModel()
                    {
                        id = item.Id,
                        text = item.Subject,
                        isDone = item.IsComplete,
                        member = Mapper.Map<FamilyMember>(item.Members)
                    });
                }
            }
            return tasks.Cast<TaskModel>().ToArray();
        }
        protected void onAddItem()
        {
            showLister = false;
            showCreator = true;
            makeMenuItemActive(null);
            StateHasChanged();
        }
        protected async Task addTask()
        {
            if (!string.IsNullOrEmpty(taskstring))
            {
                Guid memberid = Guid.Empty;
                for (int i = 0; i < leftMenuItem.Length; i++)
                {
                    if (leftMenuItem[i].isActive)
                    {
                        memberid = leftMenuItem[i].referenceId;
                    }
                }
                var result = await TaskDataService.Create(new Domain.Commands.CreateTaskCommand()
                {
                    IsComplete = false,
                    AssignedToId = memberid,
                    Subject = taskstring
                });

                if (result != null && result.Payload != null && result.Payload.Id != Guid.Empty)
                {
                    taskstring = string.Empty;
                    tasks.Add(new TaskModel()
                    {
                        id = result.Payload.Id,
                        text = result.Payload.Subject,
                        isDone = result.Payload.IsComplete,
                        member = Mapper.Map<FamilyMember>(result.Payload.Members)
                    });
                    allTasks = tasks.Cast<TaskModel>().ToArray();
                    showAllTasks(null, leftMenuItem[0]);
                }
            }
        }
        protected void onItemClick(object sender, object e)
        {
            Guid val = (Guid)e.GetType().GetProperty("referenceId").GetValue(e);
            makeMenuItemActive(e);
            if (allTasks != null && allTasks.Length > 0)
            {
                tasksToShow = allTasks.Where(item =>
                {
                    if (item.member != null)
                    {
                        return item.member.id == val;
                    }
                    else
                    {
                        return false;
                    }
                }).ToArray();
            }
            showLister = true;
            showCreator = false;
            StateHasChanged();
        }
        protected void showAllTasks(object sender, object e)
        {
            tasksToShow = allTasks;
            showLister = true;
            showCreator = false;
            makeMenuItemActive(e);
            StateHasChanged();
        }
        public void onDrag(TaskModel e)
        {
            dragTaskModel = e;
        }
        public async Task onDrop(MenuItem menuItem)
        {
            if ((menuItem.referenceId != null || menuItem.referenceId != Guid.Empty) && !dragTaskModel.isDone)
            {
                var result = await TaskDataService.Update(new Domain.Commands.UpdateTaskCommand()
                {
                    Id = dragTaskModel.id,
                    Subject = dragTaskModel.text,
                    IsComplete = dragTaskModel.isDone,
                    AssignedToId = menuItem.referenceId
                });
                if (result.Succeed)
                {
                    foreach (var taskList in tasksToShow)
                    {
                        if (taskList.id == dragTaskModel.id)
                        {
                            taskList.assignedToId = menuItem.referenceId;
                            taskList.member.avtar = menuItem.iconColor;
                            taskList.member.id = menuItem.referenceId;
                            break;
                        }
                    }
                }
            }
        }
        public async Task onCheckChange(TaskModel taskModel)
        {
            var result = await TaskDataService.Update(new Domain.Commands.UpdateTaskCommand()
            {
                Id = taskModel.id,
                Subject = taskModel.text,
                IsComplete = !taskModel.isDone,
                AssignedToId = taskModel.member.id
            });
            if (result.Succeed)
            {
                foreach (var taskList in tasksToShow)
                {
                    if (taskList.id == taskModel.id)
                    {
                        taskList.isDone = !taskModel.isDone;
                        break;
                    }
                }
            }
        }

        protected void makeMenuItemActive(object e)
        {
            foreach (var item in leftMenuItem)
            {
                item.isActive = false;
            }
            if (e != null)
            {
                e.GetType().GetProperty("isActive").SetValue(e, true);
            }
        }

        protected void onMemberAdd()
        {
            Console.WriteLine("on member add");
        }
    }
}
