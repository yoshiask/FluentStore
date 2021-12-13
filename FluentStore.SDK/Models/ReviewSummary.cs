using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace FluentStore.SDK.Models
{
    public class ReviewSummary : ObservableObject
    {
        private double _AverageRating = -1;
        public double AverageRating
        {
            get => _AverageRating;
            set
            {
                SetProperty(ref _AverageRating, value);
                HasAverageRating = AverageRating >= 0;
            }
        }

        private bool _HasAverageRating;
        public bool HasAverageRating
        {
            get => _HasAverageRating;
            set => SetProperty(ref _HasAverageRating, value);
        }

        private int _ReviewCount = -1;
        public int ReviewCount
        {
            get => _ReviewCount;
            set
            {
                SetProperty(ref _ReviewCount, value);
                HasReviewCount = ReviewCount >= 0;
            }
        }

        private bool _HasReviewCount;
        public bool HasReviewCount
        {
            get => _HasReviewCount;
            set => SetProperty(ref _HasReviewCount, value);
        }
        
        private List<Review> _Reviews;
        public List<Review> Reviews
        {
            get => _Reviews;
            set
            {
                HasReviews = Reviews != null && Reviews.Count > 0;
                SetProperty(ref _Reviews, value);
            }
        }

        private bool _HasReviews;
        public bool HasReviews
        {
            get => _HasReviews;
            set => SetProperty(ref _HasReviews, value);
        }

        private int _Star1Count;
        public int Star1Count
        {
            get => _Star1Count;
            set => SetProperty(ref _Star1Count, value);
        }

        private int _Star2Count;
        public int Star2Count
        {
            get => _Star2Count;
            set => SetProperty(ref _Star2Count, value);
        }

        private int _Star3Count;
        public int Star3Count
        {
            get => _Star3Count;
            set => SetProperty(ref _Star3Count, value);
        }

        private int _Star4Count;
        public int Star4Count
        {
            get => _Star4Count;
            set => SetProperty(ref _Star4Count, value);
        }

        private int _Star5Count;
        public int Star5Count
        {
            get => _Star5Count;
            set => SetProperty(ref _Star5Count, value);
        }

        private int _Star1ReviewCount;
        public int Star1ReviewCount
        {
            get => _Star1ReviewCount;
            set => SetProperty(ref _Star1ReviewCount, value);
        }

        private int _Star2ReviewCount;
        public int Star2ReviewCount
        {
            get => _Star2ReviewCount;
            set => SetProperty(ref _Star2ReviewCount, value);
        }

        private int _Star3ReviewCount;
        public int Star3ReviewCount
        {
            get => _Star3ReviewCount;
            set => SetProperty(ref _Star3ReviewCount, value);
        }

        private int _Star4ReviewCount;
        public int Star4ReviewCount
        {
            get => _Star4ReviewCount;
            set => SetProperty(ref _Star4ReviewCount, value);
        }

        private int _Star5ReviewCount;
        public int Star5ReviewCount
        {
            get => _Star5ReviewCount;
            set => SetProperty(ref _Star5ReviewCount, value);
        }
    }
}
