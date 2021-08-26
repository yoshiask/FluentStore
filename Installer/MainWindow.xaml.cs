using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using TaskDialogInterop;

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Page> _Steps = new()
        {
            new Steps.S00_Welcome(),
            new Steps.S01_Updates(),
            new Steps.S02_License(),
            new Steps.S03_BeginInstall(),
            new Steps.S04_Installing(),
            new Steps.S05_Completed()
        };
        public List<Page> Steps
        {
            get => _Steps;
            set => _Steps = value;
        }

        public const string DefaultNextButtonText = "Next >";
        public const string DefaultBackButtonText = "< Back";

        public void SetNextButtonText(string? text = null) => NextButton.Content = text ?? DefaultNextButtonText;
        public void SetBackButtonText(string? text = null) => BackButton.Content = text ?? DefaultBackButtonText;
        public void SetNextButtonEnabled(bool? isEnabled = null) => NextButton.IsEnabled = isEnabled ?? true;
        public void SetBackButtonEnabled(bool? isEnabled = null) => BackButton.IsEnabled = isEnabled ?? true;
        public void SetCancelButtonEnabled(bool? isEnabled = null) => CancelButton.IsEnabled = isEnabled ?? true;

        public MainWindow()
        {
            InitializeComponent();
            App.InstallerWindow = this;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Cancel();
        private void NextButton_Click(object sender, RoutedEventArgs e) => NextStep();
        private void BackButton_Click(object sender, RoutedEventArgs e) => PreviousStep();

        private void Window_Loaded(object sender, RoutedEventArgs e) => NextStep();

        int curStep = -1;
        public void NextStep()
        {
            if (curStep + 1 >= Steps.Count)
            {
                // No more steps, setup done
                App.Current.Shutdown();
                return;
            }

            curStep++;
            StepFrame.Content = Steps[curStep];
        }

        public void PreviousStep()
        {
            if (curStep <= 0)
            {
                Cancel();
                return;
            }

            curStep--;
            StepFrame.Content = Steps[curStep];
        }

        public void Cancel(bool confirm = true)
        {
            bool cancel = true;
            if (confirm)
            {
                // Confirm cancel
                TaskDialogOptions config = new TaskDialogOptions();

                config.Owner = this;
                config.Title = "Fluent Store Installer";
                config.MainInstruction = "Are you sure you want to cancel?";
                config.Content = "Setup is not complete. If you exit now, the app will not be installed.\r\n\r\n" +
                                 "You may run the setup again at another time to complete the installation.";
                config.CommonButtons = TaskDialogCommonButtons.YesNo;
                config.MainIcon = VistaTaskDialogIcon.Warning;

                cancel = TaskDialog.Show(config).Result == TaskDialogSimpleResult.Yes;
            }
            if (cancel)
            {
                if (StepFrame.Content is Steps.S04_Installing installStep)
                    installStep.Cancel();
                App.Current.Shutdown();
            }
        }

        public void ShowErrorMessage(string msg)
        {
            TaskDialogOptions config = new();

            config.Owner = this;
            config.Title = "Fluent Store Installer";
            config.MainInstruction = "Install failed";
            config.Content = msg + "\r\n\r\n" +
                             "You may run the setup again at another time to complete the installation.";
            config.CommonButtons = TaskDialogCommonButtons.Close;
            config.MainIcon = VistaTaskDialogIcon.Error;

            TaskDialogResult res = TaskDialog.Show(config);
            if (res.Result == TaskDialogSimpleResult.Close)
                Cancel(false);
        }
    }
}
