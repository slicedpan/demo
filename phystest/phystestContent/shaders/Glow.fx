float4x4 World;
float4x4 View;
float4x4 Projection;

texture lightBuffer;
float invScreenHeight;
float invScreenWidth;
float hdrPower;

sampler2D lightSampler = sampler_state
{
	Texture = (lightBuffer);
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = NONE;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderParams
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};


VertexShaderParams VertexShaderFunction(VertexShaderParams input)
{
    return input;
}

float3 SmoothPixel(float2 curCoord, bool vertical)
{
	float2 offset;
	if (vertical)
	{
		offset = float2(0, invScreenHeight);
	}
	else
	{
		offset = float2(invScreenWidth, 0);
	}

	float3 avg = tex2D(lightSampler, curCoord + offset).rgb * 0.150;
	avg += tex2D(lightSampler, curCoord + (2 * offset)).rgb * 0.250;
	avg += tex2D(lightSampler, curCoord - offset).rgb * 0.150;
	avg += tex2D(lightSampler, curCoord - (2 * offset)).rgb * 0.250;
	avg += tex2D(lightSampler, curCoord - (3 * offset)).rgb * 0.1;
	avg += tex2D(lightSampler, curCoord + (3 * offset)).rgb * 0.1;

	return avg;
}

float4 HorizontalBlurPS(VertexShaderParams input) : COLOR0
{  
	float4 base = tex2D(lightSampler, input.TexCoord);

	base.rgb = lerp(base.rgb, SmoothPixel(input.TexCoord, 0), 0.5f);

    return base;
}

float4 VerticalBlurPS(VertexShaderParams input) : COLOR0
{  
	float4 base = tex2D(lightSampler, input.TexCoord);	
	base.rgb = lerp(base.rgb, SmoothPixel(input.TexCoord, 1), 0.15f);		
    return base;
}

float4 SquareIntensity(VertexShaderParams input) : COLOR0
{
	float4 base = tex2D(lightSampler, input.TexCoord);
	base.rgb *= 100.0f;
	base.r = pow(base.r, hdrPower);
	base.g = pow(base.g, hdrPower);
	base.b = pow(base.b, hdrPower);

	return base / 100.0f;
	
}

float4 CopyToLightBuffer(VertexShaderParams input) : COLOR0
{
	float4 base = float4(tex2D(lightSampler, input.TexCoord).xyz, 0.5f);
	return base;
}

technique Technique1
{
    pass Pass1
    {
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 SquareIntensity();
    }
	pass Pass2
    {
	    VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 HorizontalBlurPS();
    }
	pass Pass3
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 VerticalBlurPS();
	}
	pass Pass4
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 CopyToLightBuffer();
	}
}
