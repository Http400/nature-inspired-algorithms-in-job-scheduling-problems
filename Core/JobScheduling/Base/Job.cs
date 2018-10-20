using System.Collections.Generic;

namespace Core.JobScheduling.Base
{
    public class Job
    {
        public int Id { get; set; }
        public List<Operation> Operations { get; set; }

        public Job()
        {
            Operations = new List<Operation>();
        }
    }
}