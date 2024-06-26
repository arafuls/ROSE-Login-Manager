﻿using Microsoft.Xaml.Behaviors;
using ScrollViewer;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;


// This code is almost entirely adapted from https://www.wpf-controls.com/wpf-smooth-scroll-viewer/


namespace ROSE_Login_Manager.Resources.Util
{
    /// <summary>
    ///     SmoothScrollViewer class inherits from ScrollViewer and provides smoother scrolling behavior.
    /// </summary>
    public class SmoothScrollViewer : System.Windows.Controls.ScrollViewer
    {
        public SmoothScrollViewer()
        {
            Loaded += ScrollViewer_Loaded;
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollInfo = new ScrollInfoAdapter(ScrollInfo);
        }
    }



    /// <summary>
    ///     SmoothScrollViewerBehavior class defines behavior for SmoothScrollViewer.
    /// </summary>
    public class SmoothScrollViewerBehavior : Behavior<System.Windows.Controls.ScrollViewer>
    {
        private const double AutoScrollMargin = 20.0;
        private const double AutoScrollSpeed = 15.0;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += ScrollViewerLoaded;
            AssociatedObject.PreviewDragOver += OnDragOver;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= ScrollViewerLoaded;
            AssociatedObject.DragOver -= OnDragOver;
        }

        private void ScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            var property = AssociatedObject.GetType().GetProperty("ScrollInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            if (property?.GetValue(AssociatedObject) is IScrollInfo scrollInfo)
            {
                property?.SetValue(AssociatedObject, new ScrollInfoAdapter(scrollInfo));
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            AutoScrollIfNeeded(e);
        }

        private void AutoScrollIfNeeded(DragEventArgs e)
        {
            // Get the position of the drag event relative to the scroll viewer
            var position = e.GetPosition(AssociatedObject);

            // Define the vertical range for triggering scrolling
            double topThreshold = AutoScrollMargin;
            double bottomThreshold = AssociatedObject.ActualHeight - AutoScrollMargin;

            // Check if the drag position is within the vertical range
            if (position.Y <= topThreshold)
            {
                ScrollUp();
            }
            else if (position.Y >= bottomThreshold)
            {
                ScrollDown();
            }
        }

        private void ScrollUp()
        {
            var adapter = GetScrollInfoAdapter();
            adapter?.SetVerticalOffset(adapter.VerticalOffset - AutoScrollSpeed);
        }

        private void ScrollDown()
        {
            var adapter = GetScrollInfoAdapter();
            adapter?.SetVerticalOffset(adapter.VerticalOffset + AutoScrollSpeed);
        }

        private ScrollInfoAdapter GetScrollInfoAdapter()
        {
            var property = AssociatedObject.GetType().GetProperty("ScrollInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            return property?.GetValue(AssociatedObject) as ScrollInfoAdapter;
        }
    }
}



namespace ScrollViewer
{
    /// <summary>
    ///     ScrollInfoAdapter class adapts the IScrollInfo interface to provide smoother scrolling behavior.
    /// </summary>
    public class ScrollInfoAdapter(IScrollInfo child) : UIElement, IScrollInfo
    {
        private readonly IScrollInfo _child = child;
        private double _computedVerticalOffset = 0;
        private double _computedHorizontalOffset = 0;
        internal const double _scrollLineDelta = 16.0;
        internal const double _mouseWheelDelta = 48.0;

        public bool CanVerticallyScroll
        {
            get => _child.CanVerticallyScroll;
            set => _child.CanVerticallyScroll = value;
        }
        public bool CanHorizontallyScroll
        {
            get => _child.CanHorizontallyScroll;
            set => _child.CanHorizontallyScroll = value;
        }

        public double ExtentWidth => _child.ExtentWidth;

        public double ExtentHeight => _child.ExtentHeight;

        public double ViewportWidth => _child.ViewportWidth;

        public double ViewportHeight => _child.ViewportHeight;

        public double HorizontalOffset => _child.HorizontalOffset;
        public double VerticalOffset => _child.VerticalOffset;

        public System.Windows.Controls.ScrollViewer ScrollOwner
        {
            get => _child.ScrollOwner;
            set => _child.ScrollOwner = value;
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return _child.MakeVisible(visual, rectangle);
        }

        public void LineUp()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.LineUp();
            else
                VerticalScroll(_computedVerticalOffset - _scrollLineDelta);
        }

        public void LineDown()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.LineDown();
            else
                VerticalScroll(_computedVerticalOffset + _scrollLineDelta);
        }

