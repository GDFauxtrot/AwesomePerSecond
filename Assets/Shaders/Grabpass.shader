Shader "FX/CONCUSIVE"
{
	Properties
	{
		_NoiseTexture ("NoiseTexture", 2D) = "white" {}
		_noiseIntensity ("noise intesnity",Range(0,100))=1.0
	}
	SubShader
	{
		Tags { "Queue"="Transparent" }
		LOD 100
		GrabPass
        {
            "_BackgroundTexture"
        }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv: TEXCOORD0;
			};

			struct v2f
			{
				float2 uv: TEXCOORD0;
				float4 grabPos : TEXCOORD1;
			
				float4 vertex : SV_POSITION;
			};


			float _noiseIntensity;
			sampler2D _NoiseTexture;
			float4 _NoiseTexture_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.grabPos = ComputeGrabScreenPos(o.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _NoiseTexture);
				return o;
			}
			
			sampler2D _BackgroundTexture;

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 noise = tex2D(_NoiseTexture,float2(i.uv.x,i.uv.y));
				fixed4 col = tex2Dproj(_BackgroundTexture,i.grabPos+noise*_noiseIntensity);
				// apply fog
				return col;
			}
			ENDCG
		}
	}
}
