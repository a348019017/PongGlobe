@echo off



glslangvalidator -V -S vert GlobeVS.glsl -o GlobeVS.spv
glslangvalidator -V -S frag GlobeFS.glsl -o GlobeFS.spv

echo 按任意键退出 & pause
exit 