        public void LineLeft()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.LineLeft();
            else
                HorizontalScroll(_computedHorizontalOffset - _scrollLineDelta);
        }

        public void LineRight()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.LineRight();
            else
                HorizontalScroll(_computedHorizontalOffset + _scrollLineDelta);
        }

        public void MouseWheelUp()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.MouseWheelUp();
            else
                VerticalScroll(_computedVerticalOffset - _mouseWheelDelta);
        }

        public void MouseWheelDown()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.MouseWheelDown();
            else
                VerticalScroll(_computedVerticalOffset + _mouseWheelDelta);
        }

        public void MouseWheelLeft()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.MouseWheelLeft();
            else
                HorizontalScroll(_computedHorizontalOffset - _mouseWheelDelta);
        }

        public void MouseWheelRight()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.MouseWheelRight();
            else
                HorizontalScroll(_computedHorizontalOffset + _mouseWheelDelta);
        }

        public void PageUp()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.PageUp();
            else
                VerticalScroll(_computedVerticalOffset - ViewportHeight);
        }

        public void PageDown()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.PageDown();
            else
                VerticalScroll(_computedVerticalOffset + ViewportHeight);
        }

        public void PageLeft()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.PageLeft();
            else
                HorizontalScroll(_computedHorizontalOffset - ViewportWidth);
        }

        public void PageRight()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.PageRight();
            else
                HorizontalScroll(_computedHorizontalOffset + ViewportWidth);
        }

        public void SetHorizontalOffset(double offset)
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.SetHorizontalOffset(offset);
            else
            {
                _computedHorizontalOffset = offset;
                Animate(HorizontalScrollOffsetProperty, offset, 0);
            }
        }

        public void SetVerticalOffset(double offset)
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.SetVerticalOffset(offset);
            else
            {
                _computedVerticalOffset = offset;
                Animate(VerticalScrollOffsetProperty, offset, 0);
            }
        }

        #region not exposed methods
        private void Animate(DependencyProperty property, double targetValue, int duration = 300)
        {
            // Make a smooth animation that starts and ends slowly
            var keyFramesAnimation = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(duration)
            };
            keyFramesAnimation.KeyFrames.Add(
                new EasingDoubleKeyFrame(
                    targetValue,
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration)),
                    new QuadraticEase() { EasingMode = EasingMode.EaseInOut }));

            BeginAnimation(property, keyFramesAnimation);
        }

        private void VerticalScroll(double val)
        {
            if (Math.Abs(_computedVerticalOffset - ValidateVerticalOffset(val)) > 0.1)//prevent restart of animation in case of frequent event fire
            {
                _computedVerticalOffset = ValidateVerticalOffset(val);
                Animate(VerticalScrollOffsetProperty, _computedVerticalOffset);
            }
        }

        private void HorizontalScroll(double val)
        {
            if (Math.Abs(_computedHorizontalOffset - ValidateHorizontalOffset(val)) > 0.1)//prevent restart of animation in case of frequent event fire
            {
                _computedHorizontalOffset = ValidateHorizontalOffset(val);
                Animate(HorizontalScrollOffsetProperty, _computedHorizontalOffset);
            }
        }

        private double ValidateVerticalOffset(double verticalOffset)
        {
            if (verticalOffset < 0)
                return 0;
            if (verticalOffset > _child.ScrollOwner.ScrollableHeight)
                return _child.ScrollOwner.ScrollableHeight;
            return verticalOffset;
        }

        private double ValidateHorizontalOffset(double horizontalOffset)
        {
            if (horizontalOffset < 0)
                return 0;
            if (horizontalOffset > _child.ScrollOwner.ScrollableWidth)
                return _child.ScrollOwner.ScrollableWidth;
            return horizontalOffset;
        }
        #endregion

        #region Helper dependency properties as scrollbars are not animatable by default
        internal double VerticalScrollOffset
        {
            get { return (double)GetValue(VerticalScrollOffsetProperty); }
            set { SetValue(VerticalScrollOffsetProperty, value); }
        }
        internal static readonly DependencyProperty VerticalScrollOffsetProperty =
            DependencyProperty.Register("VerticalScrollOffset", typeof(double), typeof(ScrollInfoAdapter),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnVerticalScrollOffsetChanged)));
        private static void OnVerticalScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var smoothScrollViewer = (ScrollInfoAdapter)d;
            smoothScrollViewer._child.SetVerticalOffset((double)e.NewValue);
        }

        internal double HorizontalScrollOffset
        {
            get { return (double)GetValue(HorizontalScrollOffsetProperty); }
            set { SetValue(HorizontalScrollOffsetProperty, value); }
        }
        internal static readonly DependencyProperty HorizontalScrollOffsetProperty =
            DependencyProperty.Register("HorizontalScrollOffset", typeof(double), typeof(ScrollInfoAdapter),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnHorizontalScrollOffsetChanged)));
        private static void OnHorizontalScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var smoothScrollViewer = (ScrollInfoAdapter)d;
            smoothScrollViewer._child.SetHorizontalOffset((double)e.NewValue);
        }
        #endregion
    }
}