using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Gosu.MsTestRunner.Core.Listeners;
using Gosu.MsTestRunner.Core.Loading;
using Gosu.MsTestRunner.Core.Runner;
using Gosu.MsTestRunner.UI.Extensions;
using Gosu.MsTestRunner.UI.Infrastructure;
using Gosu.MsTestRunner.UI.Mvvm;

namespace Gosu.MsTestRunner.UI.ViewModels
{
    internal class TestListViewModel : ViewModelBase, ILogger
    {
        private readonly TestSessionContextLoader _testSessionContextLoader;
        private readonly UserSettingsService _settingsService;
        private readonly TestCaseRunner _testRunner;
        private TestSessionContext _testSessionContext;
        private Dictionary<Guid, TestViewModel> _executingTestViewModelsByTestCaseId;

        public TestListViewModel()
        {
            _testSessionContextLoader = new TestSessionContextLoader(this);
            _settingsService = new UserSettingsService();
            _testRunner = new TestCaseRunner();

            InitializeTestListCommand = new AsyncDelegateCommand(InitializeTestList);
            ExecuteAllTestsCommand = new AsyncDelegateCommand(ExecuteAllTests);
            ExecuteSelectedTestsCommand = new AsyncDelegateCommand(ExecuteSelectedTests);
            ExecuteSelectedTestsInParallelCommand = new AsyncDelegateCommand(ExecuteSelectedTestsInParallel);
            TestGroups = new ObservableCollection<TestGroupViewModel>();
            TestCategories = new ObservableCollection<TestCategoryViewModel>();
            
            ConfigFilePath = _settingsService.LastConfigFilePath;

            _testSessionContextLoader.ProgressChanged += OnTestSessionContextLoaderProgressChanged;

            _testRunner.TestCaseFinished += OnTestCaseFinished;
            _testRunner.TestCaseStarting += OnTestCaseStarting;
            IsProgressIndeterminate = true;

            EventAggregator.TestViewModelSelected += OnTestViewModelSelected;
        }

        private void OnTestViewModelSelected(TestViewModel testViewModel)
        {
            LogOutput = testViewModel.TestResultMessage;
        }

        private void OnTestSessionContextLoaderProgressChanged(int loadedAssemblyCount, int totalAssemblyCount)
        {
            Status = loadedAssemblyCount == totalAssemblyCount ? "Ready" : "Loading tests...";

            ProgressMax = totalAssemblyCount;
            ProgressValue = loadedAssemblyCount;
        }

        public string SearchString
        {
            get { return Get(() => SearchString); }
            set
            {
                var hasChanged = value != SearchString;

                Set(() => SearchString, value);

                if (hasChanged)
                    EventAggregator.PublishSearchStringChanged(SearchString);
            }
        }

        public string LogOutput
        {
            get { return Get(() => LogOutput); }
            set { Set(() => LogOutput, value); }
        }

        public string Status
        {
            get { return Get(() => Status); }
            set { Set(() => Status, value); }
        }

        public int ProgressMax
        {
            get { return Get(() => ProgressMax); }
            set { Set(() => ProgressMax, value); }
        }

        public int ProgressValue
        {
            get { return Get(() => ProgressValue); }
            set { Set(() => ProgressValue, value); }
        }

        public bool IsProgressIndeterminate
        {
            get { return Get(() => IsProgressIndeterminate); }
            set { Set(() => IsProgressIndeterminate, value); }
        }

        public ObservableCollection<TestCategoryViewModel> TestCategories { get; }

        public IEnumerable<string> SelectedTestCategoryNames => TestCategories.Where(x => x.IsSelected).Select(x => x.Name);

        public bool HasTestCategories => TestCategories.Any();

        public string ConfigFilePath
        {
            get { return Get(() => ConfigFilePath); }
            set
            {
                Set(() => ConfigFilePath, value);

                TestAssemblyFileName = Path.GetFileName(value);

                _settingsService.LastConfigFilePath = value;
            }
        }

        public string TestAssemblyFileName
        {
            get { return Get(() => TestAssemblyFileName); }
            set
            {
                Set(() => TestAssemblyFileName, value);
            }
        }

