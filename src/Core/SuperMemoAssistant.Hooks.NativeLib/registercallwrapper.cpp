#pragma managed
using namespace System;


namespace SuperMemoAssistantHooksNativeLib {
	void __stdcall wndProcNativeWrapper(void);

	typedef void (*wndProcCallback)(int, int, bool*);

	wndProcCallback callback;

	
	public ref class WndProcWrapper
	{
	public:
		static void SetCallback(IntPtr^ callbackPtr)
		{
			callback = static_cast<wndProcCallback>(callbackPtr->ToPointer());
		}
		
		static int GetWndProcNativeWrapperAddr()
		{
			return (int)(&wndProcNativeWrapper);
		}
	};

#pragma unmanaged
	
	void __stdcall wndProcNativeWrapper()
	{
		int smMain;
		int msgAddr;
		bool* handled;

		_asm {
			mov smMain, eax
			mov msgAddr, edx
			mov handled, ecx
		}

		int* msgStruct = (int*)msgAddr;
		int msgId = *(msgStruct + 1);
		int msgParam = *(msgStruct + 2);

		// Calling .NET is costly, filter out messages
		if (/* WM_QUIT */ (msgId == 0x0012) ||
			/* WM_USER */ (msgId == 2345 && msgParam > 9000000))
			callback(smMain, msgAddr, handled);
	}
}
