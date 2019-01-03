#if false
using namespace System;

namespace SuperMemoAssistantHooksNativeLib {
	void __stdcall wndProcNativeWrapper();
	
	public ref class WndProcWrapper
	{
	public:
		WndProcWrapper() {}

		static WndProcWrapper^ Instance()
		{
			if (instance == nullptr)
				instance = gcnew WndProcWrapper();

			return instance;
		}

		void SetCallback(Action<int, int>^ callback)
		{
			Callback = callback;
		}

		void WndProc(int smMain, int msg)
		{
			System::Diagnostics::Debugger::Launch();
			Callback(smMain, msg);
		}

		int GetWndProcNativeWrapperAddr()
		{
			return (int)(&wndProcNativeWrapper);
		}

	private:
		Action<int, int>^ Callback;
		static WndProcWrapper^ instance;

	};

	void callManaged(int smMain, int msg)
	{
		return WndProcWrapper::Instance()->WndProc(smMain, msg);
	}

	void __stdcall wndProcNativeWrapper()
	{
		int smMain;
		int msg;

		_asm {
			mov smMain, eax
			mov msg, edx
		}

		callManaged(smMain, msg);
		/*
		_asm {
			mov eax, test
		}*/
	}
}

#endif
//#if false

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
	/*
	public class WndProcWrapper
	{
	public:
		WndProcWrapper(void* callback)
		{
			Callback = static_cast<wndProcCallback>(callback);
		}

		void WndProc(int smMain, int msg)
		{
			//System::Diagnostics::Debugger::Launch();
			Callback(smMain, msg);
		}

		int GetWndProcNativeWrapperAddr()
		{
			void (__stdcall WndProcWrapper::*pFunc)(void) = &(WndProcWrapper::wndProcNativeWrapper);
			return System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(pFunc).ToInt32();
		}

	private:
		static void __stdcall wndProcNativeWrapper()
		{
			int smMain;
			int msg;

			_asm {
				mov smMain, eax
				mov msg, edx
			}

			Callback(smMain, msg);
		}

	private:
		wndProcCallback Callback;
	};
	*/
}
//#endif