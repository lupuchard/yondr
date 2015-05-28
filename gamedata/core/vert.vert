#version 130

in vec3 vPosition;
in vec3 vNormal;
in vec2 vTexcoord;

uniform mat4 mvpMatrix;
uniform mat4  spMatrix;
uniform sampler2D tex;

out vec2 texcoord;

void main() {
	texcoord = vTexcoord;

	gl_Position = mvpMatrix * spMatrix * vec4(vPosition.xyz, 1.0);
}
