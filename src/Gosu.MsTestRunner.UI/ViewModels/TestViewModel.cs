using System.Collections.Generic;
using System.Linq;
using Gosu.MsTestRunner.Core.Extensions;
using Gosu.MsTestRunner.Core.Runner;
using Gosu.MsTestRunner.UI.Infrastructure;
using Gosu.MsTestRunner.UI.Mvvm;

namespace Gosu.MsTestRunner.UI.ViewModels
{
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel(TestCase testCase)
        {
            TestCase = testCase;
            Name = testCase.Name.PrettifyIdentifier();
            ClassName = testCase.TestClassName.PrettifyIdentifier();
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

        public bool? WasPartialFailure
        {
            get { return Get(() => WasPartialFailure); }
            private set { Set(() => WasPartialFailure, value); }
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

        public string TestResultMessage
        {
            get { return Get(() => TestResultMessage); }
            set { Set(() => TestResultMessage, value); }
        }

        public bool IsSelected
        {
            get { return Get(() => IsSelected); }
            set
            {
                bool wasSelected = value && !IsSelected;

                Set(() => IsSelected, value);

                if (wasSelected)
                    EventAggregator.PublishTestViewModelSelected(this);
            }
        }

        public string Name { get; }
        public string ClassName { get; }
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
                TestResultMessage = testResult.CombinedMessage;
                WasPartialFailure = !testResult.WasIgnored && (testResult.WasInitializeSuccessful == false || testResult.WasCleanUpSuccessful == false);
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

        public bool MatchesSearchString(string searchString)
        {
            return string.IsNullOrWhiteSpace(searchString) || Name.Contains(searchString) || ClassName.Contains(searchString);
        }

        public bool MatchesCategories(IEnumerable<string> selectedTestCategoryNames)
        {
            var testCategoryNames = selectedTestCategoryNames.ToList();

            return !testCategoryNames.Any() || testCategoryNames.Intersect(TestCase.Categories).Any();
        }
    }
}