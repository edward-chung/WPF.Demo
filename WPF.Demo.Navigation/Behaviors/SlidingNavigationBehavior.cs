using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

using Microsoft.Xaml.Behaviors;

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

	protected override void OnAttached()
	{
		AssociatedObject.Loaded += (_, _) =>
		{
			sliding = (FrameworkElement)AssociatedObject.FindName(SlidingName);
			Children = GetChildren<RadioButton>().ToList();
			foreach (var child in Children)
			{
				child.MouseEnter += Child_MouseEnter;
				child.MouseLeave += Child_MouseLeave;
				if (child.IsChecked == true)
				{
					InitSliding(child);
				}
			}
		};
	}

	private void Child_MouseEnter(object sender, MouseEventArgs e)
	{
		if (sender is FrameworkElement el)
		{
			BeginAnimation(el);
		}
	}

	private void Child_MouseLeave(object sender, MouseEventArgs e)
	{
		var el = Children.FirstOrDefault(x => x.IsChecked == true);
		if (el is not null)
		{
			BeginAnimation(el);
		}
	}

	private void InitSliding(FrameworkElement e)
	{
		var p = GetPosition(e);
		sliding.Width = p.ActualWidth;
		sliding.Height = p.ActualHeight;
		sliding.RenderTransform = new TranslateTransform(p.OffSetX, p.OffSetY);
	}

	private void BeginAnimation(FrameworkElement e)
	{
		var position = GetPosition(e);
		var storyboard = new Storyboard();
		var easingFuction = new PowerEase { EasingMode = EasingMode.EaseOut, };

		AddStoryboard(storyboard, easingFuction, position.ActualWidth, "Width");
		AddStoryboard(storyboard, easingFuction, position.ActualHeight, "Height");
		AddStoryboard(
			storyboard,
			easingFuction,
			position.OffSetX,
			"(UIElement.RenderTransform).(TranslateTransform.X)"
		);
		AddStoryboard(
			storyboard,
			easingFuction,
			position.OffSetY,
			"(UIElement.RenderTransform).(TranslateTransform.Y)"
		);

		storyboard.Begin();
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

	private Position GetPosition(FrameworkElement e)
	{
		var offset = VisualTreeHelper.GetOffset(e);
		return new Position(e.ActualWidth, e.ActualHeight, offset.X, offset.Y);
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

internal record struct Position(double ActualWidth, double ActualHeight, double OffSetX, double OffSetY);
