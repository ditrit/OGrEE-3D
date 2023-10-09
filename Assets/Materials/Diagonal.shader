Shader "Custom/Diagonal"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Thickness("Thickness", float) = 0.2
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1" }
		Pass{

			Blend SrcAlpha OneMinusSrcAlpha // Alpha blend
			CGPROGRAM

			#pragma	vertex vert             
			#pragma	fragment frag

			struct vertInput {
				float4 pos : POSITION;
			};

			struct vertOutput {
				float4 pos : POSITION;
				fixed3 worldPos : TEXCOORD1;
				fixed3 objPos : TEXCOORD2;
			};

			vertOutput vert(vertInput input)
			{
				vertOutput o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.worldPos = mul(unity_ObjectToWorld, input.pos).xyz;
				o.objPos = input.pos.xyz;
				return o;
			}
		fixed4 _Color;
		float _Thickness;

		half4 frag(vertOutput output) : SV_Target0
		{
			float3 worldScale = float3(
				length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)), // scale x axis
				length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y)), // scale y axis
				length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z))  // scale z axis
				);
			float2 coords = (output.objPos.xy + float2(0.5,0.5)) * worldScale;
			float distance = abs(worldScale.y*coords.x - worldScale.x * coords.y);
			half alpha = 0.0;
			if (distance < _Thickness * (worldScale.x+worldScale.y))
			{
				alpha = 1.0;
			}
			return half4(_Color.xyz, alpha);
	}
		ENDCG
	}
	}
}
