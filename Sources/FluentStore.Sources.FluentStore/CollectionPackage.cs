using FluentStore.SDK.Images;
using FluentStoreAPI.Models;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentStore.SDK.Packages;
using FluentStore.SDK;
using FluentStore.SDK.AbstractUI;
using FluentStore.SDK.AbstractUI.Models;
using OwlCore.AbstractUI.Models;

namespace FluentStore.Sources.FluentStore
{
    public class CollectionPackage : GenericPackageCollection<Collection>
    {
        public CollectionPackage(PackageHandlerBase packageHandler, Collection collection = null, IEnumerable<PackageBase> items = null)
            : base(packageHandler)
        {
            if (collection != null)
                Update(collection);
            if (items != null)
                Update(items);
        }

        public void Update(Collection collection)
        {
            Guard.IsNotNull(collection, nameof(collection));
            Model = collection;

            // Set base properties
            Title = collection.Name;
            PublisherId = collection.AuthorId;
            Urn = Urn.Parse($"urn:{FluentStoreHandler.NAMESPACE_COLLECTION}:{PublisherId}:{Model.Id}");
            //ReleaseDate = collection.LastUpdateDateUtc;
            Description = collection.Description;
            ShortTitle = Title;

            // Determine if collection can be edited
            if (Handler.AccSvc.TryGetAuthenticatedHandler<Users.FluentStoreAccountHandler>(out var accHandler))
            {
                CanEdit = accHandler.CurrentUser.Id == collection.AuthorId;
            }
        }

        public void Update(IEnumerable<PackageBase> items)
        {
            Guard.IsNotNull(items, nameof(items));

            foreach (PackageBase package in items)
                Items.Add(package);
        }

        public void Update(Profile author)
        {
            Guard.IsNotNull(author, nameof(author));

            // Set base properties
            DeveloperName = author.DisplayName;
        }

        public override async Task<ImageBase> CacheAppIcon()
        {
            ImageBase image;
            if (string.IsNullOrEmpty(Model.ImageUrl))
            {
                if (!string.IsNullOrEmpty(Model.TileGlyph))
                {
                    image = new TextImage
                    {
                        Text = Model.TileGlyph,
                        ImageType = ImageType.Tile,
                    };
                }
                else
                {
                    image = TextImage.CreateFromName(Title, ImageType.Tile);
                }
            }
            else
            {
                image = new FileImage
                {
                    Url = Model.ImageUrl,
                    ImageType = ImageType.Logo,
                };
            }

            return image;
        }

        public override Task<ImageBase> CacheHeroImage() => Task.FromResult<ImageBase>(null);

        public override Task<List<ImageBase>> CacheScreenshots() => Task.FromResult(new List<ImageBase>(0));

        public override AbstractForm CreateEditForm()
        {
            AbstractForm form = new($"{Urn}_EditForm", submitText: "Save", onSubmit: OnEditFormSubmit)
            {
                new AbstractTextBox("NameBox", string.Empty, "Name")
                {
                    TooltipText = "The name of the collection"
                },
                new AbstractTextBox("ImageUrlBox", string.Empty, "Icon URL")
                {
                    TooltipText = "A link to an image to be used as an icon"
                },
                new AbstractTextBox("TileGlyphBox", string.Empty, "Icon text")
                {
                    TooltipText = "Text to be shown in place of an icon if no image is provided"
                },
                new AbstractTextBox("DescriptionBox", string.Empty, "Description")
                {
                    TooltipText = "A description of the collection"
                },
                new AbstractBoolean("IsPublicSwitch", "Public?")
                {
                    TooltipText = "Determines if this collection is visible to others"
                },
            };
            form.Title = "Editing " + ShortTitle;

            return form;
        }

        private void OnEditFormSubmit(object sender, System.EventArgs args)
        {
            if (sender is not AbstractForm form)
                return;

            var nameBox = form.GetElement<AbstractTextBox>("NameBox");
            var imageUrlBox = form.GetElement<AbstractTextBox>("ImageUrlBox");
            var typeGlyphBox = form.GetElement<AbstractTextBox>("TileGlyphBox");
            var descriptionBox = form.GetElement<AbstractTextBox>("DescriptionBox");
            var isPublicSwitch = form.GetElement<AbstractBoolean>("IsPublicSwitch");


        }
    }
}
