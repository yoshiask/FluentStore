using FluentStore.SDK.AbstractUI.Models;
using FluentStore.Services;
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

        public static AbstractForm CreateSingleButtonForm(string formId, string formBody, string buttonText, EventHandler onClick)
        {
            AbstractForm form = new(formId, submitText: buttonText, canCancel: false, onSubmit: onClick);
            form.Add(new AbstractRichTextBlock("formBody", formBody));
            return form;
        }

        public static AbstractForm CreateOpenInBrowserForm(string formId, string formBody, Flurl.Url url, INavigationService navService)
        {
            return CreateSingleButtonForm(formId, formBody, "Open in browser",
                async (sender, e) => await navService.OpenInBrowser(url));
        }
    }
}
