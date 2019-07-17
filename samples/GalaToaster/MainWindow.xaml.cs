using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaScript;
using GalaScript.Interfaces;

namespace GalaToaster
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private IEngine _engine;
        private Thread _main;

        public Dictionary<string, object> Aliases => _engine?.Current?.Aliases;

        public MainWindow()
        {
            InitializeComponent();

            _engine = new ScriptEngine(true);

            decimal Add(decimal[] arguments) => arguments.Sum();
            _engine.Register("add", (Func<decimal[], decimal>) Add);

            _engine.OnStartedHandler += () => Dispatcher.Invoke(() =>
            {
                ScriptBox.IsReadOnly = true;

                Build.IsEnabled = false;
                Pause.IsEnabled = true;
                StepOut.IsEnabled = false;
                Stop.IsEnabled = true;

                Status.Background = new SolidColorBrush(Colors.Green);
            });
            _engine.OnPausedHandler += () => Dispatcher.Invoke(() =>
            {
                Pause.IsEnabled = false;
                StepOut.IsEnabled = true;

                Status.Background = new SolidColorBrush(Colors.Yellow);

                var position = ScriptBox.GetCharacterIndexFromLineIndex((int)_engine.Current.CurrentLineNumber);
                ScriptBox.Select(position, ScriptBox.Text.IndexOf(Environment.NewLine, position) - position);

                Refresh_Click(null, null);
            });
            _engine.OnResumedHandler += () => Dispatcher.Invoke(() =>
            {
                StepOut.IsEnabled = false;
                Pause.IsEnabled = true;

                Status.Background = new SolidColorBrush(Colors.Green);
            });
            _engine.OnExitedHandler += () => Dispatcher.Invoke(() => Stop_Click(null, null));
        }

        private void Build_Click(object sender, RoutedEventArgs e)
        {
            _engine.Prepare(ScriptBox.Text);

            _main = new Thread(() => _engine.Run());
            _main.Start();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            _engine.Paused = true;
        }

        private void StepOut_Click(object sender, RoutedEventArgs e)
        {
            _engine.Paused = false;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            AliasList.Items.Clear();
            foreach (var alias in _engine.Current.Aliases)
            {
                AliasList.Items.Add(alias);
            }

            StackList.Items.Clear();
            foreach (var alias in _engine.Current.Stack)
            {
                StackList.Items.Add(alias);
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _main.Abort();

            _engine.Reset();

            ScriptBox.IsReadOnly = false;

            Pause.IsEnabled = false;
            StepOut.IsEnabled = false;
            Stop.IsEnabled = false;
            Build.IsEnabled = true;

            Status.Background = new SolidColorBrush(Colors.Red);
        }
    }
}
