using CommunityToolkit.Diagnostics;
using System;

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

        public override string ToString() => $"[{TextContent}]({Uri})";

        /// <summary>
        /// Creates an instance of <see cref="Link"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Link"/> object, as if the parameters were passed
        /// to <see cref="Link(Uri, string)"/>, or <c>null</c> if <paramref name="uri"/>
        /// is <c>null</c>.
        /// </returns>
        public static Link Create(Uri uri, string textContent = null)
        {
            if (uri == null)
                return null;
            return new(uri, textContent);
        }

        /// <summary>
        /// Creates an instance of <see cref="Link"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Link"/> object, as if the parameters were passed
        /// to <see cref="Link(string, string)"/>, or <c>null</c> if <paramref name="uri"/>
        /// is <c>null</c>.
        /// </returns>
        public static Link Create(string url, string textContent = null)
        {
            try
            {
                Uri uri = new(url);
                return new(uri, textContent);
            }
            catch
            {
                return null;
            }
        }
    }
}
