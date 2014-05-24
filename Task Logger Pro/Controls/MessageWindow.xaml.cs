using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Threading;

namespace Task_Logger_Pro.Controls
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
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
            StringBuilder stringBuilder = new StringBuilder("Ooops, this is awkward ... something went wrong.");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("Error: ");
            stringBuilder.Append(fail.Message);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("Inner exception: ");
            stringBuilder.Append(fail.InnerException);
            tbMessage.Text = stringBuilder.ToString();
        }

        public MessageWindow(IEnumerable< Exception> failCollection) :this()
        {
            StringBuilder stringBuilder = new StringBuilder("Ooops, this is awkward ... something went wrong.");
            foreach (var fail in failCollection)
            {
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(fail.Message); 
            }
            tbMessage.Text = stringBuilder.ToString();
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
            this.DialogResult = true;
            FadeUnloaded();
        }

        private void lblCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            FadeUnloaded();
        }
    }
}
