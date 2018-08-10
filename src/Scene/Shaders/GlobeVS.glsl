#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(set=0, binding = 0) uniform UniformBufferObject {
    mat4 prjviewmodel;     
    vec3 CameraEyeSquared;
    float spa1;
    vec3 CameraEye;   
    float spa2;
    vec3 CameraLightPosition;
    float spa3;
    vec4 DiffuseSpecularAmbientShininess;
    vec3 GlobeOneOverRadiiSquared;
    float spa4;
} ubo;

layout(location = 0) in vec3 position;
layout(location = 0) out vec3 worldPosition;

out gl_PerVertex {
    vec4 gl_Position;
};

void main()                     
{
    gl_Position = ubo.prjviewmodel * vec4(position,1.0); 
    worldPosition = position;
}