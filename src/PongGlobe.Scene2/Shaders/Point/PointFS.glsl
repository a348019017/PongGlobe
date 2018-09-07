#version 450
#extension GL_ARB_separate_shader_objects : enable
                 


layout(set=1, binding = 0) uniform Style 
{
    vec4 pointColor;           
    float pointSize;
    vec3 spa1;
} style;

layout(location = 0) out vec4 fragmentColor;

//�������ݲ����ǹ���
void main()
{	
	fragmentColor = vec4(style.pointColor.rgb, style.pointColor.a);
}