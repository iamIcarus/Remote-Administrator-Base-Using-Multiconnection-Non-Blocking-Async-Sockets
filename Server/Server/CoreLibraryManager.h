#pragma once
#include <string>
using namespace std;

class CoreLibraryManager
{
	public:
		CoreLibraryManager();
		bool IsCoreDllLoaded();
		void FreeCoreDll();
		void ExecuteCommand(string command);
	private:
		void LoadCoreLibrary();


};