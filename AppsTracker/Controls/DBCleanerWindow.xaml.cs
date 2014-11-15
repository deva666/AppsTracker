#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;


namespace AppsTracker.Controls
{
    public partial class DBCleanerWindow : Window
    {
        public DBCleanerWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => ScaleLoaded();
        }

        #region Animations
        private void ScaleLoaded()
        {
            DoubleAnimation scaleX = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(0.5)));
            DoubleAnimation scaleY = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(0.5)));

            scaleX.SetValue(Storyboard.TargetProperty, this);
            scaleY.SetValue(Storyboard.TargetProperty, this);

            Storyboard story = new Storyboard();
            Storyboard.SetTarget(scaleX, this);
            Storyboard.SetTarget(scaleY, this);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("RenderTransform.ScaleY"));

            this.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            story.Children.Add(scaleX);
            story.Children.Add(scaleY);
            story.Begin(this);
        }

        private void ScaleUnloaded()
        {
            DoubleAnimation scaleX = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(0.4)));
            DoubleAnimation scaleY = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(0.4)));

            scaleX.SetValue(Storyboard.TargetProperty, this);
            scaleY.SetValue(Storyboard.TargetProperty, this);

            Storyboard story = new Storyboard();
            Storyboard.SetTarget(scaleX, this);
            Storyboard.SetTarget(scaleY, this);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("RenderTransform.ScaleY"));

            this.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            story.Children.Add(scaleX);
            story.Children.Add(scaleY);
            story.Completed += (s, e) => { this.Close(); };
            story.Begin(this);
        }
        #endregion

        private void lblCancel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ScaleUnloaded();
        }

        private void lblClean_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //int days;
            //if (!int.TryParse(tbDays.Text, out days))
            //    return;

            //decimal beforeSize = Globals.GetDBSize();
            //await DBCleaner.DeleteOldScreenshotsAsync(days,true);
            //decimal afterSize = Globals.GetDBSize();
            //MessageWindow msgWindow = new MessageWindow(string.Format("Freed {0} MB", beforeSize - afterSize));
            //msgWindow.ShowDialog();
            ScaleUnloaded();
        }

        private void tbDays_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
