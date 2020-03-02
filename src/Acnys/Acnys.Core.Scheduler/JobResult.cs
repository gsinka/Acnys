namespace Acnys.Core.Scheduler
{
    public class JobResult
    {
        public static JobResult Success => new JobResult();

        public static JobResult Failure(string error) => new JobResult(error);

        public bool Succeeded => !string.IsNullOrWhiteSpace(Error);

        public bool Failed => !string.IsNullOrWhiteSpace(Error);

        public string Error { get; }

        public JobResult()
        {
        }

        public JobResult(string error)
        {
            Error = error;
        }
    }
}