#pragma once

#include "opengl.hpp"

namespace opengl{

/**
 * Compile and link shaders
 * @param vertexShader The path to the vertex shader
 * @param fragmentShader The path to the fragment shader
 * @return The program's ID
*/
GLuint createProgram(const std::string& vertexShader, const std::string& fragmentShader);

/**
 * Put the content of a shader file into a string
 * @param shader The path to the shader file
 * @return The content of the shader inside a string
*/
std::string loadShader(const std::string& shader);

}