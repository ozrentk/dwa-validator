namespace DwaValidatorApp.Validation
{
    public class ValidationMessage
    {
        public bool IsError { get; set; }
        public string Message { get; set; }
        public override string ToString()
        {
            return Message;
        }
    }

    public class ValidationResult
    {
        public List<ValidationMessage> Messages { get; set; } = new();
        
        public bool IsError { get; set; }

        public void AddError(string msg)
        {
            Messages.Add(new ValidationMessage { Message = msg, IsError = true });
            IsError = true;
        }

        public void AddErrors(IEnumerable<string> msgs)
        {
            Messages.AddRange(
                msgs.Select(x => 
                    new ValidationMessage { Message = x, IsError = true }));
            IsError = true;
        }

        public void AddInfo(string msg) 
        {
            Messages.Add(new ValidationMessage { Message = msg });
        } 

        public void AddInfos(IEnumerable<string> msgs)
        {
            Messages.AddRange(
                msgs.Select(x =>
                    new ValidationMessage { Message = x }));
        }
    }
}
