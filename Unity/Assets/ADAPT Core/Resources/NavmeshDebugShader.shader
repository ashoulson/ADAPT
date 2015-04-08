Shader "DebugMaterial" { 
	SubShader { 
		Pass { 
			ZWrite On 
			Blend SrcAlpha OneMinusSrcAlpha 
			Cull Off 
			Lighting Off 
			ColorMaterial Emission 
			CGPROGRAM
			#pragma exclude_renderers gles
			#pragma vertex vert
			#include "UnityCG.cginc"
			struct vin {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				fixed4 color : COLOR;
			};
			struct v2f {
				float4 pos : POSITION;
				fixed4 color : COLOR;
			};
			v2f vert(vin v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.pos.xyz += v.normal.xyz * o.pos.w;
				o.color = v.color;
				return o;
			}
			ENDCG
		}
	}
}