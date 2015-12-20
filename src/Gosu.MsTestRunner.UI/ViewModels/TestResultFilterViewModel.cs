using System;
using Gosu.MsTestRunner.UI.Infrastructure;
using Gosu.MsTestRunner.UI.Mvvm;

namespace Gosu.MsTestRunner.UI.ViewModels
{
    public class TestResultFilterViewModel : ViewModelBase
    {
        public TestResultFilterViewModel(string name, Predicate<TestViewModel> filterPredicate)
        {
            Predicate = filterPredicate;
            Name = name;
            ToggleSelectionCommand = new DelegateCommand(ToggleSelection);
        }

        public DelegateCommand ToggleSelectionCommand { get; }

        public string Name { get; set; }
        public Predicate<TestViewModel> Predicate { get; set; }

        public bool IsSelected
        {
            get { return Get(() => IsSelected); }
            set
            {
                bool wasChanged = value != IsSelected;

                Set(() => IsSelected, value);

                if (wasChanged)
                    EventAggregator.PublishTestResultFilterSelectionChanged();
            }
        }

        private void ToggleSelection()
        {
            IsSelected = !IsSelected;
        }
    }
}