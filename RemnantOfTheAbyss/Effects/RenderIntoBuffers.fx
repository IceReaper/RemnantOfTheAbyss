matrix Model;
matrix View;
matrix Projection;

texture AlbedoTexture;
sampler2D AlbedoSampler = sampler_state
{
    Texture = <AlbedoTexture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 WorldPosition : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float2 TexCoord : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(mul(float4(input.Position, 1.0), Model), mul(View, Projection));
    output.WorldPosition = mul(input.Position, (float3x3)Model);
    output.Normal = normalize(mul(input.Normal, (float3x3)Model));
    output.TexCoord = input.TexCoord;

    return output;
}

#define PixelShaderInput VertexShaderOutput

struct PixelShaderOutput
{
    float4 Position : COLOR0;
    float4 Normal : COLOR1;
    float4 Albedo : COLOR2;
};

PixelShaderOutput PixelShaderFunction(PixelShaderInput input)
{
    PixelShaderOutput output;

    output.Position = float4(input.WorldPosition, 1.0);
    output.Normal = float4(input.Normal * 0.5 + 0.5, 1.0);
    output.Albedo = tex2D(AlbedoSampler, input.TexCoord);

    return output;
}

technique BasicTechnique
{
    pass P0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
