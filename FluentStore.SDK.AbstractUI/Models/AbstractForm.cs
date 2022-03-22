using OwlCore.AbstractUI.Models;
using OwlCore.Remoting;
using System;

namespace FluentStore.SDK.AbstractUI.Models
{
    /// <summary>
    /// Represents a collection of UI elements wrapped in a form with submit and cancel buttons.
    /// </summary>
    public class AbstractForm : AbstractUICollection
    {
        public AbstractForm(string id, string submitText = "Submit", string cancelText = "Cancel", bool canCancel = true,
            EventHandler? onSubmit = null, EventHandler? onCancel = null)
            : base(id, PreferredOrientation.Vertical)
        {
            // Set properties
            CanCancel = canCancel;
            SubmitText = submitText;
            CancelText = cancelText;

            // Subscribe to form events
            if (onSubmit != null)
                Submitted += onSubmit;
            if (onCancel != null)
                Cancelled += onCancel;
        }

        /// <summary>
        /// Gets or sets whether this form can be cancelled.
        /// </summary>
        public bool CanCancel { get; set; }

        /// <summary>
        /// Gets or sets the text to be displayed on the submit button.
        /// </summary>
        public string SubmitText { get; set; }

        /// <summary>
        /// Gets or sets the text to be displayed on the cancel button.
        /// </summary>
        public string CancelText { get; set; }

        /// <summary>
        /// Event that fires when the form is submitted.
        /// </summary>
        public event EventHandler? Submitted;

        /// <summary>
        /// Event that fires when the form is cancelled.
        /// </summary>
        public event EventHandler? Cancelled;

        /// <summary>
        /// Simulates the user submitting the form.
        /// </summary>
        [RemoteMethod]
        public void Submit() => Submitted?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Simulates the user cancelling the form.
        /// </summary>
        [RemoteMethod]
        public void Cancel() => Cancelled?.Invoke(this, EventArgs.Empty);
    }
}
