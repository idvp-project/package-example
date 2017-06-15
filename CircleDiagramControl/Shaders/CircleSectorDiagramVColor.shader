Shader "diagram/сircle-sector-diagram/vertex-color"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_InnerRadius("Inner Radius", Float) = 0.8
		_OuterRadius("Outer Radius", Float) = 1.0
		_ScaleCircle("Scale Circle", Float) = 1.0
		_ScaleTexture("Scale Texture", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#pragma vertex   vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 position : POSITION;
				float2 texcoord_color : TEXCOORD0;
				float2 texcoord_color2 : TEXCOORD1;
				float4 color : COLOR0;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR0;
				float4 color2 : COLOR1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _InnerRadius;
			float _OuterRadius;
			float _ScaleCircle;
			float _ScaleTexture;

			v2f vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.position);
				o.texcoord = v.position.xz;
				o.color = v.color;
				o.color2 = float4(v.texcoord_color.xy, v.texcoord_color2.xy);
				return o;
			}

			//функция, которая делает пик на линии
			float gradient(float x)
			{
				float rc = (_OuterRadius + _InnerRadius) * 0.5f;
				float rd = _OuterRadius - rc;
				return clamp((1 - saturate(abs(x - rc) / rd)) * _ScaleCircle, 0.0, 1.0);
			}

			float4 frag(v2f i) : SV_Target
			{
				float2 tc = i.texcoord * 0.5 + 0.5;
				float4 texColor = tex2D(_MainTex, tc)*_ScaleTexture;

				float4 output = texColor;

				float4 color1 = i.color;
				float4 color2 = i.color2;

				float factor = gradient(length(i.texcoord));

				output *= lerp(color1, color2, factor);

				return output;
			}
			ENDCG
		}
	}
}
