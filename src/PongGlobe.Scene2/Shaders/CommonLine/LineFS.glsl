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
layout(location = 1) in vec3 normal;
layout(location = 0) out vec4 outColor;


void main()
{
     //����ÿ�����Normal��NormalΪ��ı��洹ֱ���⣬�����normal��view�ļн�����-90-90��������Ⱦ����֮��Cull��,����˵����CULL��������һ���polyline
	//���Ƽ��㷨�ߺ�view�ߵļн�
	float angle=1.0/length(ubo.CameraEye);
	if(dot(normalize(ubo.CameraEye),normal)<=angle) discard;
	outColor=vec4(fragcolor,1.0);
	
}