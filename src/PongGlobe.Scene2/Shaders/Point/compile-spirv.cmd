@echo off

glslangvalidator -V -S vert PointVS.vert -o PointVS.spv
glslangvalidator -V -S frag PointFS.frag -o PointFS.spv
glslangvalidator -V -S geom PointGS.geom -o PointGS.spv

echo ��������˳� & pause
exit 