Shader "Unlit/Confetti_Ribbon"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "LightMode" = "ForwardBase" }

		Pass
		{
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

#pragma target 4.5

#include "UnityCG.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			Texture2D uVert;
			Texture2D uNorm;
			Texture2D uTexCoord;
			Texture2D uTri;

			Texture2D uPosLife;
			Texture2D uVelScale;
			Texture2D uRot;
			
			v2f vert (uint id : SV_VertexID)
			{
				float3 vert = uVert.Load(uint3(id, 0, 0), 0).xyz;
				float3 norm = uNorm.Load(uint3(id, 0, 0), 0).xyz;
				float2 texCoord = uTexCoord.Load(uint3(id, 0, 0), 0).xy;

				v2f o;
				o.vertex = mul(UNITY_MATRIX_VP, float4(vert, 1.0f));
				o.normal = norm;
				o.uv = texCoord;
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				half4 col = half4(1., 1., 1., 1.);
				return col;
			}
			ENDCG
		}
	}
}
