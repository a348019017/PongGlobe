@echo off

glslangvalidator -V -S vert PointVS.glsl -o PointVS.spv
glslangvalidator -V -S frag PointFS.glsl -o PointFS.spv


echo ��������˳� & pause
exit 