#pragma once

winrt::Windows::Foundation::IAsyncAction MakeCertContextAsync(PCCERT_CONTEXT* context, winrt::Windows::Storage::IStorageFile const& signingCertFile, LPCWSTR password);

// from https://docs.microsoft.com/en-us/windows/win32/appxpkg/how-to-programmatically-sign-a-package
HRESULT SignAppxPackage(_In_ PCCERT_CONTEXT signingCertContext, _In_ LPCWSTR packageFilePath);
