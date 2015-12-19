using Gosu.MsTestRunner.UI.Infrastructure;
using Gosu.MsTestRunner.UI.Mvvm;

namespace Gosu.MsTestRunner.UI.ViewModels
{
    public class TestCategoryViewModel : ViewModelBase
    {
        public TestCategoryViewModel(string name)
        {
            Name = name;
            ToggleSelectionCommand = new DelegateCommand(ToggleSelection);
        }

        public DelegateCommand ToggleSelectionCommand { get; }

        public string Name { get; set; }

        public bool IsSelected
        {
            get { return Get(() => IsSelected); }
            set
            {
                bool wasChanged = value != IsSelected;

                Set(() => IsSelected, value);

                if (wasChanged)
                    EventAggregator.PublishTestCategorySelectionChanged();
            }
        }

        private void ToggleSelection()
        {
            IsSelected = !IsSelected;
        }
    }
}