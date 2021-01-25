#include "MonoRuntime.h"

#include <assert.h>
#include <mono/metadata/mono-config.h>

MonoRuntime* MonoRuntime::Instance = nullptr;

MonoRuntime::MonoRuntime()
{
    assert(Instance == nullptr);

    Instance = this;

    m_domain = mono_jit_init("./MApplication.exe");
    mono_config_parse (NULL);

    m_assembly = mono_domain_assembly_open(m_domain, "./MApplication.exe");
    mono_set_dirs(NULL, NULL);

    assert(m_assembly != nullptr);

    MonoAssemblyName* name = mono_assembly_get_name(m_assembly);
    printf(mono_assembly_name_get_name(name));
    printf("\n");
}

MonoRuntime::~MonoRuntime()
{
    mono_jit_cleanup(m_domain);
}

void MonoRuntime::Run(int a_argc, char* a_argv[])
{
#if DEBUG
    const char* options[] = 
    {
     "--debugger-agent=transport=dt_socket,address=127.0.0.1:10000"
    };
    
    mono_jit_parse_options(1, (char**)options);
#endif

    mono_jit_exec(m_domain, m_assembly, a_argc, a_argv);
}

MonoDomain* MonoRuntime::GetDomain()
{
    return Instance->m_domain;
}