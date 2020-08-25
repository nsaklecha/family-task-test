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
            return base.Reset();
        }

        async System.Threading.Tasks.Task<IEnumerable<Domain.DataModels.Task>> IBaseRepository<Guid, Task, ITaskRepository>.ToListAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            return await TaskQuery.Include("Members").ToListAsync(cancellationToken);
        }

        async System.Threading.Tasks.Task<Domain.DataModels.Task> IBaseRepository<Guid, Task, ITaskRepository>.CreateRecordAsync(Domain.DataModels.Task record, System.Threading.CancellationToken cancellationToken = default)
        {
            var result = await TaskContext.AddAsync(record, cancellationToken);
            await TaskContext.SaveChangesAsync(cancellationToken);
            var entity = result.Entity;
            var task = await TaskContext.Tasks.Include("Members").SingleOrDefaultAsync(x => x.Id == entity.Id);
            return task;
        }
    }
}
