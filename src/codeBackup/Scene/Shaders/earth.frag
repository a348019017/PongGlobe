#version 450
#extension GL_ARB_separate_shader_objects : enable


layout(location = 0) in vec3 fragColor;

layout(set = 1, binding = 0) uniform texture2D Tex;
layout(set = 1, binding = 1) uniform sampler Samp;

layout(location = 0) out vec4 outColor;

void main() {
    outColor = vec4(fragColor,1.0);
}