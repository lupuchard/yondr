#version 130

uniform sampler2D tex;

varying vec2 texcoord;

void main() {
	vec4 color = texture(tex, texcoord);

	gl_FragColor = color;
	//gl_FragColor = vec4(texcoord.x, texcoord.y, 1.0, 1.0);
	//gl_FragColor = vec4(1.0, 1.0, 1.0, 1.0);
}
