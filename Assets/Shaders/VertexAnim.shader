Shader "Custom/VertexAnim" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_disc ("displacement",Range(0,.5))=.0
		_Squashing("Squash (0,1)",Range(0,1))=0
		_Speed( "vector of speed", Vector)=(0,0,0,0)
		
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		
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
			float2 uv_MainTex;
			float3 vectorDirection;
		};

		float _disc, _Squashing;
		float4 _Speed;
		
		void vert(inout appdata_full v, out Input o)
		{
			
			float DirectMovement = 1+(-1*pow(1-saturate(dot(normalize(-1*_Speed.xyz),UnityObjectToWorldNormal(v.normal))),.5));
		//	float Xaxiz = pow(1-saturate(dot(UnityObjectToWorldNormal(v.normal),-1*(_Speed.xyz)+1)),5);
			float yAxis = abs(dot(normalize(float3(0,1,0)),UnityObjectToWorldNormal(v.normal)));
			float xzAxis =  abs(dot(normalize(float3(1,0,1)),UnityObjectToWorldNormal(v.normal)));
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.vectorDirection.rg = float2(DirectMovement, abs(1-(_Squashing)));
			
			v.vertex.xyz+= abs(1-(_Squashing))* DirectMovement*normalize(v.normal)*_disc;
			v.vertex.xyz+= -1*_Squashing*yAxis*normalize(v.normal)*_disc; 
		}
		
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

	

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
