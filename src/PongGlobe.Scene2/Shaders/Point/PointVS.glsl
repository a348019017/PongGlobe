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
	vec2 viewport;
	vec2 spa5;
} ubo;

layout(set=1, binding = 0) uniform Style 
{
    vec4 pointColor;           
    float pointSize;
    vec3 spa1;
} style;


layout(location = 0) in vec3 position;

out gl_PerVertex {
    vec4 gl_Position;
	float gl_PointSize;
};

void main()                     
{
    gl_Position = ubo.prj * vec4(position,1.0); 
	gl_Position.y=-gl_Position.y;	
	//gl_pointSize渲染的点是方形的点，仍然需要使用GeometryShader渲染成可观的点
	gl_PointSize=style.pointSize;
}