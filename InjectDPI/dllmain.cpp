#include "pch.h"
#include <windows.h>
#include <string>
#include <iostream>
#include "include\detours.h"

#include <ShellScalingApi.h>

typedef int(WINAPI* GetDeviceCaps_Func)(HDC hdc, int index);
typedef HRESULT(WINAPI* GetDpiForMonitor_Func)(HMONITOR hmonitor, MONITOR_DPI_TYPE dpiType, UINT* dpiX, UINT* dpiY);
typedef UINT(WINAPI* GetDpiForWindow_Func)(HWND hwnd);
typedef HRESULT(WINAPI* GetScaleFactorForMonitor_Func)(HMONITOR hMon, DEVICE_SCALE_FACTOR* pScale);
typedef DEVICE_SCALE_FACTOR(WINAPI* GetScaleFactorForDevice_Func)(DISPLAY_DEVICE_TYPE deviceType);
typedef BOOL(WINAPI* IsProcessDPIAware_Func)();

GetDeviceCaps_Func origGetDeviceCaps_gdi32 = nullptr;
GetDpiForMonitor_Func origGetDpiForMonitor_shcore = nullptr;
GetScaleFactorForMonitor_Func origGetScaleFactorForMonitor_shcore = nullptr;
GetScaleFactorForDevice_Func origGetScaleFactorForDevice_shcore = nullptr;
GetDpiForWindow_Func origGetDpiForWindow_user32 = nullptr;
IsProcessDPIAware_Func origIsProcessDPIAware_user32 = nullptr;

DEVICE_SCALE_FACTOR g_spoofScaleFactor = DEVICE_SCALE_FACTOR_INVALID; //Custom scale factor;
int g_spoofDpi = USER_DEFAULT_SCREEN_DPI;

int g = RDW_ALLCHILDREN;


#pragma region Misc Helpers

// Function to convert LPCWSTR to std::string - https://www.geeksforgeeks.org/convert-lpcwstr-to-std_string-in-cpp/ with edits
std::wstring ConvLPCWSTR2WString(LPCWSTR lpcwszStr)
{
    // Determine the length of the converted string 
    int strLength
        = WideCharToMultiByte(CP_UTF8, 0, lpcwszStr, -1,
            nullptr, 0, nullptr, nullptr);

    // Create a std::string with the determined length 
    std::string str(strLength, 0);

    // Perform the conversion from LPCWSTR to std::string 
    WideCharToMultiByte(CP_UTF8, 0, lpcwszStr, -1, &str[0],
        strLength, nullptr, nullptr);

    std::wstring wstr(str.begin(), str.end());

    // Return the converted std::wstring 
    return wstr;
}

#pragma endregion


#pragma region Detoured functions

int WINAPI GetDeviceCaps_Detour(HDC hdc, int index) 
{
    int tech = origGetDeviceCaps_gdi32(hdc, TECHNOLOGY);
    if (tech == DT_RASDISPLAY)
    {
        int horzRes = -1;
        int vertRes = -1;
        int spoofDpi = (USER_DEFAULT_SCREEN_DPI * g_spoofScaleFactor) / 100;
        float spoofDpmm = spoofDpi / 25.4; //dots per millimeter

        switch (index) {
            case HORZRES:
                return origGetDeviceCaps_gdi32(hdc, DESKTOPHORZRES);
            case VERTRES:
                return origGetDeviceCaps_gdi32(hdc, DESKTOPVERTRES);
            case HORZSIZE:
                horzRes = origGetDeviceCaps_gdi32(hdc, DESKTOPHORZRES);
                return horzRes / spoofDpmm;
            case VERTSIZE:
                vertRes = origGetDeviceCaps_gdi32(hdc, DESKTOPVERTRES);
                return vertRes / spoofDpmm;
            case LOGPIXELSX:
            case LOGPIXELSY:
                return spoofDpi;
            default:
                return origGetDeviceCaps_gdi32(hdc, index);
        }
    }
}

HRESULT WINAPI GetDpiForMonitor_Detour(HMONITOR hmonitor, MONITOR_DPI_TYPE dpiType, UINT* dpiX, UINT* dpiY) 
{
    HRESULT result;
    switch (dpiType) {
        case MDT_EFFECTIVE_DPI:
        case MDT_RAW_DPI:
            result = origGetDpiForMonitor_shcore(hmonitor, dpiType, dpiX, dpiY);

            *dpiX = (USER_DEFAULT_SCREEN_DPI * g_spoofScaleFactor) / 100;
            *dpiY = (USER_DEFAULT_SCREEN_DPI * g_spoofScaleFactor) / 100;
            return result;
        default:
            return origGetDpiForMonitor_shcore(hmonitor, dpiType, dpiX, dpiY);
    }
}

UINT WINAPI GetDpiForWindow_Detour(HWND hwnd) 
{
    return (USER_DEFAULT_SCREEN_DPI * g_spoofScaleFactor) / 100;
}

