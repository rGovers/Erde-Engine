#pragma once

#include "Shader.h"

class PixelShader : public Shader
{
private:

protected:

public:
    PixelShader(const char* a_source, Pipeline* a_pipeline);
    virtual ~PixelShader();

    virtual e_ShaderType GetShaderType() const;
};