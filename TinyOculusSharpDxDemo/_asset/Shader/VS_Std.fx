/**
 * @file  VS_Std.fx
 * @brief vertex shader for test
*/

cbuffer cbMain : register(b0)
{
	float4x4 g_wvpMat;		// word view projection matrix (row major)
	float4x4 g_worldMat;	// word matrix (row major)
	bool g_isEnableSkinning;
};

#define MAX_BONE_MATRICES 255

cbuffer cbBone : register(b1) 
{ 
	matrix g_boneMatrices[MAX_BONE_MATRICES]; 
};

struct VS_INPUT
{
	float4 Position : POSITION;
	float3 Normal : NORMAL;
	float2 UV1 : TEXCOORD0;
	float3 Tangent : TANGENT0;
	int4 BoneIndex : BONEINDEX;
	float4 BoneWeight : BONEWEIGHT;
};

struct VS_OUTPUT
{
	float4 Position : SV_POSITION;
	float4 WorldPosition : POSITION;
	float2 UV1 : TEXCOORD0;
	float3 Normal : NORMAL;
	float3 Tangent : TANGENT0;
};

VS_OUTPUT main(VS_INPUT In)
{
	
	VS_OUTPUT Out;

	if (g_isEnableSkinning)
	{
		float4x4 skinningMat
			= g_boneMatrices[In.BoneIndex.x] * In.BoneWeight.x
			+ g_boneMatrices[In.BoneIndex.y] * In.BoneWeight.y
			+ g_boneMatrices[In.BoneIndex.z] * In.BoneWeight.z
			+ g_boneMatrices[In.BoneIndex.w] * In.BoneWeight.w;
		float4 skinedPosition = mul(In.Position, skinningMat);
		Out.Position = mul(skinedPosition, g_wvpMat);
		Out.WorldPosition = mul(skinedPosition, g_worldMat);
		Out.Normal = mul(mul(In.Normal, skinningMat), g_worldMat);
		Out.Tangent = mul(mul(In.Tangent, skinningMat), g_worldMat);
	}
	else
	{
		Out.Position = mul(In.Position, g_wvpMat);
		Out.WorldPosition = mul(In.Position, g_worldMat);
		Out.Normal = mul(In.Normal, g_worldMat);
		Out.Tangent = mul(In.Tangent, g_worldMat);
	}

	
	Out.UV1 = In.UV1;
	return Out;
}

