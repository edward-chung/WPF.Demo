using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WPF.Demo.Navigation.Behaviors;

internal class SlidingNavigationBehavior : Behavior<FrameworkElement>
{
    private FrameworkElement sliding = new();

    public string SlidingName
    {
        get { return (string)GetValue(SlidingNameProperty); }
        set { SetValue(SlidingNameProperty, value); }
    }

    public static readonly DependencyProperty SlidingNameProperty = DependencyProperty.Register(
        nameof(SlidingName),
        typeof(string),
        typeof(SlidingNavigationBehavior)
    );

    private IList<RadioButton> Children { get; set; }

    private FrameworkElement prev { get; set; }

    protected override void OnAttached()
    {
        AssociatedObject.Loaded += (_, _) =>
        {
            sliding = (FrameworkElement)AssociatedObject.FindName(SlidingName);
            Children = GetChildren<RadioButton>().ToList();
            foreach (var child in Children)
            {
                child.Checked += OnRadioButtonChecked;
                if ((bool)child.IsChecked)
                {
                    BeginAnimation(child);
                }
            }
        };
    }

    private void OnRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        BeginAnimation(sender as FrameworkElement);
    }

    private void BeginAnimation(FrameworkElement e)
    {
        var offset = VisualTreeHelper.GetOffset(e);
        if (sliding.Width > 0 && sliding.Height > 0)
        {
            var storyboard = new Storyboard();
            var easingFuction = new PowerEase { EasingMode = EasingMode.EaseOut, };

            AddStoryboard(storyboard, easingFuction, e.ActualWidth, "Width");
            AddStoryboard(storyboard, easingFuction, e.ActualHeight, "Height");
            AddStoryboard(
                storyboard,
                easingFuction,
                offset.X,
                "(UIElement.RenderTransform).(TranslateTransform.X)"
            );
            AddStoryboard(
                storyboard,
                easingFuction,
                offset.Y,
                "(UIElement.RenderTransform).(TranslateTransform.Y)"
            );

            storyboard.Begin();
        }
        else
        {
            sliding.Width = e.ActualWidth;
            sliding.Height = e.ActualHeight;
            sliding.RenderTransform = new TranslateTransform(offset.X, offset.Y);
        }
    }

    private void AddStoryboard(
        Storyboard storyboard,
        PowerEase powerEase,
        double toValue,
        string path
    )
    {
        var animation = new DoubleAnimation(toValue, new Duration(TimeSpan.FromMilliseconds(200)))
        {
            EasingFunction = powerEase
        };
        Storyboard.SetTarget(animation, sliding);
        Storyboard.SetTargetProperty(animation, new PropertyPath(path));
        storyboard.Children.Add(animation);
    }

    private IEnumerable<T> GetChildren<T>()
    {
        return AssociatedObject switch
        {
            Panel panel => panel.Children.OfType<T>(),
            ItemsControl itemsControl => itemsControl.Items.OfType<T>(),
            _ => Enumerable.Empty<T>(),
        };
    }
}
