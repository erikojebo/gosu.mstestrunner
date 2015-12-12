namespace Gosu.MsTestRunner.Core.Listeners
{
    public interface ILogger
    {
        void WriteError(string message);
        void WriteInfo(string message);
    }
}