using OwlCore.AbstractUI.Models;
using System;

namespace FluentStore.SDK.AbstractUI
{
    public class EmailPasswordForm : AbstractUICollection
    {
        private EventHandler? _onSubmit;

        public EmailPasswordForm(string id, EventHandler? onSubmit = null) : base(id)
        {
            EmailBox = new(id + "_EmailBox", string.Empty, "Email");
            PasswordBox = new(id + "_PasswordBox", string.Empty, "Password");
            SubmitButton = new(id + "_SubmitButton", "Submit", type: AbstractButtonType.Confirm);

            // Subscribe to submit button
            SubmitButton.Clicked += OnSubmitButtonClicked;
            _onSubmit = onSubmit;

            // Add all elements to collection
            Add(EmailBox);
            Add(PasswordBox);
            Add(SubmitButton);
        }

        /// <summary>
        /// The <see cref="AbstractUIElement"/> representing the text box the user
        /// enters their email or username into.
        /// </summary>
        public AbstractTextBox EmailBox { get; }

        /// <summary>
        /// The <see cref="AbstractUIElement"/> representing the text box the user
        /// enters their password into.
        /// </summary>
        public AbstractTextBox PasswordBox { get; }

        /// <summary>
        /// The <see cref="AbstractUIElement"/> representing the button the user
        /// presses to submit the form.
        /// </summary>
        public AbstractButton SubmitButton { get; }

        /// <summary>
        /// Gets the email entered into <see cref="EmailBox"/>.
        /// </summary>
        public string GetEmail() => EmailBox.Value;

        /// <summary>
        /// Gets the password entered into <see cref="PasswordBox"/>.
        /// </summary>
        public string GetPassword() => PasswordBox.Value;

        public event EventHandler? OnSubmit;

        private void OnSubmitButtonClicked(object sender, EventArgs e)
        {
            // Route the submit button event to this form's event,
            // so the event handler has a reference to this form
            // instead of just the button.
            _onSubmit?.Invoke(this, e);
        }
    }
}
