#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AppsTracker.Data.Models;
using AppsTracker.Service;

namespace AppsTracker.Controllers
{
    [Export(typeof(IAppearanceController))]
    internal sealed class AppearanceController : IAppearanceController
    {
        private bool isLightTheme;
        private bool isTrackingEnabled;

        private readonly IWindowService windowService;


        [ImportingConstructor]
        public AppearanceController(IWindowService windowService)
        {
            this.windowService = windowService;
        }


        public void Initialize(Setting settings)
        {
            isLightTheme = settings.LightTheme;
            isTrackingEnabled = settings.TrackingEnabled;
            Application.Current.Resources.MergedDictionaries.Clear();
            ApplyTheme();
        }


        public void SettingsChanging(Setting settings)
        {
            bool isThemeChanging = false;
            if (isLightTheme != settings.LightTheme)
            {
                isLightTheme = settings.LightTheme;
                isThemeChanging = true;
            }

            if (isTrackingEnabled != settings.TrackingEnabled)
            {
                isTrackingEnabled = settings.TrackingEnabled;
                isThemeChanging = true;
            }

            if (isThemeChanging)
                ApplyTheme();
        }


        private void ApplyTheme()
        {
            var oldDictionary = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Contains("WindowBackgroundColor"));
            var newDictionary = GetNewResourceDictionary();

            if (App.Current.MainWindow == null || oldDictionary == null)
            {
                Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                return;
            }

            AnimateThemeChange(newDictionary, oldDictionary);
        }

        private ResourceDictionary GetNewResourceDictionary()
        {
            ResourceDictionary newDictionary;
            if (isLightTheme)
            {
                if (isTrackingEnabled)
                    newDictionary = new ResourceDictionary() { Source = new Uri("/Themes/RunningLight.xaml", UriKind.Relative) };
                else
                    newDictionary = new ResourceDictionary() { Source = new Uri("/Themes/StoppedLight.xaml", UriKind.Relative) };
            }
            else
            {
                if (isTrackingEnabled)
                    newDictionary = new ResourceDictionary() { Source = new Uri("/Themes/Running.xaml", UriKind.Relative) };
                else
                    newDictionary = new ResourceDictionary() { Source = new Uri("/Themes/Stopped.xaml", UriKind.Relative) };
            }
            return newDictionary;
        }

        private void AnimateThemeChange(ResourceDictionary newDictionary, ResourceDictionary oldDictionary)
        {
            var animation = new ColorAnimation();
            animation.From = (Color)oldDictionary["WindowBackgroundColor"];
            animation.To = (Color)newDictionary["WindowBackgroundColor"];
            animation.Duration = new Duration(TimeSpan.FromSeconds(0.5d));

            var mainWindow = (System.Windows.Window)windowService.GetMainShell();
            Storyboard.SetTarget(animation, mainWindow);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Background.Color"));

            Application.Current.Resources.MergedDictionaries.Add(newDictionary);
            Application.Current.Resources.MergedDictionaries.Remove(oldDictionary);

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }
}
