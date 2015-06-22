using System;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;

namespace AppsTracker.Widgets
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "Message window")]
    public partial class MessageWindow : Window, IShell
    {
        public MessageWindow()
        {
            InitializeComponent();
        }

        public MessageWindow(string message)
            : this()
        {
            tbMessage.Text = message;
        }

        public MessageWindow(string message, bool displayCancel)
            : this(message)
        {
            if (displayCancel) lblCancel.Visibility = System.Windows.Visibility.Visible;
        }

        public MessageWindow(Exception fail)
            : this()
        {
            SetMessageText(fail);
        }

        private void SetMessageText(Exception fail)
        {
            var stringBuilder = new StringBuilder("Ooops, this is awkward ... something went wrong.");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("Error: ");
            stringBuilder.Append(fail.Message);
            stringBuilder.Append(Environment.NewLine);
            if (fail.InnerException != null)
            {
                stringBuilder.Append("Inner exception: ");
                stringBuilder.Append(fail.InnerException);
            }
            tbMessage.Text = stringBuilder.ToString();
            lblReport.Visibility = System.Windows.Visibility.Visible;
        }


        private void FadeUnloaded()
        {
            DoubleAnimation fadeOut = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(0.6)));

            fadeOut.SetValue(Storyboard.TargetProperty, this);

            Storyboard story = new Storyboard();
            Storyboard.SetTarget(fadeOut, this);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));

            story.Children.Add(fadeOut);
            story.Completed += (s, e) => { this.Close(); };
            story.Begin(this);
        }

        private void lblOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = true;
            }
            catch (InvalidOperationException)
            {
                //do nothing, window not modal
            }
            FadeUnloaded();
        }

        private void lblCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = false;
            }
            catch (InvalidOperationException)
            {
                //do nothing, window not modal
            }
            FadeUnloaded();
        }


        public object ViewArgument
        {
            get
            {
                return tbMessage.Text;
            }
            set
            {
                if(value is string)
                {
                    tbMessage.Text = (string)value;
                }
                else if(value is Exception)
                {
                    SetMessageText((Exception)value);
                }
            }
        }
    }
}
