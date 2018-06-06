Shader "Unlit/Confetti_Ribbon"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		
		Cull Off

		Pass
		{
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

#pragma target 4.5

#include "UnityCG.cginc"
#include "Assets/Shaders/SimplexNoise3D.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 vertex : SV_POSITION;

				float3 rayDir : TEXCOORD1;
				float3 reflDir : TEXCOORD2;
				float3 lightDir : TEXCOORD3;
				float3 ambient : TEXCOORD4;
				float3 diffuse : TEXCOOORD5;
				float fresnel : TEXCOORD6;
				float4 velScale : TEXCOORD7;
				float4 posLife : TEXCOORD8;
				float4 albedo : TEXCOORD9;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D uTexStroke;
			TextureCube<float4> uCubeMap;
			SamplerState sampleruCubeMap;
			int uNumTrails;

			Texture2D uPosLife;
			Texture2D uVelScale;
			Texture2D uRotDir;

			void lookAt(inout float3 tar, in float3 dir)
			{
				float3 forward = dir;
				
				float3 rand = float3(0., 1., 0.);
				if (abs(dot(forward, rand)) < .01)
				{
					if (forward.y > 0.)
						rand = float3(0., 0., 1.);
					else
						rand = float3(0., 0., -1.);
				}

				float3 left = normalize(cross(forward, rand));
				float3 up = normalize(cross(left, forward));

				tar = normalize(tar.x*left + tar.y*up + tar.z*forward);
			}
			
			v2f vert (appdata_full v)
			{
				uint3 coords = uint3(v.texcoord1.xy, 0);

				float4 d = uPosLife.Load(coords, 0);
				float3 pos = d.xyz;
				float life = d.w;
				d = uVelScale.Load(coords, 0);
				float3 vel = d.xyz;
				float scale = d.w;
				float4 rotDir = uRotDir.Load(coords, 0);

				float3 vert = v.vertex.xyz;
				lookAt(vert, rotDir.xyz);
				vert *= (1. * pow((1. - v.texcoord.y), 3.) * clamp(life, 0., 1.));
				vert *= pow(scale + .3, 9.)*400.;
				vert += pos;

				float3 norm = v.normal;
				lookAt(norm, rotDir.xyz);

				float3 rayDir = -normalize(UnityWorldSpaceViewDir(vert));
				float3 reflectDir = reflect(rayDir, norm);

				float3 light = normalize(_WorldSpaceLightPos0);
				float3 ambient = .8 + .2 * norm.y;
				float3 diffuse = max(dot(norm, light), 0.);
				float fre = pow(clamp(1. + dot(norm, rayDir), 0., 1.), 2.);

				v2f o;
				o.vertex = mul(UNITY_MATRIX_VP, float4(vert, 1.));
				o.normal = norm;
				o.uv = v.texcoord;
				o.rayDir = rayDir;
				o.reflDir = reflectDir;
				o.lightDir = light;
				o.ambient = ambient;
				o.diffuse = diffuse;
				o.fresnel = fre;
				o.velScale = float4(vel, scale);
				o.posLife = float4(pos, life);
				o.albedo = snoise_grad(v.texcoord1.yyy);
				return o;
			}
			
			half4 frag(v2f i) : SV_Target
			{
				half3 albedo = i.albedo.rgb;
				albedo = albedo * half3(.6, .55, .3) + half3(.3, .75, .2);
				albedo *= i.velScale.xyz * 10.;

				float3 env =
					pow(max(uCubeMap.SampleLevel(sampleruCubeMap, i.reflDir, 0).rgb, 0.), 2.2)
					* pow(i.fresnel, 2.) * .99 + .01;

				half3 brdf = albedo;
				brdf += 12.2 * i.ambient * albedo;
				brdf += 3. * i.diffuse * albedo;
				brdf += .3 * i.fresnel * albedo;

				half3 col = brdf * env;

				col = pow(col, .45);

				return half4(col, 1.);
			}
			ENDCG
		}
	}
}
