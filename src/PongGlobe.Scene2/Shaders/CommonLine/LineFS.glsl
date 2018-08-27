#version 450
#extension GL_ARB_separate_shader_objects : enable
                 


layout(set=0, binding = 0) uniform UniformBufferObject {
    mat4 prj;         
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


layout(location = 0) in vec3 fragcolor;
layout(location = 0) out vec4 outColor;


void main()
{
	outColor=vec4(fragcolor,1.0);
}