using System;
using Gosu.MsTestRunner.UI.ViewModels;

namespace Gosu.MsTestRunner.UI.Infrastructure
{
    public static class EventAggregator
    {
        public static event Action<TestViewModel> TestViewModelSelected = x => {};

        public static void PublishTestViewModelSelected(TestViewModel viewModel) => TestViewModelSelected(viewModel);
    }
}