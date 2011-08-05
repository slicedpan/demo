float4x4 World;
float4x4 View;
float4x4 Projection;

float invScreenHeight;
float invScreenWidth;
float mix;

const float MASKLEFT = 1.0f;
const float MASKTOPLEFT = 2.0f;
const float MASKTOP = 4.0f;

texture inputtexture;
sampler inputSampler = sampler_state
{
	Texture = <inputtexture>;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = POINT;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture depth;
sampler depthSampler = sampler_state
{
	Texture = (depth);
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture normal;
sampler normalSampler = sampler_state
{
	texture = (normal);
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderParams
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct PixelShaderOut
{
	float4 Color : COLOR0;
};

VertexShaderParams VertexShaderFunction(VertexShaderParams input)
{
	return input;
}

half4 EdgeDetectPS(VertexShaderParams input) : COLOR0
{
	half2 output = (half2)0;
	float curdepth = tex2D(depthSampler, input.TexCoord);

	float3 curnormal = tex2D(normalSampler, input.TexCoord);		
	float3 bnormal = tex2D(normalSampler, input.TexCoord + float2(0, 2 * invScreenHeight));
	float3 lnormal = tex2D(normalSampler, input.TexCoord + float2(2 * -invScreenWidth, 0));

	float bdepth = tex2D(depthSampler, input.TexCoord + float2(0, 2 * invScreenHeight));
	float tdepth = tex2D(depthSampler, input.TexCoord + float2(0, 2 * -invScreenHeight));
	float ldepth = tex2D(depthSampler, input.TexCoord + float2(2 * -invScreenWidth, 0));
	float rdepth = tex2D(depthSampler, input.TexCoord + float2(2 * invScreenWidth, 0));

	float sqdepth = sqrt(curdepth);

	output.x =  ((abs(curdepth - bdepth) / sqdepth) > 0.02f) +
				((abs(curdepth - tdepth) / sqdepth) > 0.02f) +
				((abs(curdepth - ldepth) / sqdepth) > 0.02f) +
				((abs(curdepth - rdepth) / sqdepth) > 0.02f);

	output.x /= 4.0f;

	output.y = (output.x * output.x);

	return half4(output, 1.0f, 1.0f);
}

technique EdgeDetect
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 EdgeDetectPS();
    }
}



