using FluentStore.SDK.AbstractUI.Models;
using FluentStore.Services;
using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Gets the first child with the specified ID.
        /// </summary>
        /// <returns>
        /// The first child with a matching ID. <see langword="null"/> if
        /// no child matched.
        /// </returns>
        public static AbstractUIElement? GetChildById(this ICollection<AbstractUIElement> col, string id)
        {
            foreach (AbstractUIElement element in col)
                if (element.Id == id)
                    return element;

            return null;
        }

        /// <summary>
        /// Gets the first child of the specified type with the specified ID.
        /// </summary>
        /// <returns>
        /// The first child with a matching ID and type. <see langword="null"/> if
        /// no child matched.
        /// </returns>
        public static TElem? GetChildById<TElem>(this ICollection<AbstractUIElement> col, string id) where TElem : AbstractUIElement
        {
            foreach (AbstractUIElement element in col)
                if (element.Id == id && element is TElem matched)
                    return matched;

            return null;
        }
    }
}
