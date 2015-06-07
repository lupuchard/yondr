#version 130

attribute vec3 vPosition;
attribute vec2 vTexcoord;

uniform mat4 mvpMatrix;
uniform mat4  spMatrix;
uniform sampler2D tex;

varying vec2 texcoord;

void main() {
	texcoord = vTexcoord;

	//gl_Position = mvpMatrix * spMatrix * vec4(vPosition.xyz, 1.0);
	gl_Position = vec4(vPosition.xyz, 1.0);
}
