@echo off

glslangvalidator -V -S vert LineVS.vert -o LineVS.spv
glslangvalidator -V -S frag LineFS.frag -o LineFS.spv
glslangvalidator -V -S geom LineGS.geom -o LineGS.spv

echo ��������˳� & pause
exit 