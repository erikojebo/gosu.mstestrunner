using System.Windows;
using System.Windows.Controls;

namespace Gosu.MsTestRunner.UI.Infrastructure
{
    public class ProgressBarBehaviours
    {
        public static readonly DependencyProperty StopAnimationOnCompletionProperty =
            DependencyProperty.RegisterAttached("StopAnimationOnCompletion", typeof(bool), typeof(ProgressBarBehaviours),
                new PropertyMetadata(OnStopAnimationOnCompletionChanged));

        public static bool GetStopAnimationOnCompletion(ProgressBar progressBar)
        {
            return (bool)progressBar.GetValue(StopAnimationOnCompletionProperty);
        }

        public static void SetStopAnimationOnCompletion(ProgressBar progressBar, bool value)
        {
            progressBar.SetValue(StopAnimationOnCompletionProperty, value);
        }

        private static void OnStopAnimationOnCompletionChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            var progressBar = obj as ProgressBar;
            if (progressBar == null) return;

            var stopAnimationOnCompletion = (bool)e.NewValue;

            if (stopAnimationOnCompletion)
            {
                progressBar.Loaded += StopAnimationOnCompletion_Loaded;
                progressBar.ValueChanged += StopAnimationOnCompletion_ValueChanged;
            }
            else
            {
                progressBar.Loaded -= StopAnimationOnCompletion_Loaded;
                progressBar.ValueChanged -= StopAnimationOnCompletion_ValueChanged;
            }

            if (progressBar.IsLoaded)
            {
                ReevaluateAnimationVisibility(progressBar);
            }
        }

        private static void StopAnimationOnCompletion_Loaded(object sender, RoutedEventArgs e)
        {
            ReevaluateAnimationVisibility((ProgressBar)sender);
        }

        private static void StopAnimationOnCompletion_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            var progressBar = (ProgressBar)sender;

            if (e.NewValue == progressBar.Maximum || e.OldValue == progressBar.Maximum)
            {
                ReevaluateAnimationVisibility(progressBar);
            }
        }

        private static void ReevaluateAnimationVisibility(ProgressBar progressBar)
        {
            if (GetStopAnimationOnCompletion(progressBar))
            {
                var animationElement = GetAnimationElement(progressBar);
                if (animationElement != null)
                {
                    if (progressBar.Value == progressBar.Maximum)
                    {
                        animationElement.SetCurrentValue(UIElement.VisibilityProperty, Visibility.Collapsed);
                    }
                    else
                    {
                        animationElement.InvalidateProperty(UIElement.VisibilityProperty);
                    }
                }
            }
        }

        private static DependencyObject GetAnimationElement(ProgressBar progressBar)
        {
            var template = progressBar.Template;
            if (template == null) return null;

            return template.FindName("PART_GlowRect", progressBar) as DependencyObject;
        }
    }
}
