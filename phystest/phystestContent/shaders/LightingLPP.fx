//-----------------------------------------------------------------------------
// LightingLPP.fx
//
// uses code from:
// http://jcoluna.wordpress.com
//
//-----------------------------------------------------------------------------


//-----------------------------------------
// Parameters
//-----------------------------------------
float3 LightColor;
float3 LightDir;
float3 LightPosition;
float4x4 LightViewProj;
float4x4 InvViewLightViewProj;
float4x4 InvView;
float4x4 LightView;
float3 LightViewRay;
float Radius;
float InvLightRadiusSqr;
float3 FrustumCorners[4];
float2 GBufferPixelSize;
bool Occlude;
float fudge;
float FarClip;
float2 texOffset;
float2 texScale;

//we use this to avoid clamping our results into [0..1]. 
//this way, we can fake a [0..100] range, since we are using a
//floating point buffer
const static float LightBufferScale = 0.01f;

//-----------------------------------------
// Textures
//-----------------------------------------
texture DepthBuffer;
sampler2D depthSampler = sampler_state
{
	Texture = <DepthBuffer>;
	MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture NormalBuffer;
sampler2D normalSampler = sampler_state
{
	Texture = <NormalBuffer>;
	MipFilter = NONE;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture ShadowMap;
sampler2D backShadowSampler = sampler_state
{
	Texture = <ShadowMap>;
	MipFilter = POINT;
	MagFilter = POINT;
	MinFilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

//-------------------------------
// Helper functions
//-------------------------------
half3 DecodeNormal (half4 enc)
{
	float kScale = 1.7777;
	float3 nn = enc.xyz*float3(2*kScale,2*kScale,0) + float3(-kScale,-kScale,1);
	float g = 2.0 / dot(nn.xyz,nn.xyz);
	float3 n;
	n.xy = g*nn.xy;
	n.z = g-1;
	return n;
}

float3 GetFrustumRay(in float2 texCoord)
{
	float index = texCoord.x + (texCoord.y * 2);
	return FrustumCorners[index];
}

//-------------------------------
// Shaders
//-------------------------------

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 FrustumRay : TEXCOORD1;
};

struct PixelShaderOutput
{
	float4 Color:	COLOR0;
	float4 Lit:		COLOR1;
};

VertexShaderOutput PointLightVS(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
	
	output.Position = input.Position;
	output.TexCoord = input.Position.xy*0.5f + float2(0.5f,0.5f); 
	output.TexCoord.y = 1 - output.TexCoord.y;	
	output.TexCoord += GBufferPixelSize;
	output.FrustumRay = GetFrustumRay(input.TexCoord);
	return output;
}

float ComputeAttenuation(float3 lDir)
{
	return 1 - saturate(dot(lDir,lDir)*InvLightRadiusSqr);
}


float4 PointLightPS(VertexShaderOutput input) : COLOR0
{
	//read the depth value
	float depthValue = tex2D(depthSampler, input.TexCoord).r;

	//if depth value == 1, we can assume its a background value, so skip it
	clip(-depthValue + 0.9999f);

    // Reconstruct position from the depth value, making use of the ray pointing towards the far clip plane
    float3 pos = input.FrustumRay * depthValue;
	
	//light direction from current pixel to current light
	float3 lDir = LightPosition - pos;	
 
	//compute attenuation, 1 - saturate(d2/r2)
	float atten = ComputeAttenuation(lDir);
	
	// Convert normal back with the decoding function
	float4 normalMap = tex2D(normalSampler, input.TexCoord);
	float3 normal = DecodeNormal(normalMap);
			
	lDir = normalize(lDir);

	// N dot L lighting term, attenuated
	float nl = saturate(dot(normal, lDir))*atten;

	float4 finalColor;
	//As our position is relative to camera position, we dont need to use (ViewPosition - pos) here
	float3 camDir = normalize(pos);
	
	// Calculate specular term
	float3 h = normalize(reflect(lDir, normal));

	float spec;

	spec = normalMap.a*nl*pow(saturate(dot(camDir, h)), normalMap.b*100);		
			
	finalColor = float4(LightColor * (nl), spec);	
	
	return finalColor*LightBufferScale;

	//output light

}

float4 PrePixelShader(VertexShaderOutput input) : COLOR0
{
	return float4(0,0,0,0);	
}

float4 ClearPixelShader(VertexShaderOutput input) : COLOR0
{
	
	half2 h;
	h.y = 1;
	h.x = 0;
	float f = (float)h;
	return float4(0,0,0,f);
}

technique Technique1
{
	pass first
	{		
		VertexShader = compile vs_3_0 PointLightVS();
		PixelShader = compile ps_3_0 PointLightPS();
	}
}
