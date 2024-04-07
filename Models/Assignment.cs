using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Assignment
    {
        public string TaskName {  get; set; }

        public Assignment(string taskName)
        {
            TaskName = taskName;
        }

        public override string ToString()
        {
            return TaskName;
        }
    }
}
