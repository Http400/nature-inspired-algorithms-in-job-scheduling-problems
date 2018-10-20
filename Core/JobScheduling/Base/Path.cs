namespace Core.JobScheduling.Base
{
    public class Path
    {
        public (int jobId, int? machineId) StartingOperation { get; set; }
        public (int jobId, int? machineId) EndingOperation { get; set; }
        public float Pheromone { get; set; }
    }
}