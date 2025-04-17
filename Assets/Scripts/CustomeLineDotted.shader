Shader "Custom/DottedLineShader"
{
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // 🔥 Active la transparence
            ZWrite Off                     // ❌ N’écrit pas dans le depth buffer, évite les artefacts
            Cull Off                       // (optionnel) double face

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (frac(i.uv.x * 10) < 0.5)
                    return fixed4(1, 1, 1, 1);   // Blanc
                else
                    return fixed4(1, 1, 1, 0);   // 🔍 Alpha = 0 = transparent
            }
            ENDCG
        }
    }
}
