namespace DominosLocationMap.Core.CrossCutting.Logging
{
    public class LogParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
    }

    public class LogReturnValue
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
    }
}