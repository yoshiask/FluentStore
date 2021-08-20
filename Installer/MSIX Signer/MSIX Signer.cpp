#include "../Package.h"

#include "framework.h"
#include "MSIX Signer.h"
#include "Signing.h"

using namespace std::chrono_literals;
using ATL::CComPtr;
using winrt::operator co_await;
using winrt::Windows::Foundation::Uri;
using winrt::Windows::Foundation::IAsyncAction;
using winrt::Windows::Foundation::IAsyncOperation;
using namespace winrt::Windows::Storage;
using namespace winrt::Windows::Management::Deployment;

TASKDIALOGCONFIG pageOne;
TASKDIALOGCONFIG pageTwo;
TASKDIALOGCONFIG pageThree;
TASKDIALOGCONFIG pageFour;
TASKDIALOGCONFIG pageFive;
TASKDIALOGCONFIG errorPage;

constexpr auto DISCLAIMER_PAGE_UNDERSTAND_BUTTON = 100;
constexpr auto DISCLAIMER_PAGE_CANCEL_BUTTON = 101;
constexpr auto AGREEMENT_PAGE_AGREE_BUTTON = 102;
constexpr auto AGREEMENT_PAGE_DISAGREE_BUTTON = 103;
constexpr auto SIGNING_PAGE_UNDERSTAND_BUTTON = 104;
constexpr auto SIGNING_PAGE_CANCEL_BUTTON = 105;
constexpr auto INSTALLING_PAGE_CANCEL_BUTTON = 106;
constexpr auto DONE_PAGE_LAUNCH_BUTTON = 107;
constexpr auto DONE_PAGE_CLOSE_BUTTON = 108;
constexpr auto ERROR_PAGE_TRY_AGAIN_BUTTON = 109;
constexpr auto ERROR_PAGE_CLOSE_BUTTON = 110;

HINSTANCE _hInst;
std::wstringstream errorMessageStream;
int _page = 0;

void SetTDText(HWND hWnd, std::wstring_view text)
{
	SendMessageW(hWnd, TDM_SET_ELEMENT_TEXT, TDE_CONTENT, (LPARAM)text.data());
}

void SetTDMainInstruction(HWND hWnd, LPCWSTR text)
{
	SendMessageW(hWnd, TDM_SET_ELEMENT_TEXT, TDE_MAIN_INSTRUCTION, (LPARAM)text);
}

void TDErrorOut(HWND hWnd, std::wstring_view text, std::wstring_view functionName)
{
	// Error formatting from https://docs.microsoft.com/en-us/windows/win32/debug/retrieving-the-last-error-code

	LPWSTR errorMessageBuffer{ nullptr };

	errorMessageStream
		<< APP_NAME L" could not install because "
		<< text;

	if (functionName.size() != 0)
	{
		auto dw{ GetLastError() };
		FormatMessageW(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, NULL, dw, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPTSTR)&errorMessageBuffer, 0, NULL);

		errorMessageStream
			<< L"\n\n"
			<< functionName
			<< L" failed with error "
			<< dw
			<< L": "
			<< errorMessageBuffer;

		if (errorMessageBuffer != nullptr)
		{
			LocalFree(errorMessageBuffer);
		}
	}

	_page = -1;
	SendMessageW(hWnd, TDM_NAVIGATE_PAGE, 0, (LPARAM)&errorPage);
}

IAsyncOperation<IStorageFile> ExportResourceAsync(WORD resource, std::wstring_view type, StorageFolder const& folderPath, std::wstring_view file)
{
	// From https://stackoverflow.com/a/18928098
	auto hRes{ FindResourceW(nullptr, MAKEINTRESOURCEW(resource), type.data()) };
	if (hRes == nullptr)
	{
		return nullptr;
	}

	auto hFileResource{ LoadResource(nullptr, hRes) };
	if (hFileResource == nullptr)
	{
		return nullptr;
	}

	auto lpFile{ (const uint8_t*)LockResource(hFileResource) };
	if (lpFile == nullptr)
	{
		return nullptr;
	}

	auto dwSize{ SizeofResource(nullptr, hRes) };
	if (dwSize == 0)
	{
		return nullptr;
	}

	auto f{ co_await folderPath.CreateFileAsync(file, CreationCollisionOption::ReplaceExisting) };
	co_await FileIO::WriteBytesAsync(f, { lpFile, lpFile + dwSize });

	co_return f;
}

