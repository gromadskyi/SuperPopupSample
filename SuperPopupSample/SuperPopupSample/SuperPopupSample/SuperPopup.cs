﻿using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SuperPopupSample
{
    [ContentProperty(nameof(PopupContent))]
    public class SuperPopup : ContentView
    {
        public const double ContentMargin = 10;

        AbsoluteLayout _rootLayout;
        SuperFrame _contentFrame;

        public static readonly BindableProperty PopupContentProperty =
            BindableProperty.Create(nameof(PopupContent),
                                    typeof(View),
                                    typeof(SuperPopup),
                                    default(View),
                                    propertyChanged: OnPopupContentPropertyChanged);

        public static readonly BindableProperty LocationRequestProperty =
            BindableProperty.Create(nameof(LocationRequest),
                                    typeof(Point),
                                    typeof(SuperPopup),
                                    default(Point),
                                    propertyChanged: OnLocationRequestPropertyChanged);

        public static readonly BindableProperty ProportionalSizeProperty =
            BindableProperty.Create(nameof(ProportionalSize),
                                    typeof(Size),
                                    typeof(SuperPopup),
                                    default(Size),
                                    propertyChanged: OnProportionalSizePropertyChanged);

        public static readonly BindableProperty RequiredSizeProperty =
            BindableProperty.Create(nameof(RequiredSize),
                                    typeof(Size),
                                    typeof(SuperPopup),
                                    default(Size),
                                    propertyChanged: OnRequiredSizePropertyChanged);

        public static readonly BindableProperty IsOpenProperty =
            BindableProperty.Create(nameof(IsOpen),
                                    typeof(bool),
                                    typeof(SuperPopup),
                                    false,
                                    propertyChanged: OnIsOpenPropertyChanged);

        public View PopupContent
        {
            get { return (View)GetValue(PopupContentProperty); }
            set { SetValue(PopupContentProperty, value); }
        }

        public Point LocationRequest
        {
            get { return (Point)GetValue(LocationRequestProperty); }
            set { SetValue(LocationRequestProperty, value); }
        }

        [TypeConverter(typeof(SizeTypeConverter))]
        public Size ProportionalSize
        {
            get { return (Size)GetValue(ProportionalSizeProperty); }
            set { SetValue(ProportionalSizeProperty, value); }
        }

        [TypeConverter(typeof(SizeTypeConverter))]
        public Size RequiredSize
        {
            get { return (Size)GetValue(RequiredSizeProperty); }
            set { SetValue(RequiredSizeProperty, value); }
        }

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        /// <summary>
        /// Coordinates of the Top-Left corner of PopupContent.
        /// </summary>
        public Point Location { get; private set; }

        public SuperPopup()
        {
        }

        static void OnPopupContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue is View view)
            {
                var popup = bindable as SuperPopup;
                popup.UpdateContent(view);
            }
        }

        void UpdateContent(View view)
        {
            // Note that UWP will not handle tap on AbsoluteLayout if remove Background
            _rootLayout = new AbsoluteLayout() { BackgroundColor = Color.Transparent };

            _contentFrame = new SuperFrame
            {
                HasShadow = true,
                Content = view
            };

            _rootLayout.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(ClickOutsidePopupContent)
            });

            _contentFrame.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(ClickOnPopupContent)
            });

            UpdateRequiredSize(RequiredSize);
            UpdateProportionalSize(ProportionalSize);
            UpdateLocation(LocationRequest);

            _rootLayout.Children.Add(_contentFrame);
            Content = _rootLayout;

            Opacity = 0;
            InputTransparent = true;
            _rootLayout.Opacity = 0;
            _rootLayout.InputTransparent = true;
        }

        void ClickOnPopupContent()
        {
            // Ignoring required for iOS to prevent ClickOutsidePopupContent from calling.
        }

        void ClickOutsidePopupContent()
        {
            IsOpen = false;
        }

        static void OnLocationRequestPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue is Point location)
            {
                var popup = bindable as SuperPopup;
                popup.UpdateLocation(location);
            }
        }

        void UpdateLocation(Point location)
        {
            if (_contentFrame != null)
            {
                var arrowRotation = 0;
                var x = location.X;
                var y = location.Y;
                var width = _contentFrame.Width;
                var height = _contentFrame.Height;

                // crossed the right edge of the screen
                if (x + width / 2 + ContentMargin > _rootLayout.Width)
                {
                    x = _rootLayout.Width - width - ContentMargin;
                }
                else
                {
                    if (x - width / 2 > ContentMargin)
                    {
                        x -= width / 2;
                    }
                    else
                    {
                        x = ContentMargin;
                    }
                }

                // crossed the bottom edge of the screen
                if (y + height + ContentMargin * 2 > _rootLayout.Height)
                {
                    y = y - height - ContentMargin;
                    arrowRotation = 180;
                }
                else
                {
                    y += ContentMargin;
                }

                var arrowX = Math.Min(Math.Max(location.X - x, ContentMargin * 1.5), width - ContentMargin * 1.5);

                if (!RequiredSize.IsZero)
                {
                    width = RequiredSize.Width;
                    height = RequiredSize.Height;
                    AbsoluteLayout.SetLayoutFlags(_contentFrame, AbsoluteLayoutFlags.None);
                }

                if (!ProportionalSize.IsZero)
                {
                    width = ProportionalSize.Width;
                    height = ProportionalSize.Height;
                    AbsoluteLayout.SetLayoutFlags(_contentFrame, AbsoluteLayoutFlags.SizeProportional);
                }

                Location = new Point(x, y);

                AbsoluteLayout.SetLayoutBounds(_contentFrame, new Rectangle(x, y, width, height));
                _contentFrame.DrawArrow(new DrawArrowRequest
                {
                    Location = new Point(arrowX, location.Y - y),
                    Rotation = arrowRotation
                });
            }
        }

        static void OnProportionalSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue is Size size)
            {
                var popup = bindable as SuperPopup;
                popup.UpdateProportionalSize(size);
            }
        }

        void UpdateProportionalSize(Size size)
        {
            if (_contentFrame != null && !size.IsZero)
            {
                AbsoluteLayout.SetLayoutFlags(_contentFrame, AbsoluteLayoutFlags.SizeProportional);
                AbsoluteLayout.SetLayoutBounds(_contentFrame, new Rectangle(LocationRequest.X, LocationRequest.Y, size.Width, size.Height));
            }
        }

        static void OnRequiredSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue is Size size)
            {
                var popup = bindable as SuperPopup;
                popup.UpdateRequiredSize(size);
            }
        }

        void UpdateRequiredSize(Size size)
        {
            if (_contentFrame != null && !size.IsZero)
            {
                AbsoluteLayout.SetLayoutFlags(_contentFrame, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(_contentFrame, new Rectangle(LocationRequest.X, LocationRequest.Y, size.Width, size.Height));
            }
        }

        static async void OnIsOpenPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var popup = bindable as SuperPopup;
            await popup.UpdateIsOpen((bool)newValue);
        }

        Task UpdateIsOpen(bool isOpen)
        {
            if (isOpen)
            {
                return ShowAsync();
            }
            else
            {
                return HideAsync();
            }
        }

        async Task ShowAsync()
        {
            _contentFrame.Scale = 0;
            Opacity = 1;
            InputTransparent = false;
            _rootLayout.Opacity = 1;
            _rootLayout.InputTransparent = false;
            await _contentFrame.ScaleTo(1, 100);
        }

        async Task HideAsync()
        {
            await _contentFrame.ScaleTo(0, 100);
            Opacity = 0;
            InputTransparent = true;
            _rootLayout.Opacity = 0;
            _rootLayout.InputTransparent = true;
            _contentFrame.Scale = 1;
        }
    }
}
