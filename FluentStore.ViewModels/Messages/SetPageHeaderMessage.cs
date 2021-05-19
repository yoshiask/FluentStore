using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace FluentStore.ViewModels.Messages
{
    public class SetPageHeaderMessage : ValueChangedMessage<string>
    {
        public SetPageHeaderMessage(string value) : base(value)
        {

        }
    }
}