HRESULT WINAPI GetScaleFactorForMonitor_Detour(HMONITOR hMon, DEVICE_SCALE_FACTOR* pScale) 
{
    HRESULT result = origGetScaleFactorForMonitor_shcore(hMon, pScale);
    *pScale = static_cast<DEVICE_SCALE_FACTOR>(g_spoofScaleFactor);
    return result;
}

DEVICE_SCALE_FACTOR WINAPI GetScaleFactorForDevice_Detour(DISPLAY_DEVICE_TYPE deviceType) 
{
    return static_cast<DEVICE_SCALE_FACTOR>(g_spoofScaleFactor);
}

BOOL WINAPI IsProcessDPIAware_Detour() 
{
    return false;
}
#pragma endregion


#pragma region Hook/Unhook/Change

extern "C" __declspec(dllexport) DWORD WINAPI Hook(LPVOID lpParameter) 
{
    DetourTransactionBegin();
    DetourUpdateThread(GetCurrentThread());

    origGetDeviceCaps_gdi32 = (GetDeviceCaps_Func)GetProcAddress(GetModuleHandle(L"gdi32.dll"), "GetDeviceCaps");
    origGetDpiForMonitor_shcore = (GetDpiForMonitor_Func)GetProcAddress(GetModuleHandle(L"shcore.dll"), "GetDpiForMonitor");
    origGetScaleFactorForMonitor_shcore = (GetScaleFactorForMonitor_Func)GetProcAddress(GetModuleHandle(L"shcore.dll"), "GetScaleFactorForMonitor");
    origGetScaleFactorForDevice_shcore = (GetScaleFactorForDevice_Func)GetProcAddress(GetModuleHandle(L"shcore.dll"), "GetScaleFactorForDevice");
    origGetDpiForWindow_user32 = (GetDpiForWindow_Func)GetProcAddress(GetModuleHandle(L"user32.dll"), "GetDpiForWindow");
    //origIsProcessDPIAware_user32 = (IsProcessDPIAware_Func)GetProcAddress(GetModuleHandle(L"user32.dll"), "IsProcessDPIAware");

    DetourAttach(&(PVOID&)origGetDeviceCaps_gdi32, GetDeviceCaps_Detour);
    DetourAttach(&(PVOID&)origGetDpiForMonitor_shcore, GetDpiForMonitor_Detour);
    DetourAttach(&(PVOID&)origGetScaleFactorForMonitor_shcore, GetScaleFactorForMonitor_Detour);
    DetourAttach(&(PVOID&)origGetScaleFactorForDevice_shcore, GetScaleFactorForDevice_Detour);
    DetourAttach(&(PVOID&)origGetDpiForWindow_user32, GetDpiForWindow_Detour);
    //DetourAttach(&(PVOID&)origIsProcessDPIAware_user32, IsProcessDPIAware_Detour);

    DetourTransactionCommit();

    return 0;
}

extern "C" __declspec(dllexport) DWORD WINAPI Unhook(LPVOID lpParameter)
{
    DetourTransactionBegin();
    DetourUpdateThread(GetCurrentThread());

    DetourDetach(&(PVOID&)origGetDeviceCaps_gdi32, GetDeviceCaps_Detour);
    DetourDetach(&(PVOID&)origGetDpiForMonitor_shcore, GetDpiForMonitor_Detour);
    DetourDetach(&(PVOID&)origGetScaleFactorForMonitor_shcore, GetScaleFactorForMonitor_Detour);
    DetourDetach(&(PVOID&)origGetScaleFactorForDevice_shcore, GetScaleFactorForDevice_Detour);
    DetourDetach(&(PVOID&)origGetDpiForWindow_user32, GetDpiForWindow_Detour);
    //DetourDetach(&(PVOID&)origIsProcessDPIAware_user32, IsProcessDPIAware_Detour);

    DetourTransactionCommit();

    return 0;
}

extern "C" __declspec(dllexport) DWORD WINAPI SetDpi(LPVOID lpParameter)
{
    g_spoofScaleFactor = *(DEVICE_SCALE_FACTOR*)lpParameter;
    g_spoofDpi = (USER_DEFAULT_SCREEN_DPI * g_spoofScaleFactor) / 100;

    return 0;
}

extern "C" __declspec(dllexport) DWORD WINAPI SetDpiAndHook(LPVOID lpParameter)
{
    SetDpi(lpParameter);
    return Hook(lpParameter);
}

#pragma endregion


#pragma region Inject/Control

#pragma region Helpers

//Function to get path of dll that is being called from - https://stackoverflow.com/questions/6924195/get-dll-path-at-runtime with edits
std::wstring GetSelfPath() {
    wchar_t dllPath[MAX_PATH];
    HMODULE hm = NULL;
    if (GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS |
        GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
        (LPCWSTR)&Hook, &hm) == 0)
    {
        return L"";
    }
    if (GetModuleFileName(hm, dllPath, sizeof(dllPath)) == 0)
    {
        return L"";
    }
    return ConvLPCWSTR2WString(dllPath);
}

