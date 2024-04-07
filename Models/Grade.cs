using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Grade
    {
        public string TaskName { get; }

        public double Percentage { get; }

        public string Score { get; set; }


        // Constructor
        public Grade(string taskName, double percentage, string score)
        {
            TaskName = taskName;
            Percentage = percentage;
            Score = score;
        }

        public override string ToString()
        {
            return TaskName;
        }
    }
}
