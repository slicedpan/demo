float4x4 LightWorldView;
float4x4 LightWorldViewProjection;
float3 LightPosition;
float farClip;

struct ShadowMapVSInput
{
    float4 Position : POSITION0;
};

struct ShadowMapVSOutput
{
    float4 Position : POSITION0;
	float Depth : TEXCOORD0;
};

ShadowMapVSOutput ShadowMapVS(ShadowMapVSInput input)
{
    ShadowMapVSOutput output;

	float dist = length(input.Position.xyz - LightPosition);
	output.Position = mul(input.Position, LightWorldViewProjection);
	output.Depth = dist / 100;

    return output;
}

float4 ShadowMapPS(ShadowMapVSOutput input) : COLOR0
{
   return 0.0f;
}

technique ShadowMap
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 ShadowMapVS();
        PixelShader = compile ps_2_0 ShadowMapPS();
    }
}
