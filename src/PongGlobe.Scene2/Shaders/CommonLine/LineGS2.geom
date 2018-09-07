#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable
#extension GL_EXT_geometry_shader : enable

//Line��GeometryShader����Ҫ�ǻ����߿��ο�https://github.com/paulhoux/Cinder-Samples/blob/master/GeometryShader/assets/shaders/lines2.geom                

//����LineStrip_adjacency
layout( lines_adjacency ) in;
//����Triangle
layout( triangle_strip, max_vertices = 7 ) out;

layout(location = 0) in vec3[] worldPosition;


//������Ӵ����viewToViewPort��ת������,������ʱ
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



//View����ת��Ϊ��Ļ���꣬�������Ļ�������Ͻ�ԭ��
 vec4 toScreenSpace(vec4 vertex) 
 { 
  	return vec4((vertex.xy / vertex.w) * (ubo.viewport/2.0f)+ubo.viewport/2.0f,vertex.z/vertex.w,1.0f); 
 } 

 //��Ļ����תNDC���꣬������Ҫ���������Ϣ��ԭʼ����ͬ��,�����Ѿ���λ�Ϊ1
vec4 ToNDCSpace(vec4 vertex)
{
   return vec4(vertex.xy/(ubo.viewport/2.0f)-1.0f,vertex.z,vertex.w);
}


vec3 GeodeticSurfaceNormal(vec3 positionOnEllipsoid, vec3 oneOverEllipsoidRadiiSquared)
{
    return normalize(positionOnEllipsoid * oneOverEllipsoidRadiiSquared);
}

//�жϵ��Ƿ����ӿ���,�������ӿڵĽ��㣬�����ӿ��ڵĵ㡣��������㶼���ӿ����������������㣬ע�ⷵ�ص��˳��
bool isInViewPort(vec4 Vpos)
{
   if(Vpos.x<0||Vpos.x>ubo.viewport.x) return false;
   if(Vpos.y<0||Vpos.y>ubo.viewport.y) return false;
   return true;
}

bool isEyeEarthCull(vec3 pos)
{
   //���˵����в���������ĵ�
	vec3 posToEye=ubo.CameraEye-pos;
	vec3 posNormal=GeodeticSurfaceNormal(pos,ubo.GlobeOneOverRadiiSquared);
	//���position�ڵ�ǰ��EarthCull��
	if(dot(posToEye,posNormal)<0) return false;
	return true;
}