        public ObservableCollection<TestGroupViewModel> TestGroups { get; }
        public AsyncDelegateCommand InitializeTestListCommand { get; }
        public AsyncDelegateCommand ExecuteAllTestsCommand { get; }
        public AsyncDelegateCommand ExecuteSelectedTestsCommand { get; }
        public AsyncDelegateCommand ExecuteSelectedTestsInParallelCommand { get; }

        public async Task InitializeTestList()
        {
            if (string.IsNullOrWhiteSpace(ConfigFilePath))
            {
                return;
            }

            foreach (var testGroupViewModel in TestGroups)
            {
                testGroupViewModel.PropertyChanged -= OnGroupViewModelPropertyChanged;
            }

            TestGroups.Clear();
            
            if (_testSessionContext != null)
                await _testSessionContext.DisposeAsync();

            _testSessionContext = await _testSessionContextLoader.Load(ConfigFilePath);

            var testCaseGroup = _testSessionContext.TestCases.GroupBy(x => x.AssemblyName);

            foreach (var group in testCaseGroup)
            {
                var testCases = group.OrderBy(x => x.TestClassName).ThenBy(x => x.Name);
                var groupViewModel = new TestGroupViewModel(group.Key, testCases, this);

                TestGroups.Add(groupViewModel);

                groupViewModel.PropertyChanged += OnGroupViewModelPropertyChanged;
            }

            var testCategories = _testSessionContext.TestCases
                .SelectMany(x => x.Categories)
                .Distinct()
                .Select(x => new TestCategoryViewModel(x));

            TestCategories.ResetTo(testCategories);
            FirePropertyChanged(nameof(HasTestCategories));
        }

        private void OnGroupViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TestGroupViewModel.ExecutedTestCaseCountPropertyName && _executingTestViewModelsByTestCaseId != null)
                ProgressValue = _executingTestViewModelsByTestCaseId.Values.Count(x => x.HasExecuted);
        }

        public async Task ExecuteTestGroup(TestGroupViewModel testGroup)
        {
            await ExecuteTests(testGroup.VisibleTests);
        }

        public async Task ExecuteTestGroupInParallel(TestGroupViewModel testGroup)
        {
            await ExecuteTests(testGroup.VisibleTests, allowParallelism: true);
        }

        private async Task ExecuteAllTests()
        {
            var testViewModels = TestGroups.SelectMany(x => x.VisibleTests).ToList();

            await ExecuteTests(testViewModels);
        }

        private async Task ExecuteSelectedTests()
        {
            var testViewModels = TestGroups
                .SelectMany(x => x.VisibleTests)
                .Where(x => x.IsSelected)
                .ToList();

            await ExecuteTests(testViewModels);
        }

        private async Task ExecuteSelectedTestsInParallel()
        {
            var testViewModels = TestGroups
                .SelectMany(x => x.VisibleTests)
                .Where(x => x.IsSelected)
                .ToList();

            await ExecuteTests(testViewModels, allowParallelism: true);
        }

        private async Task ExecuteTests(IEnumerable<TestViewModel> testViewModels, bool allowParallelism = false)
        {
            var testViewModelList = testViewModels.ToList();

            foreach (var testViewModel in testViewModelList)
            {
                testViewModel.ResetResult();
            }

            _executingTestViewModelsByTestCaseId = testViewModelList.ToDictionary(x => x.TestCase.Id);

            ProgressMax = _executingTestViewModelsByTestCaseId.Count;

            var testCasesToRun = testViewModelList.Select(x => x.TestCase).ToList();

            IsProgressIndeterminate = true;

            await _testRunner.Run(testCasesToRun, allowParallelism);

            _executingTestViewModelsByTestCaseId = null;
        }

        private void OnTestCaseStarting(TestCase testCase)
        {
            _executingTestViewModelsByTestCaseId[testCase.Id].OnTestCaseStarting();
        }

        private void OnTestCaseFinished(TestCase testCase, TestResult testResult)
        {
            InvokeUIAction(() => { IsProgressIndeterminate = false; });

            _executingTestViewModelsByTestCaseId[testCase.Id].OnTestCaseFinished(testResult);
        }

        public void OutputLine(string message, params object[] formatParams)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogOutput += Environment.NewLine + string.Format(message, formatParams);
            });
        }

        public void WriteError(string message)
        {
            OutputLine(message);
        }

        public void WriteInfo(string message)
        {
            OutputLine(message);
        }
    }
}