﻿using Microsoft.Mvp.Helpers;
using Microsoft.Mvp.ViewModels;
using System;
using Xamarin.Forms;

namespace Microsoft.Mvpui
{
    public partial class LiveIdLogOn : ContentPage
    {

        #region Constructor

        public LiveIdLogOn()
        {
            InitializeComponent();
            this.BindingContext = LiveIdLogOnViewModel.Instance;

            //TODO: Perhaps remove toolbaritems on Android?
        }

        #endregion

        #region Private and Protected Methods

        protected override void OnAppearing()
        {
            base.OnAppearing();

            WebView browserInstance = Content.FindByName<WebView>("browser");
            browserInstance.Navigating -= Browser_Navigating;
            browserInstance.Navigating += Browser_Navigating;
            browserInstance.Source = new UrlWebViewSource() { Url = LiveIdLogOnViewModel.Instance.LiveIdLogOnUrl };
            browserInstance.GoForward();
        }

        private async void ButtonClose_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PopModalAsync(true);
        }

        private async void Browser_Navigating(object sender, WebNavigatingEventArgs e)
        {
            Uri liveUrl = new Uri(e.Url, UriKind.Absolute);
            if (liveUrl.AbsoluteUri.Contains("code="))
            {
                if (Application.Current.Properties.ContainsKey(CommonConstants.AuthCodeKey))
                {
                    //Application.Current.Properties.Clear();
                    MvpHelper.RemoveProperties();
                }

                string auth_code = System.Text.RegularExpressions.Regex.Split(liveUrl.AbsoluteUri, "code=")[1];
                Application.Current.Properties.Add(CommonConstants.AuthCodeKey, auth_code);
                await LiveIdLogOnViewModel.Instance.GetAccessToken();

                var profileTest = await MvpHelper.MvpService.GetProfile(LogOnViewModel.StoredToken);
                if (profileTest == null || string.IsNullOrWhiteSpace(profileTest.DisplayName))
                {
                    await DisplayAlert(string.Empty, "Unable to validate MVP status, please login again with your MVP credentials.", "OK");
                    App.CookieHelper.ClearCookie();
                    LiveIdLogOnViewModel.Instance.SignOut();
                    await Navigation.PopModalAsync(true);
                }
                else
                {
                    switch (Device.RuntimePlatform)
                    {
                        case Device.iOS:
                            Application.Current.MainPage = new MainTabPageiOS();
                            break;
                        default:
                            Application.Current.MainPage = new MVPNavigationPage(new MainTabPage())
                            {
                                Title = "Microsoft MVP"
                            };
                            break;
                    }
                }

            }
        }

        #endregion

    }
}
