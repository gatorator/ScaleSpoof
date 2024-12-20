#include <windows.h>
#include <tlhelp32.h>
#include <string>
#include <iostream>

#include <ShellScalingApi.h>

// Function to get the process ID by name
DWORD GetProcessId(const std::wstring& processName) {
    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnapshot == INVALID_HANDLE_VALUE) { return 0; }

    PROCESSENTRY32 pe32 = { sizeof(PROCESSENTRY32) };
    DWORD processId = 0;

    if (Process32First(hSnapshot, &pe32)) {
        do {
            if (processName == pe32.szExeFile) {
                processId = pe32.th32ProcessID;
                break;
            }
        } while (Process32Next(hSnapshot, &pe32));
    }

    CloseHandle(hSnapshot);

    return processId;
}

// Inject DLL into a process and call an exported function
bool InjectDLLAndCallFunction(DWORD processId, const std::wstring& dllPath, const std::string& functionName, DEVICE_SCALE_FACTOR scaleFactor) {
    HANDLE hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE, FALSE, processId);
    if (!hProcess) {
        std::cerr << "Failed to open target process. Error: " << GetLastError() << std::endl;
        return false;
    }

    /* 
    Write dll path to remote memory and load it...
    */
    size_t pathSize = (dllPath.size() + 1) * sizeof(wchar_t);
    LPVOID pRemotePathMem = VirtualAllocEx(hProcess, nullptr, pathSize, MEM_COMMIT, PAGE_READWRITE);
    if (!pRemotePathMem) {
        std::cerr << "Failed to allocate memory in target process. Error: " << GetLastError() << std::endl;
        CloseHandle(hProcess);
        return false;
    }

    std::wcout << L"Allocated memory in target process at address: " << pRemotePathMem << std::endl;

    if (!WriteProcessMemory(hProcess, pRemotePathMem, dllPath.c_str(), pathSize, nullptr)) {
        std::cerr << "Failed to write DLL path to target process memory. Error: " << GetLastError() << std::endl;
        VirtualFreeEx(hProcess, pRemotePathMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return false;
    }

    std::wcout << L"Successfully wrote DLL path to target process memory." << std::endl;

    LPVOID pLoadLibrary = (LPVOID)GetProcAddress(GetModuleHandle(L"kernel32.dll"), "LoadLibraryW");
    if (!pLoadLibrary) {
        std::cerr << "Failed to resolve LoadLibraryW. Error: " << GetLastError() << std::endl;
        VirtualFreeEx(hProcess, pRemotePathMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return false;
    }

    HANDLE hThread = CreateRemoteThread(hProcess, nullptr, 0, (LPTHREAD_START_ROUTINE)pLoadLibrary, pRemotePathMem, 0, nullptr);
    if (!hThread) {
        std::cerr << "Failed to create remote thread for LoadLibraryW. Error: " << GetLastError() << std::endl;
        VirtualFreeEx(hProcess, pRemotePathMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return false;
    }

    std::wcout << L"Remote thread created successfully." << std::endl;

    WaitForSingleObject(hThread, INFINITE);

    /* 
    Write scale factor to remote memory...
    */
    LPVOID pRemoteScaleFactorMem = VirtualAllocEx(hProcess, nullptr, sizeof(DEVICE_SCALE_FACTOR), MEM_COMMIT, PAGE_READWRITE);
    if (!pRemoteScaleFactorMem) {
        std::cerr << "Failed to allocate memory in target process. Error: " << GetLastError() << std::endl;
        CloseHandle(hProcess);
        return false;
    }

    std::wcout << L"Allocated memory in target process at address: " << pRemoteScaleFactorMem << std::endl;

    if (!WriteProcessMemory(hProcess, pRemoteScaleFactorMem, &scaleFactor, pathSize, nullptr)) {
        std::cerr << "Failed to write DLL path to target process memory. Error: " << GetLastError() << std::endl;
        VirtualFreeEx(hProcess, pRemoteScaleFactorMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return false;
    }

    std::wcout << L"Successfully wrote custom scale factor to target process memory." << std::endl;

    // Get address of DLL in memory, resolve and call the remote function
    HMODULE hLocalDLL = LoadLibraryW(dllPath.c_str());
    if (!hLocalDLL) {
        std::cerr << "Failed to load DLL locally. Error: " << GetLastError() << std::endl;
        CloseHandle(hProcess);
        return false;
    }

    FARPROC pLocalFunction = GetProcAddress(hLocalDLL, functionName.c_str());
    if (!pLocalFunction) {
        std::cerr << "Failed to resolve function in local DLL. Error: " << GetLastError() << std::endl;
        FreeLibrary(hLocalDLL);
        CloseHandle(hProcess);
        return false;
    }

    FreeLibrary(hLocalDLL);

    HANDLE hFunctionThread = CreateRemoteThread(hProcess, nullptr, 0, (LPTHREAD_START_ROUTINE)pLocalFunction, pRemoteScaleFactorMem, 0, nullptr);
    if (!hFunctionThread) {
        std::cerr << "Failed to create remote thread for function call. Error: " << GetLastError() << std::endl;
        CloseHandle(hProcess);
        return false;
    }

    WaitForSingleObject(hFunctionThread, INFINITE);
    CloseHandle(hFunctionThread);
    CloseHandle(hProcess);

    return true;
}

int main() {
    const std::wstring processName = L"Code.exe";
    const std::wstring dllPath = L"Z:\\!Owen\\Projects\\Visual Studio\\SpoofDPI\\x64\\Debug\\InjectDPI.dll";
    const std::string functionName = "Hook";

    DWORD processId = GetProcessId(processName);
    if (!processId) {
        std::wcerr << L"Process not found: " << processName << std::endl;
        return 1;
    }

    if (!InjectDLLAndCallFunction(processId, dllPath, functionName, SCALE_200_PERCENT)) {
        std::cerr << "Failed to inject DLL or call the function." << std::endl;
        return 1;
    }

    std::wcout << L"Successfully injected the DLL and called the function." << std::endl;
    return 0;
}