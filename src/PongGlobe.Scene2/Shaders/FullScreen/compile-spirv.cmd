@echo off

glslangvalidator -V -S vert FullScreenVS.vert -o FullScreenVS.spv
glslangvalidator -V -S frag FullScreenFS.frag -o FullScreenFS.spv

echo ��������˳� & pause
exit 