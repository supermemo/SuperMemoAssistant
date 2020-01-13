#pragma unmanaged
#include "stdafx.h"

extern "C" __declspec(dllexport) int __stdcall registerCall1(int functionPointer, int arg1)
{
    int retVal;

    _asm 
    {
        mov eax, arg1
        call functionPointer
        mov retVal, eax
    }

    return retVal;
}

extern "C" __declspec(dllexport) int __stdcall registerCall2(int functionPointer, int arg1, int arg2)
{
    int retVal;

    _asm 
    {
		mov edx, arg2
        mov eax, arg1
        call functionPointer
        mov retVal, eax
    }

    return retVal;
}

extern "C" __declspec(dllexport) int __stdcall registerCall3(int functionPointer, int arg1, int arg2, int arg3)
{
    int retVal;

    _asm 
    {
		mov ecx, arg3
		mov edx, arg2
        mov eax, arg1
        call functionPointer
        mov retVal, eax
    }

    return retVal;
}

extern "C" __declspec(dllexport) int __stdcall registerCall4(int functionPointer, int arg1, int arg2, int arg3, int arg4)
{
    int retVal;

    _asm 
    {
		push arg4
		mov ecx, arg3
		mov edx, arg2
        mov eax, arg1
        call functionPointer
        mov retVal, eax
    }

    return retVal;
}

extern "C" __declspec(dllexport) int __stdcall registerCall5(int functionPointer, int arg1, int arg2, int arg3, int arg4, int arg5)
{
    int retVal;

    _asm 
    {
		push arg5
		push arg4
		mov ecx, arg3
		mov edx, arg2
        mov eax, arg1
        call functionPointer
        mov retVal, eax
    }

    return retVal;
}

extern "C" __declspec(dllexport) int __stdcall registerCall6(int functionPointer, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6)
{
    int retVal;

    _asm 
    {
		push arg6
		push arg5
		push arg4
		mov ecx, arg3
		mov edx, arg2
        mov eax, arg1
        call functionPointer
        mov retVal, eax
    }

    return retVal;
}