using Gosu.MsTestRunner.Core.Runner;
using Gosu.MsTestRunner.UI.Mvvm;

namespace Gosu.MsTestRunner.UI.ViewModels
{
    internal class TestViewModel : ViewModelBase
    {
        public TestViewModel(TestCase testCase)
        {
            TestCase = testCase;
            Name = testCase.DisplayName;
            Description = "";
        }

        public TestCase TestCase { get; }

        public bool? WasSuccessful
        {
            get { return Get(() => WasSuccessful); }
            set
            {
                Set(() => WasSuccessful, value);

                if (value != null)
                {
                    WasFailure = !value.Value;
                }
            }
        }

        public bool? WasFailure
        {
            get { return Get(() => WasFailure); }
            private set { Set(() => WasFailure, value); }
        }

        public bool? WasIgnored
        {
            get { return Get(() => WasIgnored); }
            set { Set(() => WasIgnored, value); }
        }

        public bool? IsTestExecuting
        {
            get { return Get(() => IsTestExecuting); }
            set { Set(() => IsTestExecuting, value); }
        }

        public bool IsSelected
        {
            get { return Get(() => IsSelected); }
            set { Set(() => IsSelected, value); }
        }

        public string Name { get; }
        public string Description { get; }
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
        public bool HasExecuted => WasSuccessful != null;

        public void OnTestCaseFinished(TestResult testResult)
        {
            InvokeUIAction(() =>
            {
                WasIgnored = testResult.WasIgnored;
                WasSuccessful = testResult.WasSuccessful;
                IsTestExecuting = false;
            });
        }

        public void OnTestCaseStarting()
        {
            InvokeUIAction(() =>
            {
                WasSuccessful = null;
                WasIgnored = false;
                IsTestExecuting = true;
            });
        }

        public static string IsTestExecutingPropertyName => nameof(IsTestExecuting);

        public void ResetResult()
        {
            WasSuccessful = null;
        }
    }
}