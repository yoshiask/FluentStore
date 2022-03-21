using OwlCore.AbstractUI.Models;
using System;

namespace FluentStore.SDK.AbstractUI
{
    public static class AbstractUIHelper
    {
        public static AbstractUICollection CreateSingleButtonUI(string collectionId, string buttonId, string buttonText, string buttonIconCode, EventHandler onClick)
        {
            AbstractButton button = new(buttonId, buttonText, iconCode: buttonIconCode, type: AbstractButtonType.Confirm);
            button.Clicked += onClick;

            AbstractUICollection ui = new(collectionId)
            {
                button,
            };
            return ui;
        }
    }
}
