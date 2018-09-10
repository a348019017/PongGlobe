#version 450
#extension GL_ARB_separate_shader_objects : enable               

//�ȴ�����ǻ�����ʽ
layout(set=1, binding = 0) uniform Style 
{
    vec4 pointColor;           
    float pointSize;
    vec3 spa1;
} style;
//��������Tex�Ͳ�����
layout(set = 1, binding = 1) uniform texture2D Tex;
layout(set = 1, binding = 2) uniform sampler Samp;


//��������������
layout(location=0) in vec2 textureCoordinate;
layout(location = 0) out vec4 fragmentColor;



//�������ݲ����ǹ���
void main()
{	
   //������������ɫ
   vec4 color=texture(sampler2D(Tex,Samp), textureCoordinate);
   if(color.a==0.0)
   {
      discard;
   }
    fragmentColor = color;
	//fragmentColor = vec4(style.pointColor.rgb, style.pointColor.a);
}