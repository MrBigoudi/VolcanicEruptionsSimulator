#include <cstdio>
#include <cstdlib>

#include "glad/glad_init.hpp"
#include "opengl.hpp"
#include "shaders.hpp"

int main(int argc, char** argv){

    opengl::initGLFW();
    GLFWwindow* window = opengl::createWindow(640, 480, "test");

    opengl::initGLAD();

    GLuint programID = opengl::createProgram("src/shaders/vert.glsl", "src/shaders/frag.glsl");

    exit(EXIT_SUCCESS);
}