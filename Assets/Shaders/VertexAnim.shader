Shader "Custom/VertexAnim" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex("main texture (if we want one)",2D) = "white"{}
		_disc ("displacement",Range(.5,2))=1.0
		
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		
		sampler2D _MainTex;
		struct Input {		
			float2 uv_Maintex;
		};

		fixed4 _Color;
		float _disc;

		
		void vert(inout appdata_full v, out Input o)
		{

		
			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.y/=_disc;
			
		}
		

	

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = _Color;
		
			o.Albedo = c;
		
		}
		ENDCG
	}
	FallBack "Diffuse"
}
