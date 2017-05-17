﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SuperPopupSample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            var vm = new MainPageViewModel();
            BindingContext = vm;

            PopupService.Register(PopupType.Popup1, PopupView1);
            PopupService.Register(PopupType.Popup2, PopupView2);
        }
    }
}
