Shader "Custom/CinematicEffectShader"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Saturation ("Saturation", Range(0, 1)) = 0.5
        _Negative ("Negative", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _Saturation;
            float _Negative;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Ajout saturation
                float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = lerp(float3(gray, gray, gray), col.rgb, _Saturation);

                // Ajout négatif
                col.rgb = lerp(col.rgb, 1.0 - col.rgb, _Negative);

                // Application de la couleur UI
                col *= _Color;

                return col;
            }
            ENDCG
        }
    }
}
