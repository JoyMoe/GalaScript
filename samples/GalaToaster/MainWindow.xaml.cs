using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using GalaScript;
using GalaScript.Abstract;

namespace GalaToaster
{
    public class MainWindow : Window
    {
        private readonly TextEditor _scriptBox;
        private readonly Button _build;
        private readonly Button _pause;
        private readonly Button _stepIn;
        private readonly Button _resume;
        private readonly Button _stop;
        private readonly TextBlock _status;
        private readonly ListBox _aliasList;
        private readonly ListBox _stackList;

        private readonly IEngine _engine;
        private Thread _main;

        public Dictionary<string, object> Aliases => _engine?.Current?.Aliases;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            _scriptBox = this.Get<TextEditor>("ScriptBox");
            _build = this.Get<Button>("Build");
            _pause = this.Get<Button>("Pause");
            _stepIn = this.Get<Button>("StepIn");
            _resume = this.Get<Button>("Resume");
            _stop = this.Get<Button>("Stop");
            _status = this.Get<TextBlock>("Status");
            _aliasList = this.Get<ListBox>("AliasList");
            _stackList = this.Get<ListBox>("StackList");

            _engine = new ScriptEngine(true);

            decimal Add(decimal[] arguments) => arguments.Sum();
            _engine.Register("add", (Func<decimal[], decimal>)Add);

            _engine.OnStartedHandler += () => Dispatcher.UIThread.InvokeAsync(() =>
            {
                _scriptBox.IsReadOnly = true;

                _build.IsEnabled = false;
                _pause.IsEnabled = true;
                _stepIn.IsEnabled = false;
                _resume.IsEnabled = false;
                _stop.IsEnabled = true;

                _status.Background = new SolidColorBrush(Colors.Green);
            });
            _engine.OnPausedHandler += () => Dispatcher.UIThread.InvokeAsync(() =>
            {
                _pause.IsEnabled = false;
                _stepIn.IsEnabled = true;
                _resume.IsEnabled = true;

                _status.Background = new SolidColorBrush(Colors.Yellow);

                Refresh_Click(null, null);
            });
            _engine.OnResumedHandler += () => Dispatcher.UIThread.InvokeAsync(() =>
            {
                _stepIn.IsEnabled = false;
                _resume.IsEnabled = false;
                _pause.IsEnabled = true;

                _status.Background = new SolidColorBrush(Colors.Green);
            });
            _engine.OnExitedHandler += () => Dispatcher.UIThread.InvokeAsync(() => Stop_Click(null, null));

            Closing += (s, e) => Stop_Click(null, null);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Build_Click(object sender, RoutedEventArgs e)
        {
            _engine.Prepare(_scriptBox.Text);

            _main = new Thread(() => _engine.Run());
            _main.Start();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            _engine.Pause();
        }

        private void StepIn_Click(object sender, RoutedEventArgs e)
        {
            _engine.StepIn();
        }

        private void Resume_Click(object sender, RoutedEventArgs e)
        {
            _engine.Continue();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _aliasList.Items = null;
            _aliasList.Items = _engine.Current?.Aliases;

            _stackList.Items = null;
            _stackList.Items = _engine.Current?.Stack;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _engine.Cancel();
            _engine.Reset();

            _scriptBox.IsReadOnly = false;

            _pause.IsEnabled = false;
            _stepIn.IsEnabled = false;
            _resume.IsEnabled = false;
            _stop.IsEnabled = false;
            _build.IsEnabled = true;

            _status.Background = new SolidColorBrush(Colors.Red);
        }
    }
}
