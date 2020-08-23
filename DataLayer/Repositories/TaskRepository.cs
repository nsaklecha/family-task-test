using Core.Abstractions.Repositories;
using Domain.DataModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataLayer.Repositories
{
    public class TaskRepository : BaseRepository<Guid, Task, TaskRepository>, ITaskRepository
    {
        protected readonly FamilyTaskContext TaskContext;
        protected IQueryable<Task> TaskQuery;
        public TaskRepository(FamilyTaskContext context) : base(context)
        {
            TaskContext = context;
            TaskQuery = context.Set<Task>();
        }



        ITaskRepository IBaseRepository<Guid, Task, ITaskRepository>.NoTrack()
        {
            return base.NoTrack();
        }

        ITaskRepository IBaseRepository<Guid, Task, ITaskRepository>.Reset()
        {
            TaskQuery = TaskContext.Tasks.Include("Members").AsQueryable();
            return this as ITaskRepository;
        }

    }
}
