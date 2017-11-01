// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Gradation" {
	SubShader
	{
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			// VS2015のグラフィックデバックON
			#pragma enable_d3d11_debug_symbols

			struct VertexInput
			{
				float4 pos:  POSITION;    // 3D空間座標
				float2 uv:   TEXCOORD0;   // テクスチャ座標
			};

			struct VertexOutput
			{
				float4 v:    SV_POSITION; // 2D座標
				float2 uv:   TEXCOORD0;   // テクスチャ座標
			};

			// 頂点 shader
			VertexOutput vert(VertexInput input)
			{
				VertexOutput output;
				output.v = UnityObjectToClipPos(input.pos);
				output.uv = input.uv;

				return output;
			}

			// ピクセル shader
			fixed4 frag(VertexOutput output) : SV_Target
			{
				float2 tex = output.uv;
				// 黄色→白色のグラデーション
				return fixed4(1.0 - tex.y, 1.0 - tex.y, 1.0 - tex.y, 1.0);
			}

			ENDCG
		}
	}
}
