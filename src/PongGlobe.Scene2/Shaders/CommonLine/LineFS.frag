#version 450
#extension GL_ARB_separate_shader_objects : enable
                 

//�����ߵ���ʽ��Ϣ,Ŀǰ�������߿����ɫ��Ϣ
layout(set=1, binding = 0) uniform LineStyle 
{
    vec4  LineColor;
    float Thickess;
} linestyle;

layout(location = 0) out vec4 outColor;

void main()
{
     //����ÿ�����Normal��NormalΪ��ı��洹ֱ���⣬�����normal��view�ļн�����-90-90��������Ⱦ����֮��Cull��,����˵����CULL��������һ���polyline
	//���Ƽ��㷨�ߺ�view�ߵļн�
	//float angle=1.0/length(ubo.CameraEye);
	//if(dot(normalize(ubo.CameraEye),normal)<=angle) discard;
	//��δ����cull
	outColor=linestyle.LineColor;
}