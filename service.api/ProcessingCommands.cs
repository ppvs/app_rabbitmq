namespace service.api
{
    public interface IProcessingCommand
    {
        long ProjectId { get; }
    }

    public class Command : IProcessingCommand
    {
        public long ProjectId { get; set; }
        public string FilesPath { get; set; }
    }

}
