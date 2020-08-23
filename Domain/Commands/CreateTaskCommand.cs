using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Commands
{
    public class CreateTaskCommand
    {
        public string Subject { get; set; }
        public Boolean IsComplete { get; set; }
        public Guid? AssignedToId { get; set; }
    }
}
