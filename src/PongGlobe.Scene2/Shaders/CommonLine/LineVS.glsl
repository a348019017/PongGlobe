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

layout(location = 0) in vec3 position;
layout(location = 0) out  vec3 fragColor;
layout(location = 1) out  vec3 normal;

out gl_PerVertex {
    vec4 gl_Position;
};

vec3 GeodeticSurfaceNormal(vec3 positionOnEllipsoid, vec3 oneOverEllipsoidRadiiSquared)
{
    return normalize(positionOnEllipsoid * oneOverEllipsoidRadiiSquared);
}



void main()                     
{
    gl_Position = ubo.prj * vec4(position,1.0); 
    gl_Position.y=-gl_Position.y;
	normal=GeodeticSurfaceNormal(position,ubo.GlobeOneOverRadiiSquared);
    fragColor=vec3(1.0,0.0,0.0);
}