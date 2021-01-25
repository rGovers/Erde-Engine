#pragma once

#if _MSC_VER
#define DllExport __declspec( dllexport )
#elif __GNUC__
#define DllExport __attribute__((visibility("default")))
#else
#define DllExport
#endif

#define EExport extern "C" DllExport

#if _MSC_VER
#define EExportFunc(Ret, Func) EExport Ret __cdecl Func
#else
#define EExportFunc(Ret, Func) EExport Ret Func __attribute__((cdecl))
#endif
