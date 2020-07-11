namespace DominosLocationMap.Core.CrossCutting.Logging
{
    public interface ILogManager
    {
        void Information(string message);

        void Warning(string message);

        void Debug(string message);

        void Error(string message);
    }
}