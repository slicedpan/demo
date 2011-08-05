float4x4 World;
float4x4 View;
float4x4 Projection;

texture lightBuffer;
sampler lightBufferSampler = sampler_state
{
	Texture = (lightBuffer);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 pos : POSITION0;
	float4 Position : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output; 
	output.Position = input.Position;   
    output.pos = input.Position;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.Position.xy / input.Position.w;
	coords.xy += float2(1.0f, 1.0f);
	coords *= 0.5f;
	coords.y *= -1;
    return float4(tex2D(lightBufferSampler, coords).xyz * 100.0f, 1.0f);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
