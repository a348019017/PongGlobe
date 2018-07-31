@echo off

glslangvalidator -V -S vert earth.vert -o Vertex.spv
glslangvalidator -V -S frag earth.frag -o Fragment.spv

glslangvalidator -V -S vert GlobeVS.glsl -o GlobeVS.spv
glslangvalidator -V -S frag GlobeFS.glsl -o GlobeFS.spv

echo 按任意键退出 & pause
exit 