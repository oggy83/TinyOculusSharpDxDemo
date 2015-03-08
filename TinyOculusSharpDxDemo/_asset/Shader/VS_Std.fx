/**
 * @file  VS_Std.fx
 * @brief vertex shader for test
*/

cbuffer cbMain : register(b0)
{
	float4x4 g_wvpMat;		// word view projection matrix (row major)
	float4x4 g_worldMat;	// word matrix (row major)
};

struct VS_INPUT
{
	float4 Position : POSITION;
	float2 UV1 : TEXCOORD0;
	//float3 Normal : NORMAL;
	//float3 Tangent : TANGENT0;
	float4 Color : COLOR;
};

struct VS_OUTPUT
{
	float4 Position : SV_POSITION;
	float4 WorldPosition : POSITION;
	float2 UV1 : TEXCOORD0;
	//float3 Normal : NORMAL;
	//float3 Tangent : TANGENT0;
	float4 Color : COLOR;
};

VS_OUTPUT main(VS_INPUT In)
{
	
	VS_OUTPUT Out;

	Out.Position = mul(In.Position, g_wvpMat);
	Out.WorldPosition = mul(In.Position, g_worldMat);
	Out.UV1 = In.UV1;
	Out.Color = In.Color;
	//Out.Normal = mul(In.Normal, g_worldMat);
	//Out.Tangent = mul(In.Tangent, g_worldMat);
	
	return Out;
}

