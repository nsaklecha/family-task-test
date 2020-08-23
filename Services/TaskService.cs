using AutoMapper;
using Core.Abstractions.Repositories;
using Core.Abstractions.Services;
using Domain.Commands;
using Domain.DataModels;
using Domain.Queries;
using Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _TaskRepository;
        private readonly IMapper _mapper;

        public TaskService(IMapper mapper, ITaskRepository taskRepository)
        {
            _mapper = mapper;
            _TaskRepository = taskRepository;
        }

        public async Task<CreateTaskCommandResult> CreateTaskCommandHandler(CreateTaskCommand command)
        {
            var manageTasks = _mapper.Map<Domain.DataModels.Task>(command);
            var persistedTask = await _TaskRepository.CreateRecordAsync(manageTasks);

            var vm = _mapper.Map<TaskVm>(persistedTask);

            return new CreateTaskCommandResult()
            {
                Payload = vm
            };
        }

        public async Task<GetAllTasksQueryResult> GetAllTaskQueryHandler()
        {
            IEnumerable<TaskVm> vm = new List<TaskVm>();

            var managetask = await _TaskRepository.Reset().ToListAsync();

            if (managetask != null && managetask.Any())
                vm = _mapper.Map<IEnumerable<TaskVm>>(managetask);

            return new GetAllTasksQueryResult()
            {
                Payload = vm
            };
        }

        public async Task<UpdateTaskCommandResult> UpdateTaskCommandHandler(UpdateTaskCommand command)
        {
            var isSucceed = true;
            var member = await _TaskRepository.ByIdAsync(command.Id);

            _mapper.Map<UpdateTaskCommand, Domain.DataModels.Task>(command, member);

            var affectedRecordsCount = await _TaskRepository.UpdateRecordAsync(member);

            if (affectedRecordsCount < 1)
                isSucceed = false;

            return new UpdateTaskCommandResult()
            {
                Succeed = isSucceed
            };
        }
    }
}
