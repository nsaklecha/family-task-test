using AutoMapper;
using Domain.Commands;
using Domain.DataModels;
using Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.AutoMapper
{
    public class TaskProfile : Profile
    {
        public TaskProfile()
        {
            CreateMap<CreateTaskCommand, Domain.DataModels.Task>();
            CreateMap<UpdateTaskCommand, Domain.DataModels.Task>();
            CreateMap<Domain.DataModels.Task, TaskVm>();
        }
    }
}
