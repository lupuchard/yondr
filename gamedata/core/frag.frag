#version 130

uniform sampler2D tex;

in vec2 texcoord;
in vec3 position;
in vec3 normal;

void main() {
	vec4 color = texture(tex, texcoord);

	gl_FragColor = color;
}
