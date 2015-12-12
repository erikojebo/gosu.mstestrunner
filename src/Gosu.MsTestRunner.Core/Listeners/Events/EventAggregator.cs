using System;

namespace Gosu.MsTestRunner.Core.Listeners.Events
{
    public class EventAggregator : IEventAggregator
    {
        public event Action<string> ConfigFileLoadFailed = x => {};

        public void PublishConfigFileLoadFailed(string errorDescription) => ConfigFileLoadFailed(errorDescription);
    }
}