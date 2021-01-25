#include <stdio.h>

#include "MonoRuntime.h"

int main (int a_argc, char* a_argv[])
{
    printf("Erde Native Runtime \n");

    char** args = new char*[a_argc + 1];
    args[0] = "./MApplication.exe";
    args[1] = "Embedded";
    for (int i = 2; i < a_argc + 1; ++i)
    {
        args[i] = a_argv[i + 2];
    }

    MonoRuntime* runtime = new MonoRuntime();

    runtime->Run(a_argc + 1, args);

    delete[] args;

    delete runtime;

    return 0;
}