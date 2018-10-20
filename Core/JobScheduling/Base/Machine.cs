using System;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;

namespace Core.JobScheduling.Base
{
    public class Machine
    {
        public int Id { get; set; }
        public List<(Operation Operation, int StartTime)> Operations { get; set; }
        public List<TimeGap> TimeGaps { get; set; }


        public Machine() {}

        public Machine(int id)
        {
            this.Id = id;
            this.Operations = new List<(Operation Operation, int StartTime)>();
            this.TimeGaps = new List<TimeGap>();
        }

        public Machine Clone()
        {
            return new Machine() {
                Id = this.Id,
                Operations = this.Operations.Select(o => ((Operation)o.Operation.Clone(), o.StartTime)).ToList(),
                TimeGaps = this.TimeGaps.Select(tg => new TimeGap(tg.StartTime, tg.Duration)).ToList()
            };
        }

        public void AddOperation(Operation operation, int operationStartTime)
        {
            var timeGapToFill = this.TimeGaps.Where(tg => tg.StartTime <= operationStartTime).LastOrDefault();

            if (timeGapToFill == null || operationStartTime > timeGapToFill.EndTime)
            {
                var lastOperation = this.Operations.LastOrDefault();

                if (lastOperation.Operation != null)
                {
                    var lastOperationEndTime = lastOperation.StartTime + lastOperation.Operation.ProcessingTime;

                    if (lastOperationEndTime < operationStartTime)
                    {
                        this.TimeGaps.Add(new TimeGap(startTime: lastOperationEndTime, duration: operationStartTime - lastOperationEndTime));
                    }
                }
                else if (operationStartTime > 0)
                {
                    this.TimeGaps.Add(new TimeGap(startTime: 0, duration: operationStartTime));
                }
            }
            else
            {
                if (timeGapToFill.EndTime < operationStartTime + operation.ProcessingTime)
                {
                    throw new Exception("Time gap is too short for the operation.");
                }

                var newTimeGaps = new List<TimeGap>();

                if (timeGapToFill.StartTime < operationStartTime)
                {
                    newTimeGaps.Add( new TimeGap(timeGapToFill.StartTime, operationStartTime - timeGapToFill.StartTime) );
                }

                if (timeGapToFill.EndTime > operationStartTime + operation.ProcessingTime)
                {
                    newTimeGaps.Add( new TimeGap(operationStartTime + operation.ProcessingTime, timeGapToFill.EndTime - (operationStartTime + operation.ProcessingTime)) );
                }

                if (newTimeGaps.Count > 0)
                {
                    this.TimeGaps.Replace(timeGapToFill, newTimeGaps);
                }

                if (timeGapToFill.StartTime == operationStartTime && timeGapToFill.EndTime == operationStartTime + operation.ProcessingTime)
                {
                    this.TimeGaps.Remove(timeGapToFill);
                }
            }
            
            var laterOperation = this.Operations.Where(o => o.StartTime > operationStartTime).FirstOrDefault();

            if (laterOperation.Operation == null)
            {
                this.Operations.Add( (operation, operationStartTime) );
            }
            else
            {
                var index = this.Operations.IndexOf(laterOperation);
                this.Operations.Insert( index, (operation, operationStartTime) );
            }
        }

        // public int GetOperationEndTime(int jobId)
        // {
        //     var operation = this.Operations.Find(item => item.Operation.JobId == jobId);
        //     var endTime = operation.StartTime + operation.Operation.ProcessingTime;
        //     return endTime;
        // }

        public int GetLastOperationEndTime()
        {
            var operation = this.Operations.LastOrDefault();

            if (operation.Operation == null)
                return 0;

            var endTime = operation.StartTime + operation.Operation.ProcessingTime;
            return endTime;
        }

        public int GetOperationEndTime(int jobId)
        {
            var operation = this.Operations.Find(o => o.Operation.JobId == jobId);
            return operation.StartTime + operation.Operation.ProcessingTime;
        }

        public class TimeGap
        {
            public int StartTime { get; set; }
            public int Duration { get; set; }
            public int EndTime => this.StartTime + this.Duration;

            public TimeGap(int startTime, int duration)
            {
                this.StartTime = startTime;
                this.Duration = duration;
            }
        }
    }
}