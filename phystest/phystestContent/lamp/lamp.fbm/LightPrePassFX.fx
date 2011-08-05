/*********************************************************************NVMH3****
$Revision$

Copyright NVIDIA Corporation 2007
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY
LOSS) ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF
NVIDIA HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.


To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

******************************************************************************/


/******************************************************************************
Changed by Jorge Adriano Luna (aka Coluna), for providing a nice integration with
XNA 4.0 Light Pre Pass sample
http://jcoluna.wordpress.com/
Enjoy it!
*******************************************************************************/

string ParamID = "0x003";

#ifdef _MAX_
	#include "ShadowMap.fxh"
#endif	

#ifdef _MAX_
	SHADOW_FUNCTOR(shadowTerm);
#endif	

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

//// UN-TWEAKABLES - AUTOMATICALLY-TRACKED TRANSFORMS ////////////////

float4x4 WorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldXf : World < string UIWidget="None"; >;
float4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;

#ifdef _MAX_
int texcoord1 : Texcoord
<
	int Texcoord = 1;
	int MapChannel = 0;
	string UIWidget = "None";
>;

int texcoord2 : Texcoord
<
	int Texcoord = 2;
	int MapChannel = -2;
	string UIWidget = "None";
>;

int texcoord3 : Texcoord
<
	int Texcoord = 3;
	int MapChannel = -1;
	string UIWidget = "None";
>;
#endif

//// TWEAKABLE PARAMETERS ////////////////////

/// Point Lamp 0 ////////////
float3 Lamp0Pos : POSITION <
    string Object = "PointLight0";
    string UIName =  "Light Position";
    string Space = "World";
	int refID = 0;
> = {-0.5f,2.0f,1.25f};
#ifdef _MAX_
float3 Lamp0Color : LIGHTCOLOR
<
	int LightRef = 0;
	string UIWidget = "None";
> = float3(1.0f, 1.0f, 1.0f);
#else
float3 Lamp0Color : Specular <
    string UIName =  "Lamp 0";
    string Object = "Pointlight0";
    string UIWidget = "Color";
> = {1.0f,1.0f,1.0f};
#endif

float4 k_a  <
	string UIName = "Ambient";
	string UIWidget = "Color";
> = float4( 0.0f, 0.0f, 0.0f, 1.0f );    // ambient
	
float4 k_d  <
	string UIName = "Diffuse";
	string UIWidget = "Color";
> = float4( 0.47f, 0.47f, 0.47f, 1.0f );    // diffuse
	
float4 k_s  <
	string UIName = "Specular";
	string UIWidget = "Color";
> = float4( 1.0f, 1.0f, 1.0f, 1.0f );    // diffuse    // specular

int n<
	string UIName = "Specular Power";
	string UIWidget = "slider";
	float UIMin = 0.0f;
	float UIMax = 50.0f;	
>  = 15;


//////// COLOR & TEXTURE /////////////////////

bool emissiveMapEnabled <
	string UIName = "Emissive Map Enable";
> = false;

texture emissiveMap : EmissiveMap< 
	string UIName = "Emissive Map ";
	string ResourceType = "2D";
	int Texcoord = 0;
	int MapChannel = 1;
>;


bool diffuseMapEnabled <
	string UIName = "Diffuse Map Enable";
> = false;

texture diffuseMap : DiffuseMap< 
	string UIName = "Diffuse Map ";
	string ResourceType = "2D";
	int Texcoord = 0;
	int MapChannel = 1;
>;

bool specularMapEnabled <
	string UIName = "Specular Map Enable";
> = false;

texture specularMap < 
	string UIName = "Specular Map";
	string ResourceType = "2D";
>;

bool normalMapEnabled <
	string UIName = "Normal Map Enable";
> = false;


texture normalMap < 
	string UIName = "Normal Map";
	string ResourceType = "2D";
>;


sampler2D emissiveSampler = sampler_state
{
	Texture = <emissiveMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
    AddressV = Wrap;
	
};
sampler2D diffuseSampler = sampler_state
{
	Texture = <diffuseMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
    AddressV = Wrap;
	
};
	
sampler2D specularSampler = sampler_state
{
	Texture = <specularMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
    AddressV = Wrap;
	
};

sampler2D normalSampler = sampler_state
{
	Texture = <normalMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
    AddressV = Wrap;
	
};

/* data from application vertex buffer */
struct appdata {
	float4 Position		: POSITION;
	float3 Normal		: NORMAL;
	float3 Tangent		: TANGENT;
	float3 Binormal		: BINORMAL;
	float2 UV0		: TEXCOORD0;	
	float3 Colour		: TEXCOORD1;
	float3 Alpha		: TEXCOORD2;
	float3 Illum		: TEXCOORD3;
	float3 UV1		: TEXCOORD4;
	float3 UV2		: TEXCOORD5;
	float3 UV3		: TEXCOORD6;
	float3 UV4		: TEXCOORD7;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    float4 UV0		: TEXCOORD0;
    // The following values are passed in "World" coordinates since
    //   it tends to be the most flexible and easy for handling
    //   reflections, sky lighting, and other "global" effects.
    float3 LightVec	: TEXCOORD1;
    float3 WorldNormal	: TEXCOORD2;
    float3 WorldTangent	: TEXCOORD3;
    float3 WorldBinormal : TEXCOORD4;
    float3 WorldView	: TEXCOORD5;
	float4 UV1		: TEXCOORD6;
	float4 UV2		: TEXCOORD7;
	float4 wPos		: TEXCOORD8;
};
 
