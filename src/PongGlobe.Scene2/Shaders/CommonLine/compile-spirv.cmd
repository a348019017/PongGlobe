@echo off

glslangvalidator -V -S vert LineVS.glsl -o LineVS.spv
glslangvalidator -V -S frag LineFS.glsl -o LineFS.spv


echo ��������˳� & pause
exit 