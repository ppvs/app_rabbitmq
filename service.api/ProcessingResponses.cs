namespace service.api
{
    public enum ProcessErrorCode
    {
        Ok = 0,

        DeadLetter = 1,

        UnableToSaveTheResult = 5,
        UnknownCommand = 6,

        UnknownError = int.MaxValue
    }
    public class ProcessingResponse
    {
        public IProcessingCommand Command { get; set; }
        public ProcessErrorCode ErrorCode { get; set; }
    }

}
