@echo off

glslangvalidator -V -S vert PolygonVS.glsl -o PolygonVS.spv
glslangvalidator -V -S frag PolygonFS.glsl -o PolygonFS.spv


echo ��������˳� & pause
exit 