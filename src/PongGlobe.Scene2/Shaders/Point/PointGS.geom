#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable
#extension GL_EXT_geometry_shader : enable

//Line��GeometryShader����Ҫ�ǻ����߿��ο�https://github.com/paulhoux/Cinder-Samples/blob/master/GeometryShader/assets/shaders/lines2.geom                

//�����Ķ���
layout( points ) in;
//����Triangle��һ���ı���
layout( triangle_strip, max_vertices = 4 ) out;

//layout(location = 0) in vec3[] worldPosition;
//���������UV����
layout(location = 0) out vec2 fsTextureCoordinates;

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

//��������Ϣ������Ҫ�أ�����Ҫ�ؿ����е��ͻ������Ҫ����
//���Ƿ���shaderʵ�־���ķ������������������ľ�����ڽ��ˣ�������еķ�����CPU���࣬Ȼ��GPU�Ƚ�����ĵ㣬Ȼ�����Ⱦ����һ���㣬����˼·��3d������Ի�һ���з��� �޳��ڵ� OcclusionCulling

void main()
{
//32x32�����ش�С
     float size=32/2;
    //��ȡ��һ������Ϊ�е�
    vec4 center = gl_in[0].gl_Position;
	//�������ӿ�����
	vec4 centerWin=toScreenSpace(center);
	//�����ĸ��������꣬��˳ʱ���Ų���ʱ��UV����ϵ��viewport�ο�ϵ��һ�µģ��������Ͻ���0��0�����½����
	//���½Ƕ���
	vec4 center1=centerWin+vec4(-size,size,0,0);
	//���ϽǶ���
	vec4 center2=centerWin+vec4(-size,-size,0,0);
	//���ϽǶ���
	vec4 center3=centerWin+vec4(size,-size,0,0);
	//���½Ƕ���
	vec4 center4=centerWin+vec4(size,size,0,0);

	//cull������ͼ��ĵ�
	if(center2.x<0||center2.y<0||center4.x>ubo.viewport.x||center4.y>ubo.viewport.y) return;	

	gl_Position=ToNDCSpace(center2);
	fsTextureCoordinates=vec2(0,0);
	EmitVertex();	
	gl_Position=ToNDCSpace(center3);
	fsTextureCoordinates=vec2(1,0);
	EmitVertex();	
	gl_Position=ToNDCSpace(center1);
	fsTextureCoordinates=vec2(0,1);
	EmitVertex();	
	gl_Position=ToNDCSpace(center4);
	fsTextureCoordinates=vec2(1,1);
	EmitVertex();
	EndPrimitive();
}