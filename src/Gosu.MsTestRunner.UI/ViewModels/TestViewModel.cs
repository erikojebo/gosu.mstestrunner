﻿using Gosu.MsTestRunner.Core.Runner;
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
            set { Set(() => WasSuccessful, value); }
        }

        public bool? IsTestExecuting
        {
            get { return Get(() => IsTestExecuting); }
            set { Set(() => IsTestExecuting, value); }
        }

        public string Name { get; }
        public string Description { get; }
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        public bool? WasIgnored
        {
            get { return Get(() => WasIgnored); }
            set { Set(() => WasIgnored, value); }
        }

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
    }
}