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
            TestGroups = new ObservableCollection<TestGroupViewModel>();
            
            ConfigFilePath = _settingsService.LastConfigFilePath;

            _testSessionContextLoader.ProgressChanged += OnTestSessionContextLoaderProgressChanged;

            _testRunner.TestCaseFinished += OnTestCaseFinished;
            _testRunner.TestCaseStarting += OnTestCaseStarting;
        }

        private void OnTestSessionContextLoaderProgressChanged(int loadedAssemblyCount, int totalAssemblyCount)
        {
            Status = loadedAssemblyCount == totalAssemblyCount ? "Ready" : "Loading tests...";

            ProgressMax = totalAssemblyCount;
            ProgressValue = loadedAssemblyCount;
        }

        public string Log
        {
            get { return Get(() => Log); }
            set { Set(() => Log, value); }
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

        public async Task InitializeTestList()
        {
            if (string.IsNullOrWhiteSpace(ConfigFilePath))
            {
                return;
            }

            foreach (var testGroupViewModel in TestGroups)
            {
                
            }

            TestGroups.Clear();
            
            if (_testSessionContext != null)
                await _testSessionContext.DisposeAsync();

            _testSessionContext = await _testSessionContextLoader.Load(ConfigFilePath);

            var testCaseGroup = _testSessionContext.TestCases.GroupBy(x => x.AssemblyName);

            foreach (var group in testCaseGroup)
            {
                var testCases = group.OrderBy(x => x.TestClassName).ThenBy(x => x.DisplayName);
                var groupViewModel = new TestGroupViewModel(group.Key, testCases);

                TestGroups.Add(groupViewModel);

                groupViewModel.PropertyChanged += OnGroupViewModelPropertyChanged;
            }
        }

        private void OnGroupViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TestGroupViewModel.ExecutedTestCaseCountPropertyName && _executingTestViewModelsByTestCaseId != null)
                ProgressValue = _executingTestViewModelsByTestCaseId.Values.Count(x => x.HasExecuted);
        }

        private async Task ExecuteAllTests()
        {
            var testViewModels = TestGroups.SelectMany(x => x.Tests).ToList();

            await ExecuteTests(testViewModels);
        }

        private async Task ExecuteSelectedTests()
        {
            var testViewModels = TestGroups
                .SelectMany(x => x.Tests)
                .Where(x => x.IsSelected)
                .ToList();

            await ExecuteTests(testViewModels);
        }

        private async Task ExecuteTests(List<TestViewModel> testViewModels)
        {
            _executingTestViewModelsByTestCaseId = testViewModels.ToDictionary(x => x.TestCase.Id);

            ProgressMax = _executingTestViewModelsByTestCaseId.Count;

            var testCasesToRun = testViewModels.Select(x => x.TestCase).ToList();

            await _testRunner.Run(testCasesToRun);

            _executingTestViewModelsByTestCaseId = null;
        }

        private void OnTestCaseStarting(TestCase testCase)
        {
            _executingTestViewModelsByTestCaseId[testCase.Id].OnTestCaseStarting();
        }

        private void OnTestCaseFinished(TestCase testCase, TestResult testResult)
        {
            _executingTestViewModelsByTestCaseId[testCase.Id].OnTestCaseFinished(testResult);
        }

        public void OutputLine(string message, params object[] formatParams)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Log += Environment.NewLine + string.Format(message, formatParams);
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