
using StoreLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace StoreLib.Services
{
    public static class FE3Handler
    {
        private static readonly MSHttpClient _httpClient = new MSHttpClient();

        /// <summary>
        /// Returns raw xml containing various (Revision, Update, Package) IDs and info.
        /// </summary>
        /// <param name="WuCategoryID"></param>
        /// <returns></returns>
        public static async Task<string> SyncUpdatesAsync(string WuCategoryID)
        {
            HttpContent httpContent = new StringContent(String.Format(GetResourceTextFile("WUIDRequest.xml"), await GetCookieAsync(), WuCategoryID), Encoding.UTF8, "application/soap+xml"); //Load in the Xml for this FE3 request and format it a cookie and the provided WuCategoryID.
            HttpRequestMessage httpRequest = new HttpRequestMessage();
            httpRequest.RequestUri = Endpoints.FE3Delivery;
            httpRequest.Content = httpContent;
            httpRequest.Method = HttpMethod.Post;
            HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest,  new System.Threading.CancellationToken());
            string content = await httpResponse.Content.ReadAsStringAsync();
            content = WebUtility.HtmlDecode(content);
            return content;
        }

        public static async Task<IList<PackageInstance>> GetPackageInstancesAsync(string WuCategoryID)
        {
            IList<PackageInstance> PackageInstances = new List<PackageInstance>();
            HttpContent httpContent = new StringContent(String.Format(GetResourceTextFile("WUIDRequest.xml"), await GetCookieAsync(), WuCategoryID), Encoding.UTF8, "application/soap+xml"); //Load in the Xml for this FE3 request and format it a cookie and the provided WuCategoryID.
            HttpRequestMessage httpRequest = new HttpRequestMessage();
            httpRequest.RequestUri = Endpoints.FE3Delivery;
            httpRequest.Content = httpContent;
            httpRequest.Method = HttpMethod.Post;
            HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, new System.Threading.CancellationToken());
            string content = await httpResponse.Content.ReadAsStringAsync();
            content = WebUtility.HtmlDecode(content);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);
            XmlNodeList nodes = doc.GetElementsByTagName("AppxMetadata");
            XmlNodeList updateNodes = doc.GetElementsByTagName("File");
            foreach (XmlNode node in nodes)
            {
                if(node.Attributes.Count >= 3)
                {
                    string digest = updateNodes.Cast<XmlNode>().Where(x =>
                            x.Attributes.GetNamedItem("InstallerSpecificIdentifier") != null &&
                            x.Attributes.GetNamedItem("InstallerSpecificIdentifier").Value ==
                            node.Attributes.GetNamedItem("PackageMoniker").Value).FirstOrDefault().Attributes
                        .GetNamedItem("Digest").Value;
                    PackageInstance package = new PackageInstance(node.Attributes.GetNamedItem("PackageMoniker").Value, new Uri("http://test.com"), Utilities.TypeHelpers.StringToPackageType(node.Attributes.GetNamedItem("PackageType").Value), digest);
                    PackageInstances.Add(package);
                }
            }
            return PackageInstances;

        }

        /// <summary>
        /// Gets a FE3 Cookie, required for all FE3 requests.
        /// </summary>
        /// <returns>Cookie extracted from returned XML</returns>
        public static async Task<String> GetCookieAsync() //Encrypted Cookie Data is needed for FE3 requests. It doesn't expire for a very long time but I still refresh it as the Store does. 
        {
            XmlDocument doc = new XmlDocument();
            HttpContent httpContent = new StringContent(GetResourceTextFile("GetCookie.xml"), Encoding.UTF8, "application/soap+xml");//Loading the request xml from a file to keep things nice and tidy.
            HttpRequestMessage httpRequest = new HttpRequestMessage();
            httpRequest.RequestUri = Endpoints.FE3Delivery;
            httpRequest.Content = httpContent;
            httpRequest.Method = HttpMethod.Post;
            HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, new System.Threading.CancellationToken()); 
            doc.LoadXml(await httpResponse.Content.ReadAsStringAsync());
            XmlNodeList xmlNodeList = doc.GetElementsByTagName("EncryptedData");
            string cookie = xmlNodeList[0].InnerText;
            return cookie;
        }
        /// <summary>
        /// This function takes in the xml returned via SyncUpdatesAsync and parses out the Revision IDs, Package Names, and Update IDs. The resulting Update IDs and Revisions IDs are used for GetFileUrlsAsync.
        /// </summary>
        /// <param name="Xml"></param>
        /// <param name="RevisionIDs"></param>
        /// <param name="PackageNames"></param>
        /// <param name="UpdateIDs"></param>
        public static void ProcessUpdateIDs(string Xml, out IList<string> RevisionIDs, out IList<string> PackageNames, out IList<string> UpdateIDs)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Xml);
            UpdateIDs = new List<string>(); 
            PackageNames = new List<string>();
            RevisionIDs = new List<string>();
            XmlNodeList nodes = doc.GetElementsByTagName("SecuredFragment"); //We need to find updateIDs that actually have a File URL. Only nodes that have SecuredFragment will have an UpdateID that connects to a url. 
            foreach (XmlNode node in nodes)
            {
                string UpdateID = node.ParentNode.ParentNode.FirstChild.Attributes[0].Value; //Go from SecuredFragment to Properties to XML then down to the first child being UpdateIdentity. The first attribute is the UpdateID, second is the revisionID. 
                string RevisionID = node.ParentNode.ParentNode.FirstChild.Attributes[1].Value;
                UpdateIDs.Add(UpdateID);
                RevisionIDs.Add(RevisionID);
            }
        }

        public class UrlItem
        {
            public Uri url;
            public string digest;
        }

        /// <summary>
        /// Returns the Uris for the listing's packages. Each Uri will be for an appx or eappx. The blockmap is filtered out before returning the list.
        /// </summary>
        /// <param name="UpdateIDs"></param>
        /// <param name="RevisionIDs"></param>
        /// <returns>IList of App Package Download Uris</returns>
        public static async Task<IList<UrlItem>> GetFileUrlsAsync(IList<string> UpdateIDs, IList<string> RevisionIDs)
        {
            XmlDocument doc = new XmlDocument();
            IList<UrlItem> uris = new List<UrlItem>();
            foreach (string ID in UpdateIDs)
            {
                HttpContent httpContent = new StringContent(String.Format(GetResourceTextFile("FE3FileUrl.xml"), ID, RevisionIDs[UpdateIDs.IndexOf(ID)]), Encoding.UTF8, "application/soap+xml");//Loading the request xml from a file to keep things nice and tidy.
                HttpRequestMessage httpRequest = new HttpRequestMessage();
                httpRequest.RequestUri = Endpoints.FE3DeliverySecured;
                httpRequest.Content = httpContent;
                httpRequest.Method = HttpMethod.Post;
                HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, new System.Threading.CancellationToken()); 
                doc.LoadXml(await httpResponse.Content.ReadAsStringAsync());
                XmlNodeList XmlUrls = doc.GetElementsByTagName("FileLocation");
                foreach (XmlNode node in XmlUrls)
                {
                    if (node.ChildNodes.Cast<XmlNode>().Any(x => x.Name == "Url" && x.InnerText.Length != 99))
                    {
                        uris.Add(new UrlItem()
                        {
                            url = new Uri(node.ChildNodes.Cast<XmlNode>().First(x => x.Name == "Url").InnerText),
                            digest = node.ChildNodes.Cast<XmlNode>().First(x => x.Name == "FileDigest").InnerText
                        });
                    }
                }
            }
            return uris;
        }

        /// <summary>
        /// Internal function used to read premade xml. 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static string GetResourceTextFile(string filename)
        {
            string result = string.Empty;

            using (Stream stream = typeof(FE3Handler).GetTypeInfo().Assembly.
                       GetManifestResourceStream("StoreLib.Xml." + filename))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }

        /// <summary>
        /// This function takes in the xml returned via SyncUpdatesAsync and parses out the Revision IDs, Package Names, and Update IDs. The resulting Update IDs and Revisions IDs are used for GetFileUrlsAsync.
        /// </summary>
        /// <param name="Xml"></param>
        /// <param name="RevisionIDs"></param>
        /// <param name="PackageNames"></param>
        /// <param name="UpdateIDs"></param>
        /// <remarks>Identical to <see cref="ProcessUpdateIDs"/>, but only processes packages where
        /// <c>MainPackage="true"</c></remarks>
        public static void ProcessMainPackageUpdateIDs(string Xml, out IList<string> RevisionIDs, out IList<string> PackageNames, out IList<string> UpdateIDs)
        {
            XDocument doc = XDocument.Parse(Xml);
            UpdateIDs = new List<string>();
            PackageNames = new List<string>();
            RevisionIDs = new List<string>();

            // First find all AppxPackageInstallData elements that have MainPackage="true"
            IEnumerable<string> mainInstallIds = doc.Root.Descendants()
                .Where(e => e.Name.LocalName == "AppxPackageInstallData" && e.Attribute("MainPackage").Value == "true")
                .Select(n => n.Parent.Parent.Parent.Elements().First(e => e.Name.LocalName == "ID").Value);

            IEnumerable<XElement> updateInfos = doc.Root.Descendants()
                .Where(e => e.Name.LocalName == "UpdateInfo" && mainInstallIds.Contains(e.Elements().First(e => e.Name.LocalName == "ID").Value));
            foreach (XElement updateInfo in updateInfos)
            {
                // These UpdateInfo elements are linked to a main AppxPackageInstallData element
                XElement updateIdentity = updateInfo.Elements().First(e => e.Name.LocalName == "Xml")
                    .Elements().First(e => e.Name.LocalName == "UpdateIdentity");
                string updateID = updateIdentity.Attribute("UpdateID").Value;
                string revisionID = updateIdentity.Attribute("RevisionNumber").Value;
                UpdateIDs.Add(updateID);
                RevisionIDs.Add(revisionID);
            }
        }
    }

}
