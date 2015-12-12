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
        private TestSessionContext _testSessionContext;
        private TestCaseRunner _testRunner;

        public TestListViewModel()
        {
            _testSessionContextLoader = new TestSessionContextLoader(this);
            _settingsService = new UserSettingsService();
            _testRunner = new TestCaseRunner();

            InitializeTestListCommand = new AsyncDelegateCommand(InitializeTestList);
            ExecuteAllTestsCommand = new AsyncDelegateCommand(ExecuteAllTests);
            TestGroups = new ObservableCollection<TestGroupViewModel>();
            
            ConfigFilePath = _settingsService.LastConfigFilePath;

            if (!string.IsNullOrWhiteSpace(ConfigFilePath))
            {
                InitializeTestList();
            }
        }

        public string Log
        {
            get { return Get(() => Log); }
            set { Set(() => Log, value); }
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

        public async Task InitializeTestList()
        {
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
            }
        }

        private async Task ExecuteAllTests()
        {
            var testViewModels = TestGroups.SelectMany(x => x.Tests).ToList();
            var testCasesToRun = testViewModels.Select(x => x.TestCase).ToList();

            var testViewModelsByTestCaseId = testViewModels.ToDictionary(x => x.TestCase.Id);

            _testRunner.TestCaseFinished += (testCase, testResult) => testViewModelsByTestCaseId[testCase.Id].OnTestCaseFinished(testResult);
            _testRunner.TestCaseStarting += testCase => testViewModelsByTestCaseId[testCase.Id].OnTestCaseStarting();

            await _testRunner.Run(testCasesToRun);
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