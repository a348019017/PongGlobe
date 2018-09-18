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


 float outline_thickness = .2;
 vec3 outline_colour = vec3(0, 0, 1);
 float outline_threshold = .5;
 sampler2D t0=sampler2D(Tex,Samp);

//�������ݲ����ǹ���
void main()
{	

    
   //������������ɫ
   vec4 color=texture(, textureCoordinate);
//   if(color.a==0.0)
//   {
//      discard;
//   }
   if (color.a <= outline_threshold) {
       
        ivec2 size = textureSize(sampler2D(Tex,Samp), 0);

        float uv_x = textureCoordinate.x * size.x;
        float uv_y = textureCoordinate.y * size.y;

        float sum = 0.0;
        for (int n = 0; n < 9; ++n) {
            uv_y = (textureCoordinate.y * size.y) + (outline_thickness * float(n - 4.5));
            float h_sum = 0.0;
            h_sum += texelFetch(t0, ivec2(uv_x - (4.0 * outline_thickness), uv_y), 0).a;
            h_sum += texelFetch(t0, ivec2(uv_x - (3.0 * outline_thickness), uv_y), 0).a;
            h_sum += texelFetch(t0, ivec2(uv_x - (2.0 * outline_thickness), uv_y), 0).a;
            h_sum += texelFetch(t0, ivec2(uv_x - outline_thickness, uv_y), 0).a;
            h_sum += texelFetch(t0, ivec2(uv_x, uv_y), 0).a;
            h_sum += texelFetch(t0, ivec2(uv_x + outline_thickness, uv_y), 0).a;
            h_sum += texelFetch(t0, ivec2(uv_x + (2.0 * outline_thickness), uv_y), 0).a;
            h_sum += texelFetch(t0, ivec2(uv_x + (3.0 * outline_thickness), uv_y), 0).a;
            h_sum += texelFetch(t0, ivec2(uv_x + (4.0 * outline_thickness), uv_y), 0).a;
            sum += h_sum / 9.0;
        }

        if (sum / 9.0 >= 0.0001) {
            color = vec4(outline_colour, 1);
        }
    }
    fragmentColor = color;
	//fragmentColor = vec4(style.pointColor.rgb, style.pointColor.a);
}