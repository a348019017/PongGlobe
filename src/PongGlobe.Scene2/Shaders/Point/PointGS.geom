#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable
#extension GL_EXT_geometry_shader : enable

//Line的GeometryShader，主要是绘制线宽，参考https://github.com/paulhoux/Cinder-Samples/blob/master/GeometryShader/assets/shaders/lines2.geom                

//传入点的顶点
layout( points ) in;
//传出Triangle，一个四边形
layout( triangle_strip, max_vertices = 4 ) out;

//layout(location = 0) in vec3[] worldPosition;
//传出顶点的UV坐标
layout(location = 0) out vec2 fsTextureCoordinates;

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
   return vec4(vertex.xy/(ubo.viewport/2.0f)-1.0f,vertex.z,vertex.w);
}


vec3 GeodeticSurfaceNormal(vec3 positionOnEllipsoid, vec3 oneOverEllipsoidRadiiSquared)
{
    return normalize(positionOnEllipsoid * oneOverEllipsoidRadiiSquared);
}

//判断点是否在视口内,返回与视口的交点，和在视口内的点。如果两个点都在视口内正常返回两个点，注意返回点的顺序
bool isInViewPort(vec4 Vpos)
{
   if(Vpos.x<0||Vpos.x>ubo.viewport.x) return false;
   if(Vpos.y<0||Vpos.y>ubo.viewport.y) return false;
   return true;
}

bool isEyeEarthCull(vec3 pos)
{
   //过滤掉所有不满足需求的点
	vec3 posToEye=ubo.CameraEye-pos;
	vec3 posNormal=GeodeticSurfaceNormal(pos,ubo.GlobeOneOverRadiiSquared);
	//如果position在当前的EarthCull内
	if(dot(posToEye,posNormal)<0) return false;
	return true;
}

//点的深度信息和其它要素，如线要素可能有点冲突可能需要调整
//看是否有shader实现聚类的方法，如果任意两个点的距离过于近了，这里可行的方法是CPU聚类，然后GPU比较相近的点，然后进渲染其中一个点，这种思路在3d里面可以换一个叫法是 剔除遮挡 OcclusionCulling

void main()
{
//32x32个像素大小
     float size=32/2;
    //获取第一个点作为中点
    vec4 center = gl_in[0].gl_Position;
	//计算其视口坐标
	vec4 centerWin=toScreenSpace(center);
	//计算四个顶点坐标，按顺时针排布此时的UV坐标系和viewport参考系是一致的，均是左上角是0，0，右下角最大
	//左下角顶点
	vec4 center1=centerWin+vec4(-size,size,0,0);
	//左上角顶点
	vec4 center2=centerWin+vec4(-size,-size,0,0);
	//右上角顶点
	vec4 center3=centerWin+vec4(size,-size,0,0);
	//右下角顶点
	vec4 center4=centerWin+vec4(size,size,0,0);

	//cull掉在视图外的点
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