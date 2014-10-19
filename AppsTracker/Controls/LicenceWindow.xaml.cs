using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppsTracker.Controls
{
    public partial class LicenceWindow : Window
    {
        public LicenceWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => ScaleLoaded();
        }

        #region Animations

        private void ScaleLoaded()
        {
            DoubleAnimation scaleX = new DoubleAnimation(0.3, 1.0, new Duration(TimeSpan.FromSeconds(0.5)));
            DoubleAnimation scaleY = new DoubleAnimation(0.3, 1.0, new Duration(TimeSpan.FromSeconds(0.5)));

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
            DoubleAnimation scaleX = new DoubleAnimation(1.0, 0.1, new Duration(TimeSpan.FromSeconds(0.4)));
            DoubleAnimation scaleY = new DoubleAnimation(1.0, 0.1, new Duration(TimeSpan.FromSeconds(0.4)));

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

        private void ApplyLicense()
        {
            string username = tbUsername.Text;
            string license = tbLicense.Text;
            string verify = string.Format("{0} {1} {2}", username.Trim(), Constants.APP_NAME, Assembly.GetExecutingAssembly().GetName().Version.Major.ToString());
            if (license == Encryption.Encrypt.GetEncryptedString(verify))
            {
                CreateLicence(username, license);
            }
            else
            {
                MessageWindow messageWindow = new MessageWindow("Invalid license key.");
                messageWindow.Owner = this;
                messageWindow.ShowDialog();
            }
        }

        private void CreateLicence(string username, string license)
        {
            string serialized = username + Environment.NewLine + license;
            App.UzerSetting.Username = username;
            App.UzerSetting.Licence = true;
            this.DialogResult = true;
            ScaleUnloaded();
        }

        private void lblOK_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ApplyLicense();
        }

        private void lblCancel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ScaleUnloaded();
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyLicense();
                e.Handled = true;
            }

            if (e.Key == Key.Escape)
            {
                ScaleUnloaded();
                e.Handled = true;
            }
        }
    }
}
