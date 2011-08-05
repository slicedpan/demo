float4x4 World;
float4x4 View;
float4x4 Projection;

float4 VertexShaderFunction(float4 input : POSITION0) : POSITION0
{
	input.z = 1.0f; //this clears the depth buffer
    return input;
}

float4 PixelShaderFunction(float4 input : POSITION0) : COLOR0
{
    return 1.0f;
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
