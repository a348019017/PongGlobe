#version 450
#extension GL_ARB_separate_shader_objects : enable
                 


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

layout(set=1, binding = 1) uniform PolygonStyle 
{
    vec4 fillColor;           	
} style;


layout(location = 0) in vec3 worldPosition;
layout(location = 1) in vec3 positionToLight;
layout(location = 2) in vec3 positionToEye;
layout(location = 0) out vec4 fragmentColor;




float LightIntensity(vec3 normal, vec3 toLight, vec3 toEye, vec4 diffuseSpecularAmbientShininess)
{
    vec3 toReflectedLight = reflect(-toLight, normal);

    float diffuse = max(dot(toLight, normal), 0.0);
    float specular = max(dot(toReflectedLight, toEye), 0.0);
    specular = pow(specular, diffuseSpecularAmbientShininess.w);

    return (diffuseSpecularAmbientShininess.x * diffuse) +
            (diffuseSpecularAmbientShininess.y * specular) +
            diffuseSpecularAmbientShininess.z;
}

vec3 GeodeticSurfaceNormal(vec3 positionOnEllipsoid, vec3 oneOverEllipsoidRadiiSquared)
{
    return normalize(positionOnEllipsoid * oneOverEllipsoidRadiiSquared);
}

void main()
{
	vec3 normal = GeodeticSurfaceNormal(worldPosition, ubo.GlobeOneOverRadiiSquared);
    float intensity = LightIntensity(normal,  normalize(positionToLight), normalize(positionToEye), ubo.DiffuseSpecularAmbientShininess);

	fragmentColor = vec4(intensity * style.fillColor.rgb, style.fillColor.a);
}