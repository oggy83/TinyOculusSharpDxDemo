/**
 * @file  PS_Test.fx
 * @brief pixel shader for test
*/

cbuffer cbMain : register(b0)
{
	float4 g_ambientCol;
	float4 g_lightCol1;		// light1 color
	float4 g_lightCol2;		// light2 color
	float4 g_lightRange;	// light2-light5 range

	float4 g_cameraPos;		// camera position in world coords
	float4 g_light1Dir;		// light1 direction in world coords
	float4 g_light2Pos;		// light2 position in world coords
};


struct PS_INPUT
{
	float4 Position : SV_POSITION;		// position in screen space
	float4 WorldPosition : POSITION;	// position in world space
	float2 UV1 : TEXCOORD0;				// texture uv
	//float3 Normal : NORMAL;				// normal in world space
	//float3 Tangent : TANGENT0;			// tangent in world space 
	float4 Color : COLOR;
};

struct PS_OUTPUT
{
	float4 Color : SV_Target;
};

// diffuse texture1
Texture2D g_Diffuse1Tex : register(t0);
SamplerState g_Diffuse1Sampler : register(s0);


PS_OUTPUT main(PS_INPUT In)
{
	PS_OUTPUT Out;

	/*
	float3 tmpLight1Dir = -g_light1Dir;
	float3 Light1Dir = normalize(-g_light1Dir);

	float3 Light2Dist = g_light2Pos - In.WorldPosition;
	float3 Light2Dir = normalize(Light2Dist);

	float3 EyeDir = normalize(g_cameraPos - In.WorldPosition);

	float attenuation2 = saturate(1.0f - length(Light2Dist) / g_lightRange.x);

	// Calc Normal
	float3 bumpMapValue = 2 * g_Bump1Tex.Sample(g_Bump1Sampler, In.UV1).xyz - 1;// [0, 1] => [-1, 1]
	float3 Normal = normalize(In.Normal);
	float3 Tangent = normalize(In.Tangent);
	float3 Binormal = normalize(cross(Normal, Tangent));
	float3x3 tangentSpaceMat = {Tangent, Binormal, Normal};// tangent space to world space
	Normal = mul(bumpMapValue, tangentSpaceMat);

	// Diffuse (and Bump)
	float4 diffLight = g_ambientCol 
		+ max(0, dot(Normal, Light1Dir)) * g_lightCol1
		+ attenuation2 * max(0, dot(Normal, Light2Dir)) * g_lightCol2;
	float4 diffCol = diffLight * g_Diffuse1Tex.Sample(g_Diffuse1Sampler, In.UV1);

	// Blinn-Phong Model
	float3 halfVec1 = normalize(EyeDir + Light1Dir);
	float3 halfVec2 = normalize(EyeDir + Light2Dir);
	float3 specLight = pow(saturate(dot(Normal, halfVec1)), 40)
		+ attenuation2 * pow(saturate(dot(Normal, halfVec2)), 40);
	float3 specCol = specLight * float3(0.5, 0.5, 0.5);// highlight is white 

	Out.Color.rgb = diffCol.rgb + specCol;
	Out.Color.a = diffCol.a;
	*/

	Out.Color = In.Color * g_Diffuse1Tex.Sample(g_Diffuse1Sampler, In.UV1);

	// display normal map
	//Out.Color.rgb = (Normal + 1) * 0.5f;
	//Out.Color.a = 1.0f;
	return Out;
}

