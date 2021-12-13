using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FluentStore.ViewModels.Messages
{
    public class PageLoadingMessage : ValueChangedMessage<bool>
    {
        public PageLoadingMessage(bool value) : base(value)
        {

        }
    }
}
