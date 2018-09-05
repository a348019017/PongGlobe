#version 450
#extension GL_ARB_separate_shader_objects : enable
// vertex shader



layout(location = 0) in vec2 position;

out gl_PerVertex {
    vec4 gl_Position;
};

void main()                     
{
   //当前glposition为屏幕坐标
    gl_Position =  vec4(position,1.0,1.0);    
	gl_Position.y=-gl_Position.y;
}