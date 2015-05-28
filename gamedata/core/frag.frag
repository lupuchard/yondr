#version 130

uniform sampler2D tex;

in vec2 texcoord;

void main() {
	vec4 color = texture(tex, texcoord);

	// gl_FragColor = color;
	gl_FragColor = vec4(1.0, 1.0, 1.0, 1.0);
}
