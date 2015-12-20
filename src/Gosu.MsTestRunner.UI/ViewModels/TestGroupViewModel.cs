using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Gosu.MsTestRunner.Core.Runner;
using Gosu.MsTestRunner.UI.Extensions;
using Gosu.MsTestRunner.UI.Infrastructure;
using Gosu.MsTestRunner.UI.Mvvm;

namespace Gosu.MsTestRunner.UI.ViewModels
{
    internal class TestGroupViewModel : ViewModelBase
    {
        private readonly TestListViewModel _testListViewModel;

        public TestGroupViewModel(string name, IEnumerable<TestCase> testCases, TestListViewModel testListViewModel)
        {
            _testListViewModel = testListViewModel;
            var testCaseList = testCases.ToList();

            Name = name;
            Tests = new ObservableCollection<TestViewModel>(testCaseList.Select(x => new TestViewModel(x)));
            VisibleTests = new ObservableCollection<TestViewModel>(Tests);
            ToggleExpandCollapseCommand = new DelegateCommand(ToggleExpandCollapse);
            ExecuteTestsCommand = new AsyncDelegateCommand(ExecuteTests);
            ExecuteTestsInParallelCommand = new AsyncDelegateCommand(ExecuteTestsInParallel);

            foreach (var testViewModel in Tests)
            {
                testViewModel.PropertyChanged += OnTestViewModelPropertyChanged;
            }

            TotalTestCaseCount = testCaseList.Count;

            EventAggregator.TestResultFilterSelectionChanged += UpdateVisibleTests;
            EventAggregator.TestCategorySelectionChanged += UpdateVisibleTests;
            EventAggregator.SearchStringChanged += x => UpdateVisibleTests();
        }

        private void UpdateVisibleTests()
        {
            var visibleTestViewModels = Tests.Where(t => 
                t.MatchesSearchString(_testListViewModel.SearchString) && 
                t.MatchesCategories(_testListViewModel.SelectedTestCategoryNames) && 
                t.Matches(_testListViewModel.SelectedTestFilterPredicates));

            VisibleTests.ResetTo(visibleTestViewModels);
        }

        private void OnTestViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TestViewModel.IsTestExecutingPropertyName)
            {
                FirePropertyChanged(nameof(SuccessfulTestCaseCount));
                FirePropertyChanged(nameof(FailedTestCaseCount));
                FirePropertyChanged(nameof(IgnoredTestCaseCount));
                FirePropertyChanged(nameof(ExecutedTestCaseCount));
            }
        }

        private void ToggleExpandCollapse()
        {
            IsExpanded = !IsExpanded;
        }

        public async Task ExecuteTests()
        {
            await _testListViewModel.ExecuteTestGroup(this);
        }

        public async Task ExecuteTestsInParallel()
        {
            await _testListViewModel.ExecuteTestGroupInParallel(this);
        }

        public int SuccessfulTestCaseCount => Tests.Count(x => x.WasSuccessful == true);
        public int FailedTestCaseCount => Tests.Count(x => x.WasSuccessful == false);
        public int IgnoredTestCaseCount => Tests.Count(x => x.WasIgnored == true);
        public int ExecutedTestCaseCount => Tests.Count(x => x.HasExecuted || x.WasIgnored == true);

        public static string SuccessfulTestCaseCountPropertyName => nameof(SuccessfulTestCaseCount);
        public static string FailedTestCaseCountPropertyName => nameof(FailedTestCaseCount);
        public static string IgnoredTestCaseCountPropertyName => nameof(IgnoredTestCaseCount);
        public static string ExecutedTestCaseCountPropertyName => nameof(ExecutedTestCaseCount);

        public DelegateCommand ToggleExpandCollapseCommand { get; }
        public AsyncDelegateCommand ExecuteTestsCommand { get; }
        public AsyncDelegateCommand ExecuteTestsInParallelCommand { get; }

        public bool IsExpanded
        {
            get { return Get(() => IsExpanded); }
            set { Set(() => IsExpanded, value); }
        }

        public int TotalTestCaseCount
        {
            get { return Get(() => TotalTestCaseCount); }
            set { Set(() => TotalTestCaseCount, value); }
        }

        public string Name { get; }
        public bool HasName => !string.IsNullOrWhiteSpace(Name);
        public ObservableCollection<TestViewModel> Tests { get; }
        public ObservableCollection<TestViewModel> VisibleTests { get; }

        public void RefreshVisibleTestCases(string visibleCategories, string searchTerm)
        {
            throw new System.NotImplementedException();
        }
    }
}