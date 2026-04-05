#version 330 core
out vec4 FragColor;
in vec2 vTexCoord;

uniform sampler2D uTexture;
uniform float uAlphaTest = 0.1;

void main()
{
    vec4 texColor = texture(uTexture, vTexCoord);
    if (texColor.a < uAlphaTest)
        discard;
    FragColor = texColor;
}