IAsyncAction WaitForDoneFileAsync(StorageFolder const& folder, std::wstring_view path)
{
	do
	{
		co_await 1s;
		auto doneFile{ co_await folder.TryGetItemAsync(path) };
		if (doneFile != nullptr
			&& doneFile.IsOfType(StorageItemTypes::File)
			&& co_await FileIO::ReadTextAsync(doneFile.as<IStorageFile>()) == L"Done")
		{
			co_await doneFile.DeleteAsync();
			break;
		}
	} while (true);
}

IAsyncAction WriteDoneFileAsync(StorageFolder const& folder, std::wstring_view path)
{
	auto file{ co_await folder.CreateFileAsync(path) };
	co_await FileIO::WriteTextAsync(file, L"Done");
}

winrt::Windows::Foundation::IAsyncAction InstallAsync(HWND hWnd)
{
	USHORT unusedProcessMachine;
	USHORT nativeMachine;
	if (!IsWow64Process2(GetCurrentProcess(), &unusedProcessMachine, &nativeMachine))
	{
		TDErrorOut(hWnd, L"the system architecture could not be determined.", L"IsWow64Process2");
		co_return;
	}

	std::array<WORD, 4> packageResources;

	switch (nativeMachine)
	{
	case IMAGE_FILE_MACHINE_AMD64:
	{
		packageResources[0] = IDR_PACKAGE_X64;
		packageResources[1] = IDR_VCLIBS_X64;
		packageResources[2] = IDR_DOTNET_NATIVE_RUNTIME_X64;
		packageResources[3] = IDR_DOTNET_NATIVE_FRAMEWORK_X64;
	}
	break;

	case IMAGE_FILE_MACHINE_I386:
	{
		packageResources[0] = IDR_PACKAGE_X86;
		packageResources[1] = IDR_VCLIBS_X86;
		packageResources[2] = IDR_DOTNET_NATIVE_RUNTIME_X86;
		packageResources[3] = IDR_DOTNET_NATIVE_FRAMEWORK_X86;
	}
	break;

	case IMAGE_FILE_MACHINE_ARM64:
	{
		packageResources[0] = IDR_PACKAGE_ARM64;
		packageResources[1] = IDR_VCLIBS_ARM64;
		packageResources[2] = IDR_DOTNET_NATIVE_RUNTIME_ARM64;
		packageResources[3] = IDR_DOTNET_NATIVE_FRAMEWORK_ARM64;
	}
	break;

	default:
	{
		// From https://stackoverflow.com/a/5100745
		std::wstringstream stream;
		stream
			<< L"this system does not have a compatible architecture. It was reported as <A HREF=\"https://docs.microsoft.com/en-us/windows/win32/sysinfo/image-file-machine-constants\">0x"
			<< std::setfill(L'0') << std::setw(sizeof nativeMachine * 2) << std::hex << nativeMachine
			<< L"</A>.";

		TDErrorOut(hWnd, stream.str(), L"");
		co_return;
	}
	}

	WCHAR folderPath[MAX_PATH];
	if (GetTempPathW(MAX_PATH, folderPath) == 0)
	{
		TDErrorOut(hWnd, L"the TEMP folder could not be found.", L"GetTempPathW");
		co_return;
	}
	auto folder{ co_await StorageFolder::GetFolderFromPathAsync(folderPath) };

	SetTDText(hWnd, L"Generating and installing new self-signed certificate\u2026");
	std::wstringstream executeStream;
	executeStream
		<< L"\""
		L"$tp = (New-SelfSignedCertificate -Type Custom -Subject '" CERTIFICATE_PUBLISHER "' -KeyUsage DigitalSignature -FriendlyName '" APP_NAME " Self-Signing Certificate' -CertStoreLocation 'Cert:\\CurrentUser\\My' -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.3', '2.5.29.19={text}')).Thumbprint;"
		L"$password = ConvertTo-SecureString -String 'Unused' -Force -AsPlainText;"
		L"Export-PfxCertificate -Cert ('Cert:\\CurrentUser\\My\\' + $tp) -FilePath '" << folderPath << APP_NAME ".pfx' -Password $password;"
		L"Get-ChildItem ('Cert:\\CurrentUser\\My\\' + $tp) | Remove-Item;"
		L"Import-PfxCertificate -FilePath '" << folderPath << APP_NAME ".pfx' -CertStoreLocation Cert:\\LocalMachine\\TrustedPeople -Password $password;"
		L"'Done' | Out-File '" << folderPath << APP_NAME " Done 1.txt' -NoNewline;"
		L"while (!((Test-Path '" << folderPath << APP_NAME " Done 2.txt') -and ((Get-Content '" << folderPath << APP_NAME " Done 2.txt') -ceq 'Done'))) { Start-Sleep 2; }"
		L"Remove-Item '" << folderPath << APP_NAME " Done 2.txt';"
		L"Get-ChildItem Cert:\\LocalMachine\\TrustedPeople | Where-Object {$_.FriendlyName -eq '" APP_NAME " Self-Signing Certificate' -and $_.Subject -eq '" CERTIFICATE_PUBLISHER "'} | Remove-Item;"
		L"'Done' | Out-File '" << folderPath << APP_NAME " Done 3.txt' -NoNewline;"
		L"\"";
	if (reinterpret_cast<INT_PTR>(ShellExecuteW(nullptr, L"runas", L"powershell.exe", executeStream.str().c_str(), folderPath, SW_HIDE)) <= 32)
	{
		TDErrorOut(hWnd, L"the installer needs administrator permission. Try again and give permission to make changes to your PC.", L"ShellExecuteW");
		co_return;
	}
	SendMessageW(hWnd, TDM_SET_PROGRESS_BAR_MARQUEE, TRUE, 0);
	co_await WaitForDoneFileAsync(folder, APP_NAME L" Done 1.txt");

	SetTDText(hWnd, L"Reading self-signed certificate\u2026");
	PCCERT_CONTEXT context;
	auto certificateFile{ co_await folder.GetFileAsync(APP_NAME L".pfx") };
	co_await MakeCertContextAsync(&context, certificateFile, L"Unused");

	std::array<IStorageFile, packageResources.size()> packages;
	auto dependencies{ winrt::single_threaded_vector<Uri>() };
	for (size_t i = 0; i < packageResources.size(); i++)
	{
		std::wstringstream progressStream;
		progressStream
			<< L"Extracting package ("
			<< i + 1
			<< L"/"
			<< packageResources.size()
			<< L")\u2026";
		SetTDText(hWnd, progressStream.str());

		std::wstringstream fileNameStream;
		fileNameStream
			<< APP_NAME L" Package "
			<< i + 1
			<< L".appx";
		auto file = co_await ExportResourceAsync(packageResources[i], L"FILE", folder, fileNameStream.str());

		packages[i] = file;

		if (i != 0)
		{
			dependencies.Append(Uri{ file.Path() });
		}
	}

	SetTDText(hWnd, L"Signing package with certificate\u2026");
	SignAppxPackage(context, packages[0].Path().c_str());

	SetTDText(hWnd, L"Installing package\u2026");
	PackageManager pm{};
	auto packageResult{ co_await pm.AddPackageAsync(Uri{ packages[0].Path() }, dependencies, DeploymentOptions::None) };
	if (!packageResult.IsRegistered())
	{
		std::wstringstream stream;
		stream
			<< L"the packaged was not successfully deployed. The system reported "
			<< packageResult.ErrorText().c_str();
		TDErrorOut(hWnd, stream.str(), L"");
		co_return;
	}

	SetTDText(hWnd, L"Deleting temporary files\u2026");
	co_await certificateFile.DeleteAsync();
	for (auto file : packages)
	{
		co_await file.DeleteAsync();
	}

	co_await WriteDoneFileAsync(folder, APP_NAME L" Done 2.txt");

	SetTDText(hWnd, L"Uninstalling certificate\u2026");
	co_await WaitForDoneFileAsync(folder, APP_NAME L" Done 3.txt");

	_page = 5;
	SendMessageW(hWnd, TDM_NAVIGATE_PAGE, 0, (LPARAM)&pageFive);
}

