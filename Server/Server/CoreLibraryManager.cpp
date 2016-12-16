#include "CoreLibraryManager.h"
#include "Command.h"
#include <windows.h>
#include <tchar.h>
#include <vector>


typedef void(__cdecl* STOP_FUNCTION)();
typedef void(__cdecl* START_FUNCTION)(LPWSTR, INT32, INT32);

Command ParseCommand(string CommandString);
vector<string> explode(const string& str, const char& ch);

START_FUNCTION mStart = NULL;
STOP_FUNCTION mStop = NULL;

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
		if (mStart != NULL && mStop != NULL)
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
		mStart = (START_FUNCTION)GetProcAddress(dllHandle, "Start");
		mStop = (STOP_FUNCTION)GetProcAddress(dllHandle, "Stop");
	}
}


void CoreLibraryManager::ExecuteCommand(string command)
{
	
	Command cmd = ParseCommand(command);

	if (cmd.Name.compare("Start") == 0)
	{
		
	}
	else if (cmd.Name.compare("Stop"))
	{
		
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

