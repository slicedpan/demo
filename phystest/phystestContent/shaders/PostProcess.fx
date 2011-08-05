float4x4 World;
float4x4 View;
float4x4 Projection;

float invScreenHeight;
float invScreenWidth;
float mix;

// TODO: add effect parameters here.

texture inputtexture;
sampler inputSampler = sampler_state
{
	Texture = (inputtexture);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture edgemask;
sampler edgeSampler = sampler_state
{
	Texture = (edgemask);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = NONE;
	AddressU = Clamp;
	AddressV = Clamp;
};

float3 SmoothPixelVertical(float3 basergb, float2 curCoord)
{
	float2 offset = float2(0, invScreenHeight);

	float3 avg = tex2D(inputSampler, curCoord + offset).rgb * 0.2218;
	avg += tex2D(inputSampler, curCoord - offset).rgb * 0.2218;

	avg += tex2D(inputSampler, curCoord + (2 * offset)).rgb * 0.0266;	
	avg += tex2D(inputSampler, curCoord - (2 * offset)).rgb * 0.0266;

	avg += basergb * 0.5;

	return avg;
}

float3 SmoothPixelHorizontal(float3 basergb, float2 curCoord)
{
	float2 offset = float2(invScreenWidth, 0);	

	float3 avg = tex2D(inputSampler, curCoord + offset).rgb * 0.2218;
	avg += tex2D(inputSampler, curCoord - offset).rgb * 0.2218;

	avg += tex2D(inputSampler, curCoord + (2 * offset)).rgb * 0.0266;	
	avg += tex2D(inputSampler, curCoord - (2 * offset)).rgb * 0.0266;

	avg += basergb * 0.5;

	return avg;
}

struct VertexShaderParams
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderParams VertexShaderFunction(VertexShaderParams input)
{
    return input;
}

float4 HorizontalBlurPS(VertexShaderParams input) : COLOR0
{
	half edge = tex2D(edgeSampler, input.TexCoord);
	float4 base = tex2D(inputSampler, input.TexCoord); 
	if (edge > 0.2f)
	{
		base.rgb = SmoothPixelHorizontal(base.rgb, input.TexCoord);
	}
	return base; 
}

float4 VerticalBlurPS(VertexShaderParams input) : COLOR0
{
	half edge = tex2D(edgeSampler, input.TexCoord);
	float4 base = tex2D(inputSampler, input.TexCoord);
	if (edge > 0.2f)
	{
		base.rgb = SmoothPixelVertical(base.rgb, input.TexCoord);
	}
	return base;
}

technique EdgeBlur
{
    pass Horizontal
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 HorizontalBlurPS();
    }
	pass Vertical
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 VerticalBlurPS();	
	}
}