HRESULT CALLBACK TaskDialogCallback(_In_ HWND hWnd, _In_ UINT msg, _In_ WPARAM wParam, _In_ LPARAM lParam, _In_ LONG_PTR lpRefData)
{
	switch (msg)
	{
	case TDN_NAVIGATED:
		switch (_page)
		{
		case 3:
			SendMessageW(hWnd, TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE, SIGNING_PAGE_UNDERSTAND_BUTTON, 1);
			break;
		case 4:
			InstallAsync(hWnd);
			break;

		case -1:
			SendMessageW(hWnd, TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE, ERROR_PAGE_TRY_AGAIN_BUTTON, 1);
			SetTDText(hWnd, errorMessageStream.str());
			errorMessageStream.str(L"");
			break;
		}
		break;
	case TDN_BUTTON_CLICKED:
		switch (wParam)
		{
		case DISCLAIMER_PAGE_UNDERSTAND_BUTTON:
			SendMessageW(hWnd, TDM_NAVIGATE_PAGE, 0, (LPARAM)&pageTwo);
			return S_FALSE;
		case AGREEMENT_PAGE_AGREE_BUTTON:
			_page = 3;
			SendMessageW(hWnd, TDM_NAVIGATE_PAGE, 0, (LPARAM)&pageThree);
			return S_FALSE;
		case SIGNING_PAGE_UNDERSTAND_BUTTON:
		case ERROR_PAGE_TRY_AGAIN_BUTTON:
			_page = 4;
			SendMessageW(hWnd, TDM_NAVIGATE_PAGE, 0, (LPARAM)&pageFour);
			return S_FALSE;
		case DONE_PAGE_LAUNCH_BUTTON:
		{
			DWORD blank;
			CComPtr<IApplicationActivationManager> apps;
			if (apps.CoCreateInstance(CLSID_ApplicationActivationManager) == S_OK)
			{
				apps->ActivateApplication(APP_USER_MODEL_ID, nullptr, AO_NONE, &blank);
			}
		}
		break;
		}
		break;
	case TDN_HYPERLINK_CLICKED:
		winrt::Windows::System::Launcher::LaunchUriAsync(Uri{ (LPCWSTR)lParam });
		break;
	}
	return S_OK;
}

