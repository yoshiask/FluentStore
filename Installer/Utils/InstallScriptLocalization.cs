using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Installer.Utils
{
    public static class InstallScriptLocalization
    {
        public static string GetLocalizedString(string key, CultureInfo culture = null)
        {
            if (culture == null)
                culture = CultureInfo.CurrentUICulture;

            if (TranslationTable == null)
                PopulateTranslationTable(culture);
            return TranslationTable![key];
        }

        private static void PopulateTranslationTable(CultureInfo culture)
        {
            DirectoryInfo cultureDir = new(Path.Combine(App.InstallerDir.FullName, "Add-AppDevPackage.resources", culture.ToString()));
            if (!cultureDir.Exists)
            {
                // If not translation exists for the current culture, use the default
                cultureDir = new(Path.Combine(App.InstallerDir.FullName, "Add-AppDevPackage.resources"));
            }

            string moduleText = File.ReadAllText(Path.Combine(cultureDir.FullName, "Add-AppDevPackage.psd1"));
            Regex rx = new(@"^\s*(?<key>\w+)\s*=\s*(?<value>.+)", RegexOptions.Multiline | RegexOptions.Compiled);
            TranslationTable = new Dictionary<string, string>();
            foreach (Match translation in rx.Matches(moduleText))
            {
                if (!translation.Success)
                    continue;
                string key = translation.Groups["key"].Value;
                string value = translation.Groups["value"].Value;

                // Remove carrige returns and new lines for easier matching
                value = value.Replace("\r", string.Empty)
                             .Replace("\\r", string.Empty)
                             .Replace("\n", string.Empty)
                             .Replace("\\n", string.Empty);

                TranslationTable.Add(key, value);
            }

            // Add special error translation
            string someErrorString = TranslationTable[KEY_ErrorAddPackageFailed];
            int idxColon = someErrorString.IndexOf(':');
            TranslationTable.Add(KEY_Error, someErrorString.Substring(0, idxColon));
        }

        private static Dictionary<string, string> TranslationTable { get; set; }

        #region Keys
        public const string KEY_Error = "Error";

        public const string KEY_PromptYesString = "PromptYesString";
        public const string KEY_PromptNoString = "PromptNoString";
        public const string KEY_BundleFound = "BundleFound";
        public const string KEY_PackageFound = "PackageFound";
        public const string KEY_EncryptedBundleFound = "EncryptedBundleFound";
        public const string KEY_EncryptedPackageFound = "EncryptedPackageFound";
        public const string KEY_CertificateFound = "CertificateFound";
        public const string KEY_DependenciesFound = "DependenciesFound";
        public const string KEY_GettingDeveloperLicense = "GettingDeveloperLicense";
        public const string KEY_InstallingCertificate = "InstallingCertificate";
        public const string KEY_InstallingPackage = "InstallingPackage";
        public const string KEY_AcquireLicenseSuccessful = "AcquireLicenseSuccessful";
        public const string KEY_InstallCertificateSuccessful = "InstallCertificateSuccessful";
        public const string KEY_Success = "Success";
        public const string KEY_WarningInstallCert = "WarningInstallCert";
        public const string KEY_ElevateActions = "ElevateActions";
        public const string KEY_ElevateActionDevLicense = "ElevateActionDevLicense";
        public const string KEY_ElevateActionCertificate = "ElevateActionCertificate";
        public const string KEY_ElevateActionsContinue = "ElevateActionsContinue";
        public const string KEY_ErrorForceElevate = "ErrorForceElevate";
        public const string KEY_ErrorForceDeveloperLicense = "ErrorForceDeveloperLicense";
        public const string KEY_ErrorLaunchAdminFailed = "ErrorLaunchAdminFailed";
        public const string KEY_ErrorNoScriptPath = "ErrorNoScriptPath";
        public const string KEY_ErrorNoPackageFound = "ErrorNoPackageFound";
        public const string KEY_ErrorManyPackagesFound = "ErrorManyPackagesFound";
        public const string KEY_ErrorPackageUnsigned = "ErrorPackageUnsigned";
        public const string KEY_ErrorNoCertificateFound = "ErrorNoCertificateFound";
        public const string KEY_ErrorManyCertificatesFound = "ErrorManyCertificatesFound";
        public const string KEY_ErrorBadCertificate = "ErrorBadCertificate";
        public const string KEY_ErrorExpiredCertificate = "ErrorExpiredCertificate";
        public const string KEY_ErrorCertificateMismatch = "ErrorCertificateMismatch";
        public const string KEY_ErrorCertIsCA = "ErrorCertIsCA";
        public const string KEY_ErrorBannedKeyUsage = "ErrorBannedKeyUsage";
        public const string KEY_ErrorBannedEKU = "ErrorBannedEKU";
        public const string KEY_ErrorNoBasicConstraints = "ErrorNoBasicConstraints";
        public const string KEY_ErrorNoCodeSigningEku = "ErrorNoCodeSigningEku";
        public const string KEY_ErrorInstallCertificateCancelled = "ErrorInstallCertificateCancelled";
        public const string KEY_ErrorCertUtilInstallFailed = "ErrorCertUtilInstallFailed";
        public const string KEY_ErrorGetDeveloperLicenseFailed = "ErrorGetDeveloperLicenseFailed";
        public const string KEY_ErrorInstallCertificateFailed = "ErrorInstallCertificateFailed";
        public const string KEY_ErrorAddPackageFailed = "ErrorAddPackageFailed";
        public const string KEY_ErrorAddPackageFailedWithCert = "ErrorAddPackageFailedWithCert";
        #endregion
    }
}
