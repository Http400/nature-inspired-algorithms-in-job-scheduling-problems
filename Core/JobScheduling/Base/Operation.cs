using System;

namespace Core.JobScheduling.Base
{
    public class Operation : ICloneable
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int MachineId { get; set; }
        public int ProcessingTime { get; set; }

        public Operation() {}

        public Operation(int Id, int jobId, int machineId, int processingTime)
        {
            this.Id = Id;
            this.JobId = jobId;
            this.MachineId = machineId;
            this.ProcessingTime = processingTime;
        }

        public object Clone()
        {
            return new Operation(this.Id, this.JobId, this.MachineId, this.ProcessingTime);
        }

        public static bool operator ==(Operation operation1, Operation operation2)
        {

            if (Object.ReferenceEquals(operation1, null) || Object.ReferenceEquals(operation2, null))
            {
                return Object.ReferenceEquals(operation1, operation2);
            } 
            else if (operation1.JobId == operation2.JobId && operation1.MachineId == operation2.MachineId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator !=(Operation operation1, Operation operation2)
        {
            if (Object.ReferenceEquals(operation1, null) || Object.ReferenceEquals(operation2, null))
            {
                return !Object.ReferenceEquals(operation1, operation2);
            } 
            if (operation1.JobId != operation2.JobId || operation1.MachineId != operation2.MachineId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(Object obj) 
        {
            return obj is Operation && this == (Operation)obj;
        }

        public override int GetHashCode() 
        {
            return JobId.GetHashCode() ^ MachineId.GetHashCode() ^ ProcessingTime.GetHashCode();
        }
    }
}