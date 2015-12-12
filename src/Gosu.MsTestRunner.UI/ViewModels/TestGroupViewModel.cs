using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Gosu.MsTestRunner.Core.Runner;
using Gosu.MsTestRunner.UI.Mvvm;

namespace Gosu.MsTestRunner.UI.ViewModels
{
    internal class TestGroupViewModel : ViewModelBase
    {
        public TestGroupViewModel(string name, IEnumerable<TestCase> testCases)
        {
            var testCaseList = testCases.ToList();

            Name = name;
            Tests = new ObservableCollection<TestViewModel>(testCaseList.Select(x => new TestViewModel(x)));
            ToggleExpandCollapseCommand = new DelegateCommand(ToggleExpandCollapse);

            foreach (var testViewModel in Tests)
            {
                testViewModel.PropertyChanged += OnTestViewModelPropertyChanged;
            }

            TotalTestCaseCount = testCaseList.Count;
        }

        private void OnTestViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            FirePropertyChanged(nameof(SuccessfulTestCaseCount));
            FirePropertyChanged(nameof(FailedTestCaseCount));
            FirePropertyChanged(nameof(IgnoredTestCaseCount));
            FirePropertyChanged(nameof(ExecutedTestCaseCount));
        }

        private void ToggleExpandCollapse()
        {
            IsExpanded = !IsExpanded;
        }

        public int SuccessfulTestCaseCount => Tests.Count(x => x.WasSuccessful == true);
        public int FailedTestCaseCount => Tests.Count(x => x.WasSuccessful == false);
        public int IgnoredTestCaseCount => Tests.Count(x => x.WasIgnored == true);
        public int ExecutedTestCaseCount => Tests.Count(x => x.WasSuccessful != null);

        public DelegateCommand ToggleExpandCollapseCommand { get; }

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
    }
}