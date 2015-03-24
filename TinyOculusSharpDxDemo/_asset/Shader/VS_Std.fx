/**
 * @file  VS_Std.fx
 * @brief vertex shader for test
*/

#define MAX_INSTANCE_COUNT 100

cbuffer cbMain : register(b0)
{
	float4x4 g_worldMat[MAX_INSTANCE_COUNT];	// word matrix (row major)
};

cbuffer cbWorld : register(b1)
{
	float4x4 g_vpMat;		// view projection matrix (row major)
};


struct VS_INPUT
{
	uint InstanceId : SV_InstanceID;   
	float4 Position : POSITION;
	float2 UV1 : TEXCOORD0;
	float3 Normal : NORMAL;
};

struct VS_OUTPUT
{
	float4 Position : SV_POSITION;
	float4 WorldPosition : POSITION;
	float2 UV1 : TEXCOORD0;
	float3 Normal : NORMAL;
};

VS_OUTPUT main(VS_INPUT In)
{
	
	VS_OUTPUT Out;

	float4x4 worldMat = g_worldMat[In.InstanceId];
	float4x4 wvpMat = mul(worldMat, g_vpMat);

	Out.Position = mul(In.Position, wvpMat);
	Out.WorldPosition = mul(In.Position, worldMat);
	Out.UV1 = In.UV1;
	Out.Normal = mul(In.Normal, worldMat);
	
	return Out;
}

