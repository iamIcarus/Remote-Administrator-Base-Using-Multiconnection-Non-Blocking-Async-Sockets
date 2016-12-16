#pragma once
#define _WINSOCKAPI_
#include <winsock2.h>
#include <thread>
#include "Message.h"
#include "jsoncons/json.hpp"
#include "CoreLibraryManager.h"

using jsoncons::json;
#pragma comment(lib, "Ws2_32.lib")

#define WM_SOCKET	 104			// For async socket events
#define WM_SOCKET	 104

class ConnectionManager
{
	public:
		ConnectionManager();
		ConnectionManager(char* Host ,int Port );
		void SetupSocket(HWND hWnd);
		void CloseSocketConnection();
		void CloseSocket();
		Message ParseMessage(string szIncoming);
		void HandleMessage(Message msg);
		void SetLibraryManager(CoreLibraryManager* manager);

		string GetRecvData();		
		static DWORD Thread_Start_TryConnect(LPVOID* param);


	private:
		void TryConnect();
		void SendData();
		void RequestInfo();
		void Pong();
		CoreLibraryManager* _LibraryManager;


};