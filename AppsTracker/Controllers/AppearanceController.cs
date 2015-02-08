#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace AppsTracker.Controllers
{
    [Export(typeof(IAppearanceController))]
    internal sealed class AppearanceController : IAppearanceController
    {
        private bool _lightTheme;
        private bool _loggingEnabled;

        public void Initialize(Models.EntityModels.Setting settings)
        {
            _lightTheme = settings.LightTheme;
            _loggingEnabled = settings.LoggingEnabled;
            Application.Current.Resources.MergedDictionaries.Clear();
            ApplyTheme();
        }

        public void SettingsChanging(Models.EntityModels.Setting settings)
        {
            bool isThemeChanging = false;
            if (_lightTheme != settings.LightTheme)
            {
                _lightTheme = settings.LightTheme;
                isThemeChanging = true;
            }

            if (_loggingEnabled != settings.LoggingEnabled)
            {
                _loggingEnabled = settings.LoggingEnabled;
                isThemeChanging = true;
            }

            if (isThemeChanging)
                ApplyTheme();
        }

        private void ApplyTheme()
        {
            ResourceDictionary newDictionary;
            var oldDictionary = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Contains("WindowBackgroundColor"));

            if (_lightTheme)
            {
                if (_loggingEnabled)
                    newDictionary = new ResourceDictionary() { Source = new Uri("/Themes/RunningLight.xaml", UriKind.Relative) };
                else
                    newDictionary = new ResourceDictionary() { Source = new Uri("/Themes/StoppedLight.xaml", UriKind.Relative) };
            }
            else
            {
                if (_loggingEnabled)
                    newDictionary = new ResourceDictionary() { Source = new Uri("/Themes/Running.xaml", UriKind.Relative) };
                else
                    newDictionary = new ResourceDictionary() { Source = new Uri("/Themes/Stopped.xaml", UriKind.Relative) };
            }

            if (App.Current.MainWindow == null || oldDictionary == null)
            {
                Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                return;
            }

            ColorAnimation animation = new ColorAnimation();
            animation.From = (System.Windows.Media.Color)oldDictionary["WindowBackgroundColor"];
            animation.To = (System.Windows.Media.Color)newDictionary["WindowBackgroundColor"];
            animation.Duration = new Duration(TimeSpan.FromSeconds(0.4d));

            Storyboard.SetTarget(animation, App.Current.MainWindow);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Background.Color"));

            Application.Current.Resources.MergedDictionaries.Add(newDictionary);
            Application.Current.Resources.MergedDictionaries.Remove(oldDictionary);

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();

        }
    }
}
