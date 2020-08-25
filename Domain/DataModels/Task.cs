using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DataModels
{
    public class Task
    {
        public Task() { }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Subject { get; set; }
        public Boolean IsComplete { get; set; }
        public Guid? AssignedToId { get; set; }
        public virtual Member Members { get; set; }
    }
}
