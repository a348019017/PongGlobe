@echo off

glslangvalidator -V -S vert FullScreenVS.vert -o FullScreenVS.spv
glslangvalidator -V -S frag FullScreenFS.frag -o FullScreenFS.spv

echo 按任意键退出 & pause
exit 