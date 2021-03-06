﻿using Android.Views;
using SuperPopupSample.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("SuperForms")]
[assembly: ExportEffect(typeof(TapGestureEffect), nameof(TapGestureEffect))]
namespace SuperPopupSample.Droid
{
    public class TapGestureEffect : PlatformEffect
    {
        Android.Views.View _view;

        protected override void OnAttached()
        {
            _view = Control ?? Container;
            _view.Touch += OnControlTouch;
        }

        protected override void OnDetached()
        {
            _view.Touch -= OnControlTouch;
            _view = null;
        }

        void OnControlTouch(object sender, Android.Views.View.TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Up)
            {
                var command = Gestures.GetTappedCommand(Element);
                if (command != null)
                {
                    var statusBarHeight = Helpers.GetStatusBarHeight(_view.Context);

                    int[] coordinates = new int[2];
                    _view.GetLocationOnScreen(coordinates);
                    var x = e.Event.GetX() + coordinates[0];
                    var y = e.Event.GetY() + coordinates[1] - statusBarHeight;

                    var convertedPoint = Helpers.PxToDp(new Point(x, y), _view.Resources.DisplayMetrics);
                    if (command.CanExecute(convertedPoint))
                    {
                        command.Execute(convertedPoint);
                    }
                }
            }
        }
    }
}