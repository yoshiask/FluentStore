using FluentStore.SDK.AbstractUI.Models;
using Microsoft.Toolkit.Mvvm.Input;
using OwlCore.AbstractUI.ViewModels;
using System;

namespace FluentStore.SDK.AbstractUI.ViewModels
{
    /// <summary>
    /// A ViewModel wrapper for an <see cref="AbstractForm"/>.
    /// </summary>
    public class AbstractFormViewModel : AbstractUIViewModelBase
    {
        private readonly AbstractForm _model;

        /// <summary>
        /// Initializes a new instance of see <see cref="AbstractFormViewModel"/>.
        /// </summary>
        /// <param name="model">The model to wrap around.</param>
        public AbstractFormViewModel(AbstractForm model)
            : base(model)
        {
            _model = model;

            SubmitCommand = new RelayCommand(model.Submit);
            CancelCommand = new RelayCommand(model.Cancel);
        }

        /// <inheritdoc cref="AbstractForm.CanCancel"/>
        public bool CanCancel
        {
            get => _model.CanCancel;
            set => SetProperty(_model.CanCancel, value, _model, (u, n) => _model.CanCancel = n);
        }

        /// <inheritdoc cref="AbstractForm.SubmitText"/>
        public string SubmitText
        {
            get => _model.SubmitText;
            set => SetProperty(_model.SubmitText, value, _model, (u, n) => _model.SubmitText = n);
        }

        /// <inheritdoc cref="AbstractForm.CancelText"/>
        public string CancelText
        {
            get => _model.CancelText;
            set => SetProperty(_model.CancelText, value, _model, (u, n) => _model.CancelText = n);
        }

        /// <inheritdoc cref="AbstractForm.Submitted"/>
        public event EventHandler Submitted
        {
            add => _model.Submitted += value;
            remove => _model.Submitted -= value;
        }

        /// <summary>
        /// Command for <see cref="AbstractForm.Submit"/>.
        /// </summary>
        public IRelayCommand SubmitCommand;

        /// <inheritdoc cref="AbstractForm.Cancelled"/>
        public event EventHandler Cancelled
        {
            add => _model.Cancelled += value;
            remove => _model.Cancelled -= value;
        }

        /// <summary>
        /// Command for <see cref="AbstractForm.Cancel"/>.
        /// </summary>
        public IRelayCommand CancelCommand;
    }
}
