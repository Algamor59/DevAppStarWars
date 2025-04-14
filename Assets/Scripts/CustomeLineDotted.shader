Shader "Custom/DottedLineShader"
{
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Opaque" }
        Pass
        {
            // Mat�riau unlit, sans �clairage.
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
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            // Vertex shader : juste passe les coordonn�es
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Fragment shader : cr�er l'effet pointill�
            half4 frag(v2f i) : SV_Target
            {
                // Si la position UV est pair, on la rend blanche, sinon transparente.
                if (frac(i.uv.x * 10) < 0.5)
                    return half4(1, 1, 1, 1); // Couleur blanche
                else
                    return half4(0, 0, 0, 0); // Transparent (pas de couleur noire)
            }
            ENDCG
        }
    }
}
