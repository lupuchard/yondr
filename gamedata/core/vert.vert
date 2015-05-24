#version 130

in vec3 vPosition;
in vec3 vNormal;
in vec2 vTexcoord;

uniform mat4 mvpMatrix;
uniform sampler2D tex;

/* View vector */
out vec2 texcoord;
out vec3 position;
out vec3 normal;

void main() {
	texcoord = vTexcoord;
	position = vPosition;
	normal   = vNormal;

	gl_Position = mvpMatrix * vec4(vPosition.xyz, 1.0);
}
