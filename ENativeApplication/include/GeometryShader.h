#pragma once

#include "Shader.h"

class GeometryShader : public Shader
{
private:

protected:

public:
    GeometryShader(const char* a_source, Pipeline* a_pipeline);
    virtual ~GeometryShader();

    virtual e_ShaderType GetShaderType() const;
};