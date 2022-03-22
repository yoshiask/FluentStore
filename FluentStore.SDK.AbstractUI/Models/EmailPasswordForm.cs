using OwlCore.AbstractUI.Models;
using System;

namespace FluentStore.SDK.AbstractUI.Models
{
    public class EmailPasswordForm : AbstractForm
    {
        public EmailPasswordForm(string id, EventHandler? onSubmit = null) : base(id, submitText: "Sign in", onSubmit: onSubmit)
        {
            // Create email and password boxes
            EmailBox = new(id + "_EmailBox", string.Empty, "Email");
            PasswordBox = new(id + "_PasswordBox", string.Empty, "Password");

            // Add all elements to collection
            Add(EmailBox);
            Add(PasswordBox);
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
        /// Gets the email entered into <see cref="EmailBox"/>.
        /// </summary>
        public string GetEmail() => EmailBox.Value;

        /// <summary>
        /// Gets the password entered into <see cref="PasswordBox"/>.
        /// </summary>
        public string GetPassword() => PasswordBox.Value;
    }
}
