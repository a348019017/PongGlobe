#version 450
#extension GL_ARB_separate_shader_objects : enable               



layout(location=0) in vec2 textureCoordinate;
layout(location = 0) out vec4 fragmentColor;
//��������Tex�Ͳ�����
layout(set = 0, binding = 0) uniform texture2D Tex;
layout(set = 0, binding = 1) uniform sampler Samp;


void main()
{	  
   //������������ɫ
    fragmentColor=texture(sampler2D(Tex,Samp), textureCoordinate);   
   if(fragmentColor.a<0.2)
    discard;
           
}