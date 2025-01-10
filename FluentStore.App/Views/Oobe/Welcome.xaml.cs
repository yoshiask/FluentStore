using FluentStore.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Oobe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Welcome : WizardPageBase
    {
        public Welcome(StartupWizardViewModel wizard) : base(wizard)
        {
            this.InitializeComponent();
        }
    }
}
