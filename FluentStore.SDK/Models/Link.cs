using CommunityToolkit.Diagnostics;
using System;
using System.Diagnostics.Contracts;

namespace FluentStore.SDK.Models
{
    public class Link
    {
        /// <remarks>
        /// Consider using <see cref="Create(string, string)"/> for safety.
        /// </remarks>
        public Link(string uriString, string textContent = null) : this(new Uri(uriString), textContent)
        {
        }

        /// <remarks>
        /// Consider using <see cref="Create(Uri, string)"/> for safety.
        /// </remarks>
        public Link(Uri uri, string textContent = null)
        {
            Guard.IsNotNull(uri, nameof(uri));

            Uri = uri;
            TextContent = textContent;
        }

        /// <summary>
        /// The <see cref="Uri"/> this link represents.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Text to be displayed.
        /// </summary>
        private string _TextContent;
        public string TextContent
        {
            get => _TextContent ?? Uri.ToString();
            set => _TextContent = value;
        }

        [Pure]
        public override string ToString() => $"[{TextContent}]({Uri})";

        /// <summary>
        /// Creates an instance of <see cref="Link"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Link"/> object, as if the parameters were passed
        /// to <see cref="Link(Uri, string)"/>, or <c>null</c> if <paramref name="uri"/>
        /// is <c>null</c>.
        /// </returns>
        [Pure]
        public static Link Create(Uri uri, string textContent = null)
            => uri is null ? null : new(uri, textContent);

        /// <summary>
        /// Creates an instance of <see cref="Link"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Link"/> object, as if the parameters were passed
        /// to <see cref="Link(string, string)"/>, or <c>null</c> if <paramref name="uri"/>
        /// is <c>null</c>.
        /// </returns>
        [Pure]
        public static Link Create(string url, string textContent = null)
        {
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
                return new(uri, textContent);
            return null;
        }

        public static implicit operator Flurl.Url(Link link) => link.Uri;
        public static implicit operator Uri(Link link) => link.Uri;
    }
}
