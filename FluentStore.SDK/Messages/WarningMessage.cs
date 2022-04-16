namespace FluentStore.SDK.Messages
{
    public class WarningMessage
    {
        public WarningMessage(string message, object context = null)
        {
            Message = message;
            Context = context;
        }

        public string Message { get; set; }
        public object Context { get; set; }
    }
}
