#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable
#extension GL_EXT_geometry_shader : enable

//Line的GeometryShader，主要是绘制线宽，参考https://github.com/paulhoux/Cinder-Samples/blob/master/GeometryShader/assets/shaders/lines2.geom                

//传入LineStrip_adjacency
layout( lines_adjacency ) in;
//传出Triangle
layout( triangle_strip, max_vertices = 7 ) out;



//额外添加传入的viewToViewPort的转换矩阵,这里暂时
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

 layout(set=1, binding = 0) uniform LineStyle 
{
    vec4  LineColor;
    float Thickess;
} linestyle;



//View坐标转换为屏幕坐标，这里的屏幕坐标左上角原点
 vec4 toScreenSpace(vec4 vertex) 
 { 
  	return vec4((vertex.xy / vertex.w) * (ubo.viewport/2.0f)+ubo.viewport/2.0f,vertex.z/vertex.w,1.0f); 
 } 

 //屏幕坐标转NDC坐标，这里需要保持深度信息和原始是相同的,这里已经其次话为1
vec4 ToNDCSpace(vec4 vertex)
{
   return vec4(vertex.xy/(ubo.viewport/2.0f)-1.0f,vertex.z,1.0f);
}

void main()
{
     vec4 p1=gl_in[1].gl_Position;
	 vec4 p1w=toScreenSpace(p1);
	 vec4 p2=gl_in[2].gl_Position;
	 vec4 p2w=toScreenSpace(p2);
	 vec2 offset=vec2(20,20);
	 //计算其向量
	 //vec2 v1 = normalize(p2w.xy-p1w.xy);
	 //这里存在一个精度的问题，假设p2.xy的精度已经很低了可能4-5位的精度，一旦乘法或触发之后，

	 vec2 v0=  (p2.xy*vec2(640,360)/p2.w)-(p1.xy
	 *vec2(640,360)/p1.w);
	  vec2 v1=normalize(p2.xy-p1.xy);
	 if(v0.x<1||v0.y<1||v0.x==0||v0.y==0)
	 {
	   return;
	 }
	
	 //if(ubo.viewport.xy!=vec2(640,360))return;
	 
     gl_Position=ToNDCSpace(vec4(p1w.xy-offset*v1,p1w.z,1));
     EmitVertex();
	 gl_Position=ToNDCSpace(vec4(p1w.xy+offset*v1,p1w.z,1));
	 EmitVertex();
	 
	 gl_Position=ToNDCSpace(vec4(p2w.xy-offset*v1,p1w.z,1));
	 EmitVertex();
	 gl_Position=ToNDCSpace(vec4(p2w.xy+offset*v1,p1w.z,1));
	 EmitVertex();
	 EndPrimitive();   


//	// get the four vertices passed to the shader:
//  vec4 p0 = gl_in[0].gl_Position ;	// start of previous segment
//  vec4 p1 = gl_in[1].gl_Position ;	// end of previous segment, start of current segment
//  vec4 p2 = gl_in[2].gl_Position ;	// end of current segment, start of next segment
//  vec4 p3 = gl_in[3].gl_Position ;	// end of next segment
//
////  // perform naive culling
////  vec2 area = ubo.viewport * 1.2;
////  if( p1.x < -area.x || p1.x > area.x ) return;
////  if( p1.y < -area.y || p1.y > area.y ) return;
////  if( p2.x < -area.x || p2.x > area.x ) return;
////  if( p2.y < -area.y || p2.y > area.y ) return;
//  
//  // determine the direction of each of the 3 segments (previous, current, next)
//  vec2 v0 = normalize((p1-p0).xy);
//  vec2 v1 = normalize((p2-p1).xy);
//  vec2 v2 = normalize((p3-p2).xy);
//
//  // determine the normal of each of the 3 segments (previous, current, next)
//  vec2 n0 = vec2(-v0.y, v0.x);
//  vec2 n1 = vec2(-v1.y, v1.x);
//  vec2 n2 = vec2(-v2.y, v2.x);
//
//  // determine miter lines by averaging the normals of the 2 segments
//  vec2 miter_a = normalize(n0 + n1);	// miter at start of current segment
//  vec2 miter_b = normalize(n1 + n2);	// miter at end of current segment
//
//  // determine the length of the miter by projecting it onto normal and then inverse it
//  float length_a = linestyle.Thickess / dot(miter_a, n1);
//  float length_b = linestyle.Thickess / dot(miter_b, n1);
//
//  if( dot(v0,n1) > 0 ) {      
//	gl_Position = ToNDCSpace(vec4((p1.xy - length_a * miter_a) , 0, 1.0 ));
//	EmitVertex();
//	// proceed to positive normal
//   
//	gl_Position = ToNDCSpace(vec4((p1.xy +  linestyle.Thickess * n1) , 0, 1.0) );
//	EmitVertex();
// }
// else { 
//   
//    //这里反向计算的时候深度线性好像丢失了
//	gl_Position =  ToNDCSpace(vec4((p1.xy -  linestyle.Thickess * n1) , 0, 1.0) );
//	EmitVertex();
//	
//	gl_Position =  ToNDCSpace(vec4((p1.xy + length_a * miter_a) , 0, 1.0 ));
//	EmitVertex();
//  } 
//  
//  if( dot(v2,n1) < 0 ) {
//	// proceed to negative miter
//   
//	gl_Position = ToNDCSpace(vec4((p2.xy - length_b * miter_b) , 0, 1.0 ));
//	EmitVertex();
//	
//	gl_Position = ToNDCSpace(vec4((p2.xy +  linestyle.Thickess * n1) , 0, 1.0 ));
//	EmitVertex();
//	
//	gl_Position = ToNDCSpace(vec4((p2.xy +  linestyle.Thickess * n2) ,0, 1.0 ));
//	EmitVertex();
//  }
//  else { 
//   
//	gl_Position = ToNDCSpace(vec4((p2.xy -  linestyle.Thickess * n1) ,0, 1.0 ));
//	EmitVertex();
//	// proceed to positive miter
//	gl_Position =  ToNDCSpace(vec4((p2.xy + length_b * miter_b),0, 1.0 ));
//	EmitVertex();
//	// end at negative normal
//	gl_Position = ToNDCSpace(vec4(( p2.xy -  linestyle.Thickess * n2) , 0, 1.0) );
//	EmitVertex();
//  }
//  EndPrimitive();

	
}