#pragma once

#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>

class MonoRuntime
{
private:
    static MonoRuntime* Instance;

    MonoDomain*   m_domain;
    MonoAssembly* m_assembly;
protected:

public:
    MonoRuntime();
    ~MonoRuntime();

    void Run(int a_argc, char* a_argv[]);

    static MonoDomain* GetDomain();
};