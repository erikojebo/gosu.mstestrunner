using System;
using Gosu.MsTestRunner.UI.ViewModels;

namespace Gosu.MsTestRunner.UI.Infrastructure
{
    public static class EventAggregator
    {
        public static event Action<TestViewModel> TestViewModelSelected = x => {};
        public static event Action TestCategorySelectionChanged = () => {};
        public static event Action<string> SearchStringChanged = x => {};

        public static void PublishTestViewModelSelected(TestViewModel viewModel) => TestViewModelSelected(viewModel);
        public static void PublishTestCategorySelectionChanged() => TestCategorySelectionChanged();
        public static void PublishSearchStringChanged(string newSearchString) => SearchStringChanged(newSearchString);
    }
}