void main()
{
    float width=5.0f;
   //��shader����ǰcull���������ߣ����Լ����ݵ�ǰ�ֱ��ʣ���������ɸѡ���ʵĵ㣩
   if(!isEyeEarthCull(worldPosition[1])&&!isEyeEarthCull(worldPosition[2]))
    return;

  vec4 p0 = toScreenSpace(gl_in[0].gl_Position) ;	 //start of previous segment
  vec4 p1 = toScreenSpace(gl_in[1].gl_Position) ;	 //end of previous segment, start of current segment
  vec4 p2 = toScreenSpace(gl_in[2].gl_Position) ;	 //end of current segment, start of next segment
  vec4 p3 = toScreenSpace(gl_in[3].gl_Position) ;	 //end of next segment

  //���p1 p2����̫����Ʃ������λ��ͬ�Ͳ�������,//�൱��ͨ�����ȣ���ǰ�ֱ��ʣ�,����Ҳ����һ�����ܵ�bug��������е㶼�Ǿ��ȶ��ܸߣ���ô�ͻ������ˣ���˻�����Ҫ���ü��ʽѡ��ķ�ʽ
  if(abs(p1.x-p2.x)<1.0f&&abs(p1.y-p2.y)<1.0f) return;
  //��p0��p1�ľ������������p2p3�������ʱ���normal���㲻׼ȷ����ʱp0��p3��ȡ�ڲ�ķ�ʽ
  if(distance(p0.xy,p1.xy)==0)
  {
     p0=vec4(2*p1.xy-p2.xy,p1.w,1);
  }
  if(distance(p2.xy,p3.xy)==0)
  {
      p3=vec4(2*p2.xy-p1.xy,p2.w,1);
  }
   //perform naive culling
  vec2 area = ubo.viewport * 1.0f;
  if(!isInViewPort(p1)&&!isInViewPort(p2)) return;
  //�Ե�����һ��Ķ�����в��У����ܷ��������Ե�ߵ�����
  //�Գ����ӿڵ��߽��вü�
  //��������㶼���ӿ��⣬�ü���
//  if(!isInViewPort(p1)&&!isInViewPort(p2)) return;
//  if(!isInViewPort(p1))
   
  // determine the direction of each of the 3 segments (previous, current, next)
  vec2 v0 = normalize((p1-p0).xy);
  vec2 v1 = normalize((p2-p1).xy);
  vec2 v2 = normalize((p3-p2).xy);

  // determine the normal of each of the 3 segments (previous, current, next)
  vec2 n0 = vec2(-v0.y, v0.x);
  vec2 n1 = vec2(-v1.y, v1.x);
  vec2 n2 = vec2(-v2.y, v2.x);

  // determine miter lines by averaging the normals of the 2 segments
  if(distance(n0+n1,vec2(0,0))==0)
   return;
   if(distance(n1+n2,vec2(0,0))==0)
   return;
  vec2 miter_a = normalize(n0 + n1);	// miter at start of current segment
  vec2 miter_b = normalize(n1 + n2);	// miter at end of current segment

  // determine the length of the miter by projecting it onto normal and then inverse it
  //����length���ܼ�����ܴ��ֵ
  float length_a = width / dot(miter_a, n1);
  float length_b = width / dot(miter_b, n1);
  //����������һ�㲻������߿������������㷨����������ǵ�p0��p2�ܽӽ�ʱ���α䣬��������µ���Ⱦ��ò�Ҫ��ʧ��
  if(length_a>5*width) length_a=5*width;
  if(length_b>5*width)length_b=5*width;

  //˳ʱ��������
  if( dot(v0,n1) > 0 ) {      
	gl_Position = ToNDCSpace(vec4(p1.xy - length_a * miter_a ,  p1.z, 1.0 ));
	EmitVertex();
	// proceed to positive normal
   
	gl_Position = ToNDCSpace(vec4((p1.xy +  width * n1) , p1.z, 1.0));
	EmitVertex();
 }
 else { 
   
    //���ﷴ������ʱ��������Ժ���ʧ��
	gl_Position =  ToNDCSpace(vec4((p1.xy -  width * n1) ,  p1.z, 1.0) );
	EmitVertex();
	
	gl_Position =  ToNDCSpace(vec4((p1.xy + length_a * miter_a) ,  p1.z, 1.0 ));
	EmitVertex();
  } 
  
  ///˳ʱ��
  if( dot(v2,n1) < 0 ) {
	// proceed to negative miter
   
	gl_Position = ToNDCSpace(vec4(p2.xy - length_b * miter_b , p2.z, 1.0 ));
	EmitVertex();
	
	gl_Position = ToNDCSpace(vec4(p2.xy +  width * n1 , p2.z, 1.0 ));
	EmitVertex();
	
	gl_Position = ToNDCSpace(vec4(p2.xy +  width * n2 ,p2.z, 1.0 ));
	EmitVertex();
  }
  else { 
    vec4 tmp=   vec4(p2.xy -  width * n1 ,p2.z , 1.0);
	gl_Position = ToNDCSpace(tmp);
	EmitVertex();
	// proceed to positive miter
	gl_Position =  ToNDCSpace(vec4(p2.xy + length_b * miter_b,p2.z, 1.0 ));
	EmitVertex();
	// end at negative normal
	gl_Position = ToNDCSpace(vec4( p2.xy -  width * n2 , p2.z, 1.0) );
	EmitVertex();
  }
  EndPrimitive();

	
}