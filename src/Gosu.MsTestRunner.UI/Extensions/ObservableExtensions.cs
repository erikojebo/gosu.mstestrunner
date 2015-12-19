using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Gosu.MsTestRunner.UI.Extensions
{
    public static class ObservableExtensions
    {
        public static void ResetTo<T>(this ObservableCollection<T> collection, IEnumerable<T> newItems)
        {
            collection.Clear();

            foreach (var newItem in newItems)
            {
                collection.Add(newItem);
            }
        }
    }
}