int APIENTRY wWinMain(
	_In_ HINSTANCE hInstance,
	_In_opt_ HINSTANCE hPrevInstance,
	_In_ LPWSTR    lpCmdLine,
	_In_ int       nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	_hInst = hInstance;

	winrt::init_apartment(winrt::apartment_type::single_threaded);

	TASKDIALOG_BUTTON pageOneButtons[2];
	pageOneButtons[0].nButtonID = DISCLAIMER_PAGE_UNDERSTAND_BUTTON;
	pageOneButtons[0].pszButtonText = DISCLAIMER_ACCEPT_BUTTON;
	pageOneButtons[1].nButtonID = DISCLAIMER_PAGE_CANCEL_BUTTON;
	pageOneButtons[1].pszButtonText = DISCLAIMER_CANCEL_BUTTON;
	pageOne.cbSize = sizeof pageOne;
	pageOne.hwndParent = nullptr;
	pageOne.hInstance = hInstance;
	pageOne.dwFlags = TDF_USE_COMMAND_LINKS | TDF_ENABLE_HYPERLINKS;
	pageOne.dwCommonButtons = 0;
	pageOne.pszWindowTitle = WINDOW_TITLE;
	pageOne.pszMainIcon = MAKEINTRESOURCEW(IDI_MSIXSIGNER);
	pageOne.pszMainInstruction = DISCLAIMER_PAGE_HEADING;
	pageOne.pszContent = DISCLAIMER_PAGE_CONTENT;
	pageOne.cButtons = 2;
	pageOne.pButtons = pageOneButtons;
	pageOne.nDefaultButton = DISCLAIMER_PAGE_UNDERSTAND_BUTTON;
	pageOne.cRadioButtons = 0;
	pageOne.pRadioButtons = nullptr;
	pageOne.nDefaultRadioButton = 0;
	pageOne.pszVerificationText = nullptr;
	pageOne.pszExpandedInformation = nullptr;
	pageOne.pszExpandedControlText = nullptr;
	pageOne.pszCollapsedControlText = nullptr;
	pageOne.pszFooterIcon = nullptr;
	pageOne.pszFooter = DISCLAIMER_FOOTER;
	pageOne.pfCallback = (PFTASKDIALOGCALLBACK)&TaskDialogCallback;
	pageOne.lpCallbackData = NULL;
	pageOne.cxWidth = 0;

	TASKDIALOG_BUTTON pageTwoButtons[2];
	pageTwoButtons[0].nButtonID = AGREEMENT_PAGE_AGREE_BUTTON;
	pageTwoButtons[0].pszButtonText = LICENSE_AGREE_BUTTON;
	pageTwoButtons[1].nButtonID = AGREEMENT_PAGE_DISAGREE_BUTTON;
	pageTwoButtons[1].pszButtonText = LICENSE_DISAGREE_BUTTON;
	pageTwo.cbSize = sizeof pageTwo;
	pageTwo.hwndParent = nullptr;
	pageTwo.hInstance = hInstance;
	pageTwo.dwFlags = TDF_USE_COMMAND_LINKS | TDF_ENABLE_HYPERLINKS;
	pageTwo.dwCommonButtons = 0;
	pageTwo.pszWindowTitle = WINDOW_TITLE;
	pageTwo.pszMainIcon = MAKEINTRESOURCEW(IDI_MSIXSIGNER);
	pageTwo.pszMainInstruction = LICENSE_PAGE_HEADING;
	pageTwo.pszContent = LICENSE_PAGE_CONTENT;
	pageTwo.cButtons = 2;
	pageTwo.pButtons = pageTwoButtons;
	pageTwo.nDefaultButton = AGREEMENT_PAGE_AGREE_BUTTON;
	pageTwo.cRadioButtons = 0;
	pageTwo.pRadioButtons = nullptr;
	pageTwo.nDefaultRadioButton = 0;
	pageTwo.pszVerificationText = nullptr;
	pageTwo.pszExpandedInformation = nullptr;
	pageTwo.pszExpandedControlText = nullptr;
	pageTwo.pszCollapsedControlText = nullptr;
	pageTwo.pszFooterIcon = nullptr;
	pageTwo.pszFooter = LICENSE_FOOTER;
	pageTwo.pfCallback = (PFTASKDIALOGCALLBACK)&TaskDialogCallback;
	pageTwo.lpCallbackData = NULL;
	pageTwo.cxWidth = 0;

	TASKDIALOG_BUTTON pageThreeButtons[2];
	pageThreeButtons[0].nButtonID = SIGNING_PAGE_UNDERSTAND_BUTTON;
	pageThreeButtons[0].pszButtonText = CONFIRMATION_INSTALL_BUTTON;
	pageThreeButtons[1].nButtonID = SIGNING_PAGE_CANCEL_BUTTON;
	pageThreeButtons[1].pszButtonText = CONFIRMATION_CANCEL_BUTTON;
	pageThree.cbSize = sizeof pageThree;
	pageThree.hwndParent = nullptr;
	pageThree.hInstance = hInstance;
	pageThree.dwFlags = TDF_ENABLE_HYPERLINKS;
	pageThree.dwCommonButtons = 0;
	pageThree.pszWindowTitle = WINDOW_TITLE;
	pageThree.pszMainIcon = MAKEINTRESOURCEW(IDI_MSIXSIGNER);
	pageThree.pszMainInstruction = CONFIRMATION_PAGE_HEADING;
	pageThree.pszContent = CONFIRMATION_PAGE_CONTENT;
	pageThree.cButtons = 2;
	pageThree.pButtons = pageThreeButtons;
	pageThree.nDefaultButton = SIGNING_PAGE_UNDERSTAND_BUTTON;
	pageThree.cRadioButtons = 0;
	pageThree.pRadioButtons = nullptr;
	pageThree.nDefaultRadioButton = 0;
	pageThree.pszVerificationText = nullptr;
	pageThree.pszExpandedInformation = nullptr;
	pageThree.pszExpandedControlText = nullptr;
	pageThree.pszCollapsedControlText = nullptr;
	pageThree.pszFooterIcon = nullptr;
	pageThree.pszFooter = CONFIRMATION_FOOTER;
	pageThree.pfCallback = (PFTASKDIALOGCALLBACK)&TaskDialogCallback;
	pageThree.lpCallbackData = NULL;
	pageThree.cxWidth = 0;

	TASKDIALOG_BUTTON pageFourButtons[1];
	pageFourButtons[0].nButtonID = INSTALLING_PAGE_CANCEL_BUTTON;
	pageFourButtons[0].pszButtonText = INSTALLATION_CANCEL_BUTTON;
	pageFour.cbSize = sizeof pageFour;
	pageFour.hwndParent = nullptr;
	pageFour.hInstance = hInstance;
	pageFour.dwFlags = TDF_SHOW_MARQUEE_PROGRESS_BAR | TDF_ENABLE_HYPERLINKS;
	pageFour.dwCommonButtons = 0;
	pageFour.pszWindowTitle = WINDOW_TITLE;
	pageFour.pszMainIcon = MAKEINTRESOURCEW(IDI_MSIXSIGNER);
	pageFour.pszMainInstruction = INSTALLATION_PAGE_HEADING;
	pageFour.pszContent = INSTALLATION_PAGE_CONTENT;
	pageFour.cButtons = 1;
	pageFour.pButtons = pageFourButtons;
	pageFour.nDefaultButton = INSTALLING_PAGE_CANCEL_BUTTON;
	pageFour.cRadioButtons = 0;
	pageFour.pRadioButtons = nullptr;
	pageFour.nDefaultRadioButton = 0;
	pageFour.pszVerificationText = nullptr;
	pageFour.pszExpandedInformation = nullptr;
	pageFour.pszExpandedControlText = nullptr;
	pageFour.pszCollapsedControlText = nullptr;
	pageFour.pszFooterIcon = nullptr;
	pageFour.pszFooter = INSTALLATION_FOOTER;
	pageFour.pfCallback = (PFTASKDIALOGCALLBACK)&TaskDialogCallback;
	pageFour.lpCallbackData = NULL;
	pageFour.cxWidth = 0;

	TASKDIALOG_BUTTON pageFiveButtons[2];
	pageFiveButtons[0].nButtonID = DONE_PAGE_LAUNCH_BUTTON;
	pageFiveButtons[0].pszButtonText = SUCCESS_LAUNCH_BUTTON;
	pageFiveButtons[1].nButtonID = DONE_PAGE_CLOSE_BUTTON;
	pageFiveButtons[1].pszButtonText = SUCCESS_CLOSE_BUTTON;
	pageFive.cbSize = sizeof pageFive;
	pageFive.hwndParent = nullptr;
	pageFive.hInstance = hInstance;
	pageFive.dwFlags = TDF_USE_COMMAND_LINKS | TDF_ENABLE_HYPERLINKS;
	pageFive.dwCommonButtons = 0;
	pageFive.pszWindowTitle = WINDOW_TITLE;
	pageFive.pszMainIcon = MAKEINTRESOURCEW(IDI_MSIXSIGNER);
	pageFive.pszMainInstruction = SUCCESS_PAGE_HEADING;
	pageFive.pszContent = SUCCESS_PAGE_CONTENT;
	pageFive.cButtons = 2;
	pageFive.pButtons = pageFiveButtons;
	pageFive.nDefaultButton = DONE_PAGE_LAUNCH_BUTTON;
	pageFive.cRadioButtons = 0;
	pageFive.pRadioButtons = nullptr;
	pageFive.nDefaultRadioButton = 0;
	pageFive.pszVerificationText = nullptr;
	pageFive.pszExpandedInformation = nullptr;
	pageFive.pszExpandedControlText = nullptr;
	pageFive.pszCollapsedControlText = nullptr;
	pageFive.pszFooterIcon = nullptr;
	pageFive.pszFooter = SUCCESS_FOOTER;
	pageFive.pfCallback = (PFTASKDIALOGCALLBACK)&TaskDialogCallback;
	pageFive.lpCallbackData = NULL;
	pageFive.cxWidth = 0;

	TASKDIALOG_BUTTON errorPageButtons[2];
	errorPageButtons[0].nButtonID = ERROR_PAGE_TRY_AGAIN_BUTTON;
	errorPageButtons[0].pszButtonText = L"Try again";
	errorPageButtons[1].nButtonID = ERROR_PAGE_CLOSE_BUTTON;
	errorPageButtons[1].pszButtonText = L"Close";
	errorPage.cbSize = sizeof errorPage;
	errorPage.hwndParent = nullptr;
	errorPage.hInstance = hInstance;
	errorPage.dwFlags = TDF_USE_COMMAND_LINKS | TDF_ENABLE_HYPERLINKS;
	errorPage.dwCommonButtons = 0;
	errorPage.pszWindowTitle = WINDOW_TITLE;
	errorPage.pszMainIcon = TD_ERROR_ICON;
	errorPage.pszMainInstruction = L"Installation failed";
	errorPage.pszContent = APP_NAME L" could not install because";
	errorPage.cButtons = 2;
	errorPage.pButtons = errorPageButtons;
	errorPage.nDefaultButton = ERROR_PAGE_TRY_AGAIN_BUTTON;
	errorPage.cRadioButtons = 0;
	errorPage.pRadioButtons = nullptr;
	errorPage.nDefaultRadioButton = 0;
	errorPage.pszVerificationText = nullptr;
	errorPage.pszExpandedInformation = nullptr;
	errorPage.pszExpandedControlText = nullptr;
	errorPage.pszCollapsedControlText = nullptr;
	errorPage.pszFooterIcon = nullptr;
	errorPage.pszFooter = FAILURE_FOOTER;
	errorPage.pfCallback = (PFTASKDIALOGCALLBACK)&TaskDialogCallback;
	errorPage.lpCallbackData = NULL;
	errorPage.cxWidth = 0;

	TaskDialogIndirect(&pageOne, nullptr, nullptr, nullptr);
}
