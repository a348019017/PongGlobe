#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable
#extension GL_EXT_geometry_shader : enable

//Line的GeometryShader，主要是绘制线宽，参考https://github.com/paulhoux/Cinder-Samples/blob/master/GeometryShader/assets/shaders/lines2.geom                

//传入LineStrip_adjacency
layout( lines_adjacency ) in;
//传出Triangle
layout( triangle_strip, max_vertices = 7 ) out;

layout(location = 0) in vec3[] worldPosition;


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


void main()
{
    float width=5.0f;
   //在shader中提前cull到地球反面线，（以及根据当前分辨率，地球曲率筛选合适的点）
   if(!isEyeEarthCull(worldPosition[1])&&!isEyeEarthCull(worldPosition[2]))
    return;

  vec4 p0 = toScreenSpace(gl_in[0].gl_Position) ;	 //start of previous segment
  vec4 p1 = toScreenSpace(gl_in[1].gl_Position) ;	 //end of previous segment, start of current segment
  vec4 p2 = toScreenSpace(gl_in[2].gl_Position) ;	 //end of current segment, start of next segment
  vec4 p3 = toScreenSpace(gl_in[3].gl_Position) ;	 //end of next segment

  //如果p1 p2挨得太近，譬如整数位相同就不计算了,//相当于通过精度（当前分辨率）,这里也存在一个可能的bug，如果所有点都是精度都很高，那么就画不上了，因此还是需要采用间隔式选点的方式
  if(abs(p1.x-p2.x)<1.0f&&abs(p1.y-p2.y)<1.0f) return;
  //当p0和p1的距离相近，或者p2p3距离相近时造成normal计算不准确，此时p0和p3采取内插的方式
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
  //对地球另一面的顶点进行裁切，看能否解决地球边缘线的问题
  //对超过视口的线进行裁剪
  //如果两个点都在视口外，裁剪他
//  if(!isInViewPort(p1)&&!isInViewPort(p2)) return;
//  if(!isInViewPort(p1))
   
    vec2 v0 = normalize( (p1 - p0).xy );
	vec2 v1 = normalize( (p2 - p1).xy );
	vec2 v2 = normalize( (p3 - p2).xy );

	// determine the normal of each of the 3 segments (previous, current, next)
	vec2 n0 = vec2( -v0.y, v0.x );
	vec2 n1 = vec2( -v1.y, v1.x );
	vec2 n2 = vec2( -v2.y, v2.x );

	// determine miter lines by averaging the normals of the 2 segments
	vec2 miter_a = normalize( n0 + n1 );	// miter at start of current segment
	vec2 miter_b = normalize( n1 + n2 );	// miter at end of current segment

	// determine the length of the miter by projecting it onto normal and then inverse it
	float length_a = width / dot( miter_a, n1 );
	float length_b = width / dot( miter_b, n1 );

	// prevent excessively long miters at sharp corners
	if( dot( v0, v1 ) < -MITER_LIMIT ) {
		miter_a = n1;
		length_a = width;

		// close the gap
		if( dot( v0, n1 ) > 0 ) {
			VertexOut.mTexCoord = vec2( 0, 0 );
			VertexOut.mColor = VertexIn[1].mColor;
			gl_Position = vec4( ( p1 + THICKNESS * n0 ) / WIN_SCALE, 0.0, 1.0 );
			EmitVertex();

			VertexOut.mTexCoord = vec2( 0, 0 );
			VertexOut.mColor = VertexIn[1].mColor;
			gl_Position = vec4( ( p1 + THICKNESS * n1 ) / WIN_SCALE, 0.0, 1.0 );
			EmitVertex();

			VertexOut.mTexCoord = vec2( 0, 0.5 );
			VertexOut.mColor = VertexIn[1].mColor;
			gl_Position = vec4( p1 / WIN_SCALE, 0.0, 1.0 );
			EmitVertex();

			EndPrimitive();
		}
		else {
			VertexOut.mTexCoord = vec2( 0, 1 );
			VertexOut.mColor = VertexIn[1].mColor;
			gl_Position = vec4( ( p1 - THICKNESS * n1 ) / WIN_SCALE, 0.0, 1.0 );
			EmitVertex();

			VertexOut.mTexCoord = vec2( 0, 1 );
			VertexOut.mColor = VertexIn[1].mColor;
			gl_Position = vec4( ( p1 - THICKNESS * n0 ) / WIN_SCALE, 0.0, 1.0 );
			EmitVertex();

			VertexOut.mTexCoord = vec2( 0, 0.5 );
			VertexOut.mColor = VertexIn[1].mColor;
			gl_Position = vec4( p1 / WIN_SCALE, 0.0, 1.0 );
			EmitVertex();

			EndPrimitive();
		}
	}

	if( dot( v1, v2 ) < -MITER_LIMIT ) {
		miter_b = n1;
		length_b = THICKNESS;
	}

	// generate the triangle strip
	VertexOut.mTexCoord = vec2( 0, 0 );
	VertexOut.mColor = VertexIn[1].mColor;
	gl_Position = vec4( ( p1 + length_a * miter_a ) / WIN_SCALE, 0.0, 1.0 );
	EmitVertex();

	VertexOut.mTexCoord = vec2( 0, 1 );
	VertexOut.mColor = VertexIn[1].mColor;
	gl_Position = vec4( ( p1 - length_a * miter_a ) / WIN_SCALE, 0.0, 1.0 );
	EmitVertex();

	VertexOut.mTexCoord = vec2( 0, 0 );
	VertexOut.mColor = VertexIn[2].mColor;
	gl_Position = vec4( ( p2 + length_b * miter_b ) / WIN_SCALE, 0.0, 1.0 );
	EmitVertex();

	VertexOut.mTexCoord = vec2( 0, 1 );
	VertexOut.mColor = VertexIn[2].mColor;
	gl_Position = vec4( ( p2 - length_b * miter_b ) / WIN_SCALE, 0.0, 1.0 );
	EmitVertex();

	EndPrimitive();
	
}