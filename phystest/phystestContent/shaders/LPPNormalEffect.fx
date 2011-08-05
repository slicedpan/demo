//-----------------------------------------------------------------------------
// LPPMainEffect.fx
//
// Jorge Adriano Luna 2010
// http://jcoluna.wordpress.com
//
// It uses some code from Nomal Mapping Sample found at
// http://create.msdn.com/en-US/education/catalog/sample/normal_mapping
// and also code from here
// http://aras-p.info/texts/CompactNormalStorage.html
//-----------------------------------------------------------------------------


//-----------------------------------------
// Parameters
//-----------------------------------------
float4x4 World;
float4x4 WorldView;
float4x4 View;
float4x4 Projection;
float4x4 WorldViewProjection;


float FarClip;
float SpecularPower;
float Shininess;
float2 LightBufferPixelSize;
float Highlight;
float HighlightMin;

//as we used a 0.01f scale when rendering to light buffer,
//revert it back here.
const static float LightBufferScaleInv = 100.0f;

//-----------------------------------------
// Textures
//-----------------------------------------
texture DiffuseMap;
sampler diffuseMapSampler = sampler_state
{
	Texture = (DiffuseMap);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture NormalMap;
sampler normalMapSampler = sampler_state
{
	Texture = (NormalMap);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture LightBuffer;
sampler2D lightSampler = sampler_state
{
	Texture = <LightBuffer>;
	MipFilter = POINT;
	MagFilter = POINT;
	MinFilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};
//-------------------------------
// Helper functions
//-------------------------------
half2 EncodeNormal (half3 n)
{
	float kScale = 1.7777;
	float2 enc;
	enc = n.xy / (n.z+1);
	enc /= kScale;
	enc = enc*0.5+0.5;
	return enc;
}

float2 PostProjectionSpaceToScreenSpace(float4 pos)
{
	float2 screenPos = pos.xy / pos.w;
	return (0.5f * (float2(screenPos.x, -screenPos.y) + 1));
}

half3 NormalMapToSpaceNormal(half3 normalMap, float3 normal, float3 binormal, float3 tangent)
{
	normalMap = normalMap * 2 - 1;
	normalMap = half3(normal * normalMap.z + normalMap.x * tangent - normalMap.y * binormal);
	return normalMap;
}	
//-------------------------------
// Shaders
//-------------------------------

struct VertexShaderInput
{
    float4 Position  : POSITION0;
    float2 TexCoord  : TEXCOORD0;
    float3 Normal    : NORMAL0;    
    float3 Binormal  : BINORMAL0;
    float4 Tangent   : TANGENT;
};


struct VertexShaderOutput
{
    float4 Position			: POSITION0;
    float2 TexCoord			: TEXCOORD0;
    float Depth				: TEXCOORD1;
	
    float3 Normal	: TEXCOORD2;
    float3 Tangent	: TEXCOORD3;
    float3 Binormal : TEXCOORD4; 
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float3 viewSpacePos = mul(input.Position, WorldView);
    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = input.TexCoord; //pass the texture coordinates further

	//we output our normals/tangents/binormals in viewspace
	output.Normal = normalize(mul(input.Normal,WorldView)); 
	output.Tangent =  normalize(mul(input.Tangent.xyz,WorldView)); 
	
	output.Binormal = normalize(mul(input.Binormal.xyz, WorldView));
		
	output.Depth = viewSpacePos.z; //pass depth
    return output;
}
//render to our 2 render targets, normal and depth 
struct PixelShaderOutput
{
    float4 Normal : COLOR0;
    float4 Depth : COLOR1;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output = (PixelShaderOutput)1;   
	//read from our normal map
	half4 normalMap = tex2D(normalMapSampler, input.TexCoord);
	half3 normalViewSpace = NormalMapToSpaceNormal(normalMap.xyz, input.Normal, input.Binormal, input.Tangent);
    
	output.Normal.rg =  EncodeNormal (normalize(normalViewSpace));	//our encoder output in RG channels
	output.Normal.b = normalMap.a;			//our SpecularPower goes into B channel
	output.Normal.a = Shininess;					//not used
	output.Depth.x = -input.Depth/ FarClip;		//output Depth in linear space, [0..1]
	
	return output;
}



struct ReconstructVertexShaderInput
{
    float4 Position  : POSITION0;
    float2 TexCoord  : TEXCOORD0;
};


struct ReconstructVertexShaderOutput
{
    float4 Position			: POSITION0;
    float2 TexCoord			: TEXCOORD0;
	float4 TexCoordScreenSpace : TEXCOORD1;
};

ReconstructVertexShaderOutput ReconstructVertexShaderFunction(ReconstructVertexShaderInput input)
{
    ReconstructVertexShaderOutput output;
	
    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = input.TexCoord; //pass the texture coordinates further
	output.TexCoordScreenSpace = output.Position;
    return output;
}

float4 ReconstructPixelShaderFunction(ReconstructVertexShaderOutput input):COLOR0
{
	PixelShaderOutput output = (PixelShaderOutput)1; 
	
	  
	// Find the screen space texture coordinate and offset it
	float2 screenPos = PostProjectionSpaceToScreenSpace(input.TexCoordScreenSpace) + LightBufferPixelSize;

	//read from our diffuse, specular and emissive maps
	half3 diffuseMap  = tex2D(diffuseMapSampler , input.TexCoord).rgb;	
	
	//read our light buffer texture. Remember to multiply by our magic constant explained on the blog
	float4 lightColor =  tex2D(lightSampler, screenPos) * LightBufferScaleInv;

	//our specular intensity is stored in alpha. We reconstruct the specular here, using a cheap and NOT accurate trick

	float3 specular = lightColor.rgb*lightColor.a;
	//return float4(lightColor.aaa,1);
	float4 finalColor = float4(diffuseMap*lightColor.rgb + specular,1);
	//add a small constant to avoid dark areas
	return (finalColor * Highlight) + HighlightMin;
}




technique Technique1
{
    pass RenderToGBuffer
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

technique Technique2
{
	pass ReconstructShading
    {
        VertexShader = compile vs_2_0 ReconstructVertexShaderFunction();
        PixelShader = compile ps_2_0 ReconstructPixelShaderFunction();
    }
}

//----------------------------------------------------------------------------------------------------
//	Shadow Map
//----------------------------------------------------------------------------------------------------

float4x4 LightWorldView;
float4x4 LightWorldViewProjection;
float3 LightPosition;
float LightClip;
float Dir;

struct ShadowMapVSInput
{
    float4 Position : POSITION0;
};

struct ShadowMapVSOutput
{
	float4 Position: POSITION0;
	float Depth: TEXCOORD0;
	float ClipDepth: TEXCOORD1;
};


ShadowMapVSOutput ShadowMapVS(ShadowMapVSInput input)
{
    ShadowMapVSOutput output;
	
	output.Position = mul(input.Position, LightWorldView); // position in light's view space
	output.Position /= output.Position.w;

	output.Position.z *= Dir;

	float fLength = length (output.Position.xyz);
	output.Position /= fLength;
	output.ClipDepth = output.Position.z;

	output.Position.x /= output.Position.z + 1.0f;
	output.Position.y /= output.Position.z + 1.0f;

	output.Position.y += Dir;
	output.Position.y /= 2.0f;

	output.Position.z = fLength / LightClip;
		
	output.Position.w = 1;

	output.Depth = output.Position.z;

    return output;
}

float4 ShadowMapPS(ShadowMapVSOutput input) : COLOR0
{	
	clip(input.ClipDepth);
	return input.Depth;
}

technique ShadowMap
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 ShadowMapVS();
        PixelShader = compile ps_2_0 ShadowMapPS();
    }
}

texture edgemask;
sampler edgeSampler = sampler_state
{
	Texture = (edgemask);
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct HighlightVSOutput
{
	float4 Position : TEXCOORD0;
	float4 pos : POSITION0;	 
};

HighlightVSOutput HighlightVertex (float4 position : POSITION0)
{
	HighlightVSOutput output;
	output.pos = mul(position, WorldViewProjection);
	output.Position = output.pos;
	return output;
}

float4 HighlightPixel (HighlightVSOutput input) : COLOR0
{
	float edge = tex2D(edgeSampler, PostProjectionSpaceToScreenSpace(input.Position)).rg;
	clip(edge - 0.01f);
	return float4(1.0f, 0.839f, 0.192f, 1.0f);
}

technique DrawHighlight
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 HighlightVertex();
		PixelShader = compile ps_2_0 HighlightPixel();
	}
}