///////// VERTEX SHADING /////////////////////

/*********** Generic Vertex Shader ******/

vertexOutput std_VS(appdata IN) {
    vertexOutput OUT = (vertexOutput)0;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
#ifndef _MAX_	
    OUT.WorldTangent = mul(IN.Tangent,WorldITXf).xyz;
    OUT.WorldBinormal = mul(IN.Binormal,WorldITXf).xyz;
#else
    OUT.WorldTangent = mul(IN.Binormal,WorldITXf).xyz;
    OUT.WorldBinormal = mul(IN.Tangent,WorldITXf).xyz;
#endif	
    float4 Po = float4(IN.Position.xyz,1);
    float3 Pw = mul(Po,WorldXf).xyz;
    OUT.LightVec = (Lamp0Pos - Pw);
    OUT.WorldView = normalize(ViewIXf[3].xyz - Pw);
    OUT.HPosition = mul(Po,WvpXf);
	OUT.wPos = mul(IN.Position, WorldXf);
	
// UV bindings
// Encode the color data
 	float4 colour;
   	colour.rgb = IN.Colour * IN.Illum;
   	colour.a = IN.Alpha.x;
   	OUT.UV0.z = colour.r;
   	OUT.UV0.a = colour.g;
  	OUT.UV1.z = colour.b;
   	OUT.UV1.a = colour.a;

// Pass through the UVs
	OUT.UV0.xy = IN.UV0.xy;
   	OUT.UV1.xy = IN.UV1.xy;
   	OUT.UV2.xyz = IN.UV2.xyz;
// 	OUT.UV3 = OUT.UV3;
// 	OUT.UV4 = OUT.UV4;
    return OUT;
}

///////// PIXEL SHADING //////////////////////

// Utility function for phong shading

void phong_shading(vertexOutput IN,
		    float3 LightColor,
		    float3 Nn,
		    float3 Ln,
		    float3 Vn,
		    out float3 DiffuseContrib,
		    out float3 SpecularContrib)
{
	float3 specLevel = float3(1.0,1.0,1.0);
	if(specularMapEnabled)
		specLevel = tex2D(specularSampler, IN.UV0.xy).xyz;
	
	float glossiness = n;
	if(normalMapEnabled)
	{
	    glossiness = tex2D(normalSampler,IN.UV0.xy).a * 100;
	}
    float3 Hn = normalize(Vn + Ln);
    float4 litV = lit(dot(Ln,Nn),dot(Hn,Nn),glossiness);
    DiffuseContrib = litV.y * LightColor;
    SpecularContrib = litV.y * litV.z * k_s * LightColor * specLevel;
}

float4 std_PS(vertexOutput IN) : COLOR {
    float3 diffContrib;
    float3 specContrib;
    float3 Ln = normalize(IN.LightVec);
    float3 Vn = normalize(IN.WorldView);
    float3 Nn = normalize(IN.WorldNormal);
    float3 Tn = normalize(IN.WorldTangent);
    float3 Bn = normalize(IN.WorldBinormal);
	float4 vertColour = float4(IN.UV0.z,IN.UV0.w,IN.UV1.z,IN.UV1.w);	
	float3 BottomCol = k_d.rgb; 
	if(normalMapEnabled)
	{
	    float3 bump = (tex2D(normalSampler,IN.UV0).rgb - float3(0.5,0.5,0.5));
	    Nn = Nn + bump.x*Tn + bump.y*Bn;
	}
			
    Nn = normalize(Nn);
	
	phong_shading(IN,Lamp0Color,Nn,Ln,Vn,diffContrib,specContrib);

    float4 diffuseColor = k_d;
	float3 ambientColor = k_a;
	if(diffuseMapEnabled)
		diffuseColor = tex2D(diffuseSampler,IN.UV0.xy);
			

// Perform the alpha blends ops		
	float alpha = diffuseColor.a;
		
	if (emissiveMapEnabled)
		diffuseColor += tex2D(emissiveSampler,IN.UV0.xy);
	
#ifndef _MAX_		
    float3 result = specContrib+(diffuseColor*(diffContrib.rgb+ambientColor ));
#else	
	float3 result = (ambientColor + shadowTerm(IN.wPos)) * specContrib +(diffuseColor.rgb * diffContrib.rgb);
#endif	
	
    return float4(result,1);
}

///// TECHNIQUES /////////////////////////////

technique Main <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 std_VS();
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = true;
		CullMode = None;
        PixelShader = compile ps_3_0 std_PS();
    }
}

/////////////////////////////////////// eof //
