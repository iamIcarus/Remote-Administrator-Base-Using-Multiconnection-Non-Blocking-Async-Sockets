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

int WINAPI WinMain(HINSTANCE hInst, HINSTANCE hPrevInst, LPSTR lpCmdLine, int nShowCmd)
{
	_ConnectionManager = ConnectionManager();

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

	HWND hWnd = CreateWindowEx(NULL, _T("Window Class"), _T("EPL 606 - Bot"), WS_OVERLAPPEDWINDOW, 200, 200, 300, 100, NULL, NULL, hInst, NULL);

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

