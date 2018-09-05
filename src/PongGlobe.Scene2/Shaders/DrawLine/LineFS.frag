#version 450
#extension GL_ARB_separate_shader_objects : enable
                 

//传入线的样式信息,目前仅包含线宽和颜色信息
layout(set=1, binding = 0) uniform LineStyle 
{
    vec4  LineColor;
    float Thickess;
} linestyle;

layout(location = 0) out vec4 outColor;

void main()
{
     //计算每个点的Normal，Normal为球的表面垂直向外，如果此normal和view的夹角在在-90-90度内则渲染，反之则Cull掉,简单来说就是CULL掉地球另一面的polyline
	//近似计算法线和view线的夹角
	//float angle=1.0/length(ubo.CameraEye);
	//if(dot(normalize(ubo.CameraEye),normal)<=angle) discard;
	//暂未考虑cull
	outColor=linestyle.LineColor;
}