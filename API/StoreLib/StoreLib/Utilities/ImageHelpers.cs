using StoreLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace StoreLib.Utilities
{
    public static class ImageHelpers
    {
        /// <summary>
        /// Returns the uri for the proper image format requested. If the requested format isn't found, we sliently return nothing.
        /// </summary>
        /// <param name="WantedImageType"></param>
        /// <param name="displayCatalogModel"></param>
        /// <returns></returns>
        public static Uri GetImageUri(ImagePurpose WantedImageType, DisplayCatalogModel displayCatalogModel)
        {
            foreach (StoreLib.Models.Image image in displayCatalogModel.Product.LocalizedProperties[0].Images)
            {
                Uri imageUri;
                if (image.ImagePurpose == Enum.GetName(typeof(ImagePurpose), WantedImageType))
                {
                    if (image.Uri.StartsWith("//")) //For whatever reason, uris for images from UWP listings will start with "//", i.e //store-images.s-microsoft.com. Checking for that and adding http: to the beginning if they do to create a valid url. 
                    {
                        imageUri = new Uri("http:" + image.Uri);
                        return imageUri;
                    }
                    imageUri = new Uri(image.Uri);
                    return imageUri;
                }
            }
            return null;
        }

        /// <summary>
        /// A caching function for images, the passed InmageUri is hashed and used to name local images. If this is the first time requesting a specific image, we fetch it from DCat and store it locally. Otherwise we return the local image saving a web request. 
        /// </summary>
        /// <param name="ImageUri">Uri to Image</param>
        /// <param name="CachePath">Local caching path, this isn't hard coded for extendablity reasons</param>
        /// <param name="OverwriteCache">Should the local image be overwritten?</param>
        /// <returns></returns>
        public static async Task<byte[]> CacheImageAsync(Uri ImageUri, string CachePath, bool OverwriteCache) //.Net Standard appears to have no standard image object (like bitmapimage) so the easiest thing to do, is to return the image as a byte[] and have the using app convert it to the required type.
        {
            string hasheduri = HashHelpers.GetHashString(ImageUri.ToString()); //Each uri will be hashed, the hashed output will be used as the filename for the local cached image. This is to prevent any collision. 
            if (OverwriteCache | !File.Exists($"{CachePath}\\{hasheduri}"))
            {
                HttpClient ImageClient = new HttpClient();
                using(var response = await ImageClient.GetAsync(ImageUri))
                {
                    response.EnsureSuccessStatusCode();
                    byte[] image = await response.Content.ReadAsByteArrayAsync();
                    FileStream fs = new FileStream($"{CachePath}\\{hasheduri}", FileMode.Create);
                    await fs.WriteAsync(image, 0, image.Length);
                    return image;
                }
            }
            else
            {
                FileStream fs = new FileStream($"{CachePath}\\{hasheduri}", FileMode.Open);
                byte[] cachedimage = new byte[fs.Length];
                await fs.ReadAsync(cachedimage, 0, cachedimage.Length);
                return cachedimage;
            }
        }

    }
}
