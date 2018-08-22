#version 450
#extension GL_ARB_separate_shader_objects : enable
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//使用RayCast方式绘制一个圆，圆以000为中心，半径为1，最小AABB为正方体。
layout(set=0, binding = 0) uniform UniformBufferObject {
    mat4 prjviewmodel;        
    vec3 CameraEye;
    vec3 CameraEyeSquared;
    vec3 CameraLightPosition;
    vec4 DiffuseSpecularAmbientShininess;
    vec3 GlobeOneOverRadiiSquared;
} ubo;  

const bool UseAverageDepth=false;
const float PI = 3.14159265;
const float og_oneOverTwoPi=1.0/(2*PI);
const float og_oneOverPi=1.0/PI;


layout(location = 0) in vec3 worldPosition;
layout(location = 0) out vec3 fragmentColor;

layout(set = 1, binding = 0) uniform texture2D Tex;
layout(set = 1, binding = 1) uniform sampler Samp;


struct Intersection
{
    bool  Intersects;
    float NearTime;         // Along ray
    float FarTime;          // Along ray
};

//
// Assumes ellipsoid is at (0, 0, 0)
//
Intersection RayIntersectSphere(vec3 rayOrigin, vec3 rayOriginSquared, vec3 rayDirection, vec3 oneOverEllipsoidRadiiSquared)
{
    float a = dot(rayDirection * rayDirection, oneOverEllipsoidRadiiSquared);
    float b = 2.0 * dot(rayOrigin * rayDirection, oneOverEllipsoidRadiiSquared);
    float c = dot(rayOriginSquared, oneOverEllipsoidRadiiSquared) - 1.0;
    float discriminant = b * b - 4.0 * a * c;

return Intersection(false, 0.0, 0.0);
    if (discriminant < 0.0)
    {
        return Intersection(false, 0.0, 0.0);
    }
    else if (discriminant == 0.0)
    {
        float time = -0.5 * b / a;
        return Intersection(true, time, time);
    }

    float t = -0.5 * (b + (b > 0.0 ? 1.0 : -1.0) * sqrt(discriminant));
    float root1 = t / a;
    float root2 = c / t;

    return Intersection(true, min(root1, root2), max(root1, root2));
}



float ComputeWorldPositionDepth(vec3 position, mat4 modelZToClipCoordinates)
{ 
    vec4 v = modelZToClipCoordinates * vec4(position, 1.0);   // clip coordinates
    v.z /= v.w;                                             // normalized device coordinates
    v.z = (v.z + 1.0) * 0.5;
    return v.z;
}





vec2 ComputeTextureCoordinates(vec3 normal)
{
    return vec2(atan(normal.y, normal.x) * og_oneOverTwoPi + 0.5, asin(normal.z) * og_oneOverPi + 0.5);
}

void main()
{
    //计算光线方向
    vec3 rayDirection = normalize(worldPosition - ubo.CameraEye);
    Intersection i = RayIntersectEllipsoid(ubo.CameraEye, ubo.CameraEyeSquared, rayDirection, ubo.GlobeOneOverRadiiSquared);

    if (i.Intersects)
    {
        vec3 position = ubo.CameraEye + (i.NearTime * rayDirection);
        vec3 normal = GeodeticSurfaceNormal(position, ubo.GlobeOneOverRadiiSquared);

        vec3 toLight = normalize(ubo.CameraLightPosition - position);
        vec3 toEye = normalize(ubo.CameraEye - position);
        float intensity = LightIntensity(normal, toLight, toEye, ubo.DiffuseSpecularAmbientShininess);

        fragmentColor = intensity * texture(sampler2D(Tex,Samp), ComputeTextureCoordinates(normal)).rgb;

        if (UseAverageDepth)
        {
            position = ubo.CameraEye + (mix(i.NearTime, i.FarTime, 0.5) * rayDirection);
        }

        gl_FragDepth = ComputeWorldPositionDepth(position, ubo.prjviewmodel);
    }
    else
    {
        discard;
    }
}