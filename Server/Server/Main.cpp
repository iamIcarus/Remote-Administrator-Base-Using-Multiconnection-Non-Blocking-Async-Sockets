#pragma once
#include <tchar.h>
#include "ConnectionManager.h"
#include <windows.h>
#include "CoreLibraryManager.h"

using namespace std;

ConnectionManager _ConnectionManager;
CoreLibraryManager _LibraryManager;

LRESULT CALLBACK WinProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
HWND CreateWindowGUI(HINSTANCE hInst);
void HandleMessage(Message msg);


void GetSettings(char*& Host, int& Port)
{

	//Open self
	char directory[MAX_PATH];
	GetModuleFileNameA(NULL, directory, 500);

	HANDLE hFile = CreateFileA(directory, GENERIC_READ, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	DWORD dwFileSize = GetFileSize(hFile, NULL);
	DWORD dwBytesRead = 0;
	LPBYTE lpBuffer = new BYTE[dwFileSize];
	ReadFile(hFile, lpBuffer, dwFileSize, &dwBytesRead, NULL);
	CloseHandle(hFile);

	//Loop until we find tag [STOP]

	char* settings = NULL;
	for (int i = dwFileSize-1; i > 0; i--)
	{
		char h = (char)lpBuffer[i];
		char h1 = (char)lpBuffer[i-1];
		char h2 = (char)lpBuffer[i-2];
		char h3 = (char)lpBuffer[i-3];
		char h4 = (char)lpBuffer[i-4];
		char h5 = (char)lpBuffer[i-5];

		if (h == ']' && h1 == 'P'&& h2 == 'O'&& h3 == 'T'&& h4 == 'S'&& h5 == '[')
		{
			int offset = i + 1;

			int settingsSize = (dwFileSize - offset) + 1; // Add null terminating character
			settings = new char[settingsSize];
			ZeroMemory(settings, settingsSize);
			CopyMemory(settings, lpBuffer + offset, settingsSize-1); // Minus the null terminating character
		}

	}


	if (settings == NULL)
		return;

	std::vector<char*> UserSettings;
	char* chars_array = strtok(settings, "|");
	while (chars_array)
	{
		UserSettings.push_back(chars_array);
		chars_array = strtok(NULL, "|");
	}

	try
	{
		Host = UserSettings[0];
		Port = atoi(UserSettings[1]);
	}
	catch (exception e)
	{


	}

}
int WINAPI WinMain(HINSTANCE hInst, HINSTANCE hPrevInst, LPSTR lpCmdLine, int nShowCmd)
{
	char* Host = NULL;
	int Port = -1;
	GetSettings(Host,Port);
	_ConnectionManager = ConnectionManager(Host, Port);

	ShowWindow(CreateWindowGUI(hInst), nShowCmd);

	_LibraryManager = CoreLibraryManager();

	MSG msg;
	ZeroMemory(&msg, sizeof(MSG));

	while (GetMessage(&msg, NULL, 0, 0))
	{		
		TranslateMessage(&msg);			
		DispatchMessage(&msg);
	}

	return 0;
}

HWND CreateWindowGUI(HINSTANCE hInst)
{
	WNDCLASSEX wClass;
	ZeroMemory(&wClass, sizeof(WNDCLASSEX));
	wClass.cbClsExtra = NULL;
	wClass.cbSize = sizeof(WNDCLASSEX);
	wClass.cbWndExtra = NULL;
	wClass.hbrBackground = (HBRUSH)COLOR_WINDOW;
	wClass.hCursor = LoadCursor(NULL, IDC_ARROW);
	wClass.hIcon = NULL;
	wClass.hIconSm = NULL;
	wClass.hInstance = hInst;
	wClass.lpfnWndProc = (WNDPROC)WinProc;
	wClass.lpszClassName = _T("Window Class");
	wClass.lpszMenuName = NULL;
	wClass.style = CS_HREDRAW | CS_VREDRAW;

	if (!RegisterClassEx(&wClass))
	{
		int nResult = GetLastError();
		MessageBoxA(NULL, "Window class creation failed\r\nError code:", "Window Class Failed", MB_ICONERROR);
	}

	HWND hWnd = CreateWindowEx(NULL, _T("Window Class"), _T("Server Window"), WS_OVERLAPPEDWINDOW, 200, 200, 300, 100, NULL, NULL, hInst, NULL);

	if (!hWnd)
	{
		int nResult = GetLastError();
		MessageBoxA(NULL, "Window creation failed\r\nError code:", "Window Creation Failed", MB_ICONERROR);
	}

	return hWnd;
}

LRESULT CALLBACK WinProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
		case WM_CREATE:
			_ConnectionManager.SetupSocket(hWnd);
		break;	

		case WM_DESTROY:
			_LibraryManager.FreeCoreDll();
			_ConnectionManager.CloseSocketConnection();
			  return 0;	
		break;

		case WM_SOCKET:
		{
				if (WSAGETSELECTERROR(lParam))
				{
					_ConnectionManager.CloseSocket();
					_ConnectionManager.SetupSocket(hWnd);
					break;
				}

				switch (WSAGETSELECTEVENT(lParam))
			    {
					case FD_READ:
					{
						string szIncoming =_ConnectionManager.GetRecvData();	
						_ConnectionManager.HandleMessage(_ConnectionManager.ParseMessage(szIncoming));
					}
					break;

					 case FD_CLOSE:
					 {
						_ConnectionManager.CloseSocket();
						_ConnectionManager.SetupSocket(hWnd);
						
					 }
					 break;
				}
		}
	}

	return DefWindowProc(hWnd, msg, wParam, lParam);
}

