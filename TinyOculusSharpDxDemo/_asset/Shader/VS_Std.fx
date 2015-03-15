/**
 * @file  VS_Std.fx
 * @brief vertex shader for test
*/

cbuffer cbMain : register(b0)
{
	float4x4 g_worldMat;	// word matrix (row major)
};

cbuffer cbWorld : register(b1)
{
	float4x4 g_vpMat;		// view projection matrix (row major)
};


struct VS_INPUT
{
	float4 Position : POSITION;
	float4 Color : COLOR;
	float2 UV1 : TEXCOORD0;
	float3 Normal : NORMAL;
};

struct VS_OUTPUT
{
	float4 Position : SV_POSITION;
	float4 WorldPosition : POSITION;
	float2 UV1 : TEXCOORD0;
	float3 Normal : NORMAL;
	float4 Color : COLOR;
};

VS_OUTPUT main(VS_INPUT In)
{
	
	VS_OUTPUT Out;

	float4x4 wvpMat = mul(g_worldMat, g_vpMat);

	Out.Position = mul(In.Position, wvpMat);
	Out.WorldPosition = mul(In.Position, g_worldMat);
	Out.UV1 = In.UV1;
	Out.Color = In.Color;
	Out.Normal = mul(In.Normal, g_worldMat);
	
	return Out;
}

