#pragma once

#include "Shader.h"

class VertexShader : public Shader
{
private:

protected:

public:
    VertexShader(const char* a_source, Pipeline* a_pipeline);
    virtual ~VertexShader();

    virtual e_ShaderType GetShaderType() const;
};
