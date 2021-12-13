using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentStore.SDK.Models
{
    public class Review : ObservableObject
    {
        private string _ReviewerName;
        public string ReviewerName
        {
            get => _ReviewerName;
            set => SetProperty(ref _ReviewerName, value);
        }

        private int _Rating;
        public int Rating
        {
            get => _Rating;
            set => SetProperty(ref _Rating, value);
        }

        private string _Market;
        public string Market
        {
            get => _Market;
            set => SetProperty(ref _Market, value);
        }

        private string _Locale;
        public string Locale
        {
            get => _Locale;
            set => SetProperty(ref _Locale, value);
        }

        private int _HelpfulPositive;
        public int HelpfulPositive
        {
            get => _HelpfulPositive;
            set => SetProperty(ref _HelpfulPositive, value);
        }

        private int _HelpfulNegative;
        public int HelpfulNegative
        {
            get => _HelpfulNegative;
            set => SetProperty(ref _HelpfulNegative, value);
        }

        private string _ReviewId;
        public string ReviewId
        {
            get => _ReviewId;
            set => SetProperty(ref _ReviewId, value);
        }

        private string _ReviewText;
        public string ReviewText
        {
            get => _ReviewText;
            set => SetProperty(ref _ReviewText, value);
        }

        private string _Title;
        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        private DateTimeOffset _SubmittedDateTimeUtc;
        public DateTimeOffset SubmittedDateTimeUtc
        {
            get => _SubmittedDateTimeUtc;
            set => SetProperty(ref _SubmittedDateTimeUtc, value);
        }

        private bool _IsRevised;
        public bool IsRevised
        {
            get => _IsRevised;
            set => SetProperty(ref _IsRevised, value);
        }

        private bool _UpdatedSinceResponse;
        public bool UpdatedSinceResponse
        {
            get => _UpdatedSinceResponse;
            set => SetProperty(ref _UpdatedSinceResponse, value);
        }
    }
}
