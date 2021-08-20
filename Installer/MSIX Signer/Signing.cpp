#include "framework.h"
#include "SigningStructures.h"
#include "Signing.h"

using winrt::Windows::Foundation::IAsyncAction;
using namespace winrt::Windows::Storage;

IAsyncAction MakeCertContextAsync(PCCERT_CONTEXT* context, IStorageFile const& signingCertFile, LPCWSTR password)
{
	CRYPT_INTEGER_BLOB blob;
	auto buffer{ co_await FileIO::ReadBufferAsync(signingCertFile) };
	blob.pbData = buffer.data();
	blob.cbData = buffer.Length();
	auto store{ PFXImportCertStore(&blob, password, CRYPT_MACHINE_KEYSET) };
	*context = CertFindCertificateInStore(store, X509_ASN_ENCODING | PKCS_7_ASN_ENCODING, 0, CERT_FIND_ANY, nullptr, nullptr);
	CertCloseStore(store, 0);
}

// From https://docs.microsoft.com/en-us/windows/win32/appxpkg/how-to-programmatically-sign-a-package
HRESULT SignAppxPackage(_In_ PCCERT_CONTEXT signingCertContext, _In_ LPCWSTR packageFilePath)
{
	HRESULT hr = S_OK;

	// Initialize the parameters for SignerSignEx2
	DWORD signerIndex = 0;

	SIGNER_FILE_INFO fileInfo = {};
	fileInfo.cbSize = sizeof(SIGNER_FILE_INFO);
	fileInfo.pwszFileName = packageFilePath;

	SIGNER_SUBJECT_INFO subjectInfo = {};
	subjectInfo.cbSize = sizeof(SIGNER_SUBJECT_INFO);
	subjectInfo.pdwIndex = &signerIndex;
	subjectInfo.dwSubjectChoice = SIGNER_SUBJECT_FILE;
	subjectInfo.pSignerFileInfo = &fileInfo;

	SIGNER_CERT_STORE_INFO certStoreInfo = {};
	certStoreInfo.cbSize = sizeof(SIGNER_CERT_STORE_INFO);
	certStoreInfo.dwCertPolicy = SIGNER_CERT_POLICY_CHAIN_NO_ROOT;
	certStoreInfo.pSigningCert = signingCertContext;

	SIGNER_CERT cert = {};
	cert.cbSize = sizeof(SIGNER_CERT);
	cert.dwCertChoice = SIGNER_CERT_STORE;
	cert.pCertStoreInfo = &certStoreInfo;

	// The algidHash of the signature to be created must match the
	// hash algorithm used to create the app package
	SIGNER_SIGNATURE_INFO signatureInfo = {};
	signatureInfo.cbSize = sizeof(SIGNER_SIGNATURE_INFO);
	signatureInfo.algidHash = CALG_SHA_256;
	signatureInfo.dwAttrChoice = SIGNER_NO_ATTR;

	SIGNER_SIGN_EX2_PARAMS signerParams = {};
	signerParams.pSubjectInfo = &subjectInfo;
	signerParams.pSigningCert = &cert;
	signerParams.pSignatureInfo = &signatureInfo;

	APPX_SIP_CLIENT_DATA sipClientData = {};
	sipClientData.pSignerParams = &signerParams;
	signerParams.pSipData = &sipClientData;

	// Type definition for invoking SignerSignEx2 via GetProcAddress
	typedef HRESULT(WINAPI* SignerSignEx2Function)(
		DWORD,
		PSIGNER_SUBJECT_INFO,
		PSIGNER_CERT,
		PSIGNER_SIGNATURE_INFO,
		PSIGNER_PROVIDER_INFO,
		DWORD,
		PCSTR,
		PCWSTR,
		PCRYPT_ATTRIBUTES,
		PVOID,
		PSIGNER_CONTEXT*,
		PVOID,
		PVOID);

	// Load the SignerSignEx2 function from MSSign32.dll
	HMODULE msSignModule = LoadLibraryExW(
		L"MSSign32.dll",
		NULL,
		LOAD_LIBRARY_SEARCH_SYSTEM32);

	if (msSignModule)
	{
		SignerSignEx2Function SignerSignEx2 = reinterpret_cast<SignerSignEx2Function>(
			GetProcAddress(msSignModule, "SignerSignEx2"));
		if (SignerSignEx2)
		{
			hr = SignerSignEx2(
				signerParams.dwFlags,
				signerParams.pSubjectInfo,
				signerParams.pSigningCert,
				signerParams.pSignatureInfo,
				signerParams.pProviderInfo,
				signerParams.dwTimestampFlags,
				signerParams.pszAlgorithmOid,
				signerParams.pwszTimestampURL,
				signerParams.pCryptAttrs,
				signerParams.pSipData,
				signerParams.pSignerContext,
				signerParams.pCryptoPolicy,
				signerParams.pReserved);
		}
		else
		{
			DWORD lastError = GetLastError();
			hr = HRESULT_FROM_WIN32(lastError);
		}

		FreeLibrary(msSignModule);
	}
	else
	{
		DWORD lastError = GetLastError();
		hr = HRESULT_FROM_WIN32(lastError);
	}

	// Free any state used during app package signing
	if (sipClientData.pAppxSipState)
	{
		sipClientData.pAppxSipState->Release();
	}

	return hr;
}
