#version 450
#extension GL_ARB_separate_shader_objects : enable               

//先传入的是基本样式
layout(set=1, binding = 0) uniform Style 
{
    vec4 pointColor;           
    float pointSize;
    vec3 spa1;
} style;
//紧接着是Tex和采样器
layout(set = 1, binding = 1) uniform texture2D Tex;
layout(set = 1, binding = 2) uniform sampler Samp;


//传入点的纹理坐标
layout(location=0) in vec2 textureCoordinate;
layout(location = 0) out vec4 fragmentColor;



//点这里暂不考虑光照
void main()
{	
   //计算其纹理颜色
   vec4 color=texture(sampler2D(Tex,Samp), textureCoordinate);
   if(color.a==0.0)
   {
      discard;
   }
    fragmentColor = color;
	//fragmentColor = vec4(style.pointColor.rgb, style.pointColor.a);
}