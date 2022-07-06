Shader "Unlit/FallingSandsShaderBasic"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
		_TemperatureTex ("_TemperatureTex", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque+1" }
		LOD 100

		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD1;
				float4 worldNormal : TEXCOORD2;
			};

			sampler2D _TemperatureTex;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			float4 FallingSandsDataColors[32];
			int FallingSandsDataColorsLength;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1));
				o.worldNormal = mul(unity_ObjectToWorld, float4(v.normal.xyz, 0));

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 background = 0;

				float4 data = tex2D(_MainTex, i.uv);
				float temperature = tex2D(_TemperatureTex, i.uv).x;

				int colorIndex = (int) data.x; 

				if (colorIndex > 0 && colorIndex < FallingSandsDataColorsLength)
				{
					float4 dataColor = FallingSandsDataColors[colorIndex];
					float brightness = 1.0 + abs(temperature);

					return dataColor * brightness;
				}

				return background;
			}
			ENDCG
		}
	}
}