template <typename T>
bool remoteWrite(HANDLE hProcess, T data, LPVOID* pRemoteMem, size_t dataSize = sizeof(T)) {
    *pRemoteMem = VirtualAllocEx(hProcess, nullptr, dataSize, MEM_COMMIT, PAGE_READWRITE);
    if (!pRemoteMem) {
        return false;
    }

    if (!WriteProcessMemory(hProcess, *pRemoteMem, (LPCVOID)data, dataSize, nullptr)) {
        VirtualFreeEx(hProcess, pRemoteMem, 0, MEM_RELEASE);
        return false;
    }

    return true;
}

bool remoteCallMethod(HANDLE hProcess, std::wstring dll, std::string method, LPVOID lpParameter = nullptr) {
    LPVOID pMethod = (LPVOID)GetProcAddress(GetModuleHandle(dll.c_str()), method.c_str());
    if (!pMethod) {
        return false;
    }

    HANDLE hMethodThread = CreateRemoteThread(hProcess, nullptr, 0, (LPTHREAD_START_ROUTINE)pMethod, lpParameter, 0, nullptr);
    if (!hMethodThread) {
        VirtualFreeEx(hProcess, lpParameter, 0, MEM_RELEASE);
        return false;
    }

    WaitForSingleObject(hMethodThread, INFINITE);
    CloseHandle(hMethodThread);

    return true;
}

#pragma endregion


extern "C" __declspec(dllexport) bool Install(DWORD processId, DEVICE_SCALE_FACTOR scaleFactor) 
{
    std::wstring dllPath = GetSelfPath();

    HANDLE hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE, FALSE, processId);
    if (!hProcess) 
    {
        return false;
    }

    //Write dll path to remote memory and load it...
    LPVOID pRemotePathMem;
    if (!remoteWrite<const wchar_t*>(hProcess, dllPath.c_str(), &pRemotePathMem, (dllPath.size() + 1) * sizeof(wchar_t)))
    {
        CloseHandle(hProcess);
        return false;
    }

    if (!remoteCallMethod(hProcess, L"kernel32.dll", "LoadLibraryW", pRemotePathMem))
    {
        VirtualFreeEx(hProcess, pRemotePathMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return false;
    }

    //Write scale factor to remote memory...
    LPVOID pRemoteScaleFactorMem;
    if (!remoteWrite<DEVICE_SCALE_FACTOR*>(hProcess, &scaleFactor, &pRemoteScaleFactorMem))
    {
        VirtualFreeEx(hProcess, pRemotePathMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return false;
    }

    if (!remoteCallMethod(hProcess, dllPath.c_str(), "SetDpiAndHook", pRemoteScaleFactorMem))
    {
        VirtualFreeEx(hProcess, pRemoteScaleFactorMem, 0, MEM_RELEASE);
        VirtualFreeEx(hProcess, pRemotePathMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return false;
    }

    CloseHandle(hProcess);
    return true;
}

extern "C" __declspec(dllexport) bool Enable(DWORD processId)
{
    std::wstring dllPath = GetSelfPath();

    HANDLE hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE, FALSE, processId);
    if (!hProcess)
    {
        return false;
    }

    if (!remoteCallMethod(hProcess, dllPath.c_str(), "Hook"))
    {
        CloseHandle(hProcess);
        return false;
    }

    CloseHandle(hProcess);
    return true;
}

extern "C" __declspec(dllexport) bool Disable(DWORD processId)
{
    std::wstring dllPath = GetSelfPath();

    HANDLE hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE, FALSE, processId);
    if (!hProcess)
    {
        return false;
    }

    if (!remoteCallMethod(hProcess, dllPath.c_str(), "Unhook"))
    {
        CloseHandle(hProcess);
        return false;
    }

    CloseHandle(hProcess);
    return true;
}

extern "C" __declspec(dllexport) bool SetScaleFactor(DWORD processId, DEVICE_SCALE_FACTOR scaleFactor)
{
    std::wstring dllPath = GetSelfPath();

    HANDLE hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE, FALSE, processId);
    if (!hProcess)
    {
        return false;
    }

    //Write scale factor to remote memory...
    LPVOID pRemoteScaleFactorMem;
    if (!remoteWrite<DEVICE_SCALE_FACTOR*>(hProcess, &scaleFactor, &pRemoteScaleFactorMem))
    {
        CloseHandle(hProcess);
        return false;
    }

    if (!remoteCallMethod(hProcess, dllPath.c_str(), "SetDpi", pRemoteScaleFactorMem))
    {
        VirtualFreeEx(hProcess, pRemoteScaleFactorMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return false;
    }

    CloseHandle(hProcess);
    return true;
}

#pragma endregion


// --Entry point--
BOOL APIENTRY DllMain(HMODULE hModule, DWORD ulReason, LPVOID lpReserved) 
{
    return TRUE;
}