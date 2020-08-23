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
//using AutoMapper;

namespace WebClient.Pages
{
    public class ManageTasksBase : ComponentBase
    {
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

        //protected readonly IMapper _mapper;
        //public ManageTasksBase(IMapper mapper)
        //{
        //    _mapper = mapper;
        //}
        [Inject]
        public ITaskDataService TaskDataService { get; set; }
        [Inject]
        public IMemberDataService MemberDataService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            //members = await Http.GetFromJsonAsync<FamilyMember[]>("sample-data/family-members.json");

            var memberResult = await MemberDataService.GetAllMembers();

            if (memberResult != null && memberResult.Payload != null && memberResult.Payload.Any())
            {
                foreach (var item in memberResult.Payload)
                {
                    if (item.Id != Guid.Empty)
                    {
                        familyMembers.Add(new FamilyMember()
                        {
                            avtar = item.Avatar,
                            email = item.Email,
                            firstname = item.FirstName,
                            lastname = item.LastName,
                            role = item.Roles,
                            id = item.Id
                        });
                    }
                }
            }

            var result = await TaskDataService.GetAllTasks();

            if (result != null && result.Payload != null && result.Payload.Any())
            {
                foreach (var item in result.Payload)
                {
                    FamilyMember familyMember = new FamilyMember();
                    TaskModel taskModel = new TaskModel();

                    taskModel.id = item.Id;
                    taskModel.text = item.Subject;
                    taskModel.isDone = item.IsComplete;
                    if (item.Members != null || item.AssignedToId.Value != Guid.Empty)
                    {
                        familyMember.id = item.AssignedToId.Value;
                        familyMember.firstname = item.Members.FirstName;
                        familyMember.lastname = item.Members.LastName;
                        familyMember.avtar = item.Members.Avatar;
                        familyMember.role = item.Members.Roles;
                        familyMember.email = item.Members.Email;
                        taskModel.member = familyMember;
                    }
                    tasks.Add(taskModel);

                    //tasks.Add(new TaskModel()
                    //{
                    //    id = item.Id,
                    //    text = item.Subject,
                    //    isDone = item.IsComplete,
                    //    //member = _mapper.Map<FamilyMember>(item.Members)
                    //});

                }
                Console.Write("Task List : {0}", tasks);
            }

            allTasks = tasks.Cast<TaskModel>().ToArray();
            Console.Write("allTasks List : {0}", allTasks);

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
            showAllTasks(null, leftMenuItem[0]);
            isLoaded = true;
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
                    tasks.Add(new TaskModel()
                    {
                        id = result.Payload.Id,
                        text = result.Payload.Subject,
                        isDone = result.Payload.IsComplete
                    });
                    showCreator = false;
                    StateHasChanged();
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

        //protected override async Task OnInitializedAsync()
        //{
        //    var result = await TaskDataService.GetAllTasks();

        //    if (result != null && result.Payload != null && result.Payload.Any())
        //    {                
        //        foreach (var item in result.Payload)
        //        {
        //            tasks.Add(new TaskModel()
        //            {
        //               id = item.Id,
        //               text = item.Subject,
        //               isDone = item.IsComplete
        //            });
        //        }
        //    }

        //    //for (int i = 0; i < members.Count; i++)
        //    //{
        //    //    leftMenuItem.Add(new MenuItem
        //    //    {
        //    //        iconColor = members[i].avtar,
        //    //        label = members[i].firstname,
        //    //        referenceId = members[i].id
        //    //    });
        //    //}
        //    showCreator = true;
        //    isLoaded = true;
        //}



        //protected void onAddItem()
        //{
        //    showCreator = true;
        //    StateHasChanged();
        //}

        //protected async Task onMemberAdd(FamilyMember familyMember)
        //{
        //   var result = await  MemberDataService.Create(new Domain.Commands.CreateMemberCommand()
        //    {
        //        Avatar = familyMember.avtar,
        //        FirstName = familyMember.firstname,
        //        LastName = familyMember.lastname,
        //        Email = familyMember.email,
        //        Roles = familyMember.role
        //    });

        //    if (result != null && result.Payload != null && result.Payload.Id != Guid.Empty)
        //    {
        //        members.Add(new FamilyMember()
        //        {
        //            avtar = result.Payload.Avatar,
        //            email = result.Payload.Email,
        //            firstname = result.Payload.FirstName,
        //            lastname = result.Payload.LastName,
        //            role = result.Payload.Roles,
        //            id = result.Payload.Id
        //        });

        //        leftMenuItem.Add(new MenuItem
        //        {
        //            iconColor = result.Payload.Avatar,
        //            label = result.Payload.FirstName,
        //            referenceId = result.Payload.Id
        //        });


        //        showCreator = false;
        //        StateHasChanged();
        //    }
        //}

    }
}
