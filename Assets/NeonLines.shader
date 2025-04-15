Shader "Custom/FluorescentLineWithEmission"
{
    Properties
    {
        _Color ("Line Color", Color) = (0, 1, 0, 1) // Couleur verte fluo par d�faut
        _EmissionColor ("Emission Color", Color) = (0, 1, 0, 1) // Couleur de l'�mission (verte fluo)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            // D�claration des couleurs
            float4 _Color;
            float4 _EmissionColor;

            struct appdata
            {
                float4 vertex : POSITION; // Position du vertex
            };
            struct v2f
            {
                float4 pos : POSITION; // Position dans le fragment shader
            };

            // Vertex Shader : transformation des vertices
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // Transformation des coordonn�es du vertex
                return o;
            }

            // Fragment Shader : on applique la couleur et l'�mission
            half4 frag(v2f i) : SV_Target
            {
                // Appliquer la couleur de la ligne
                half4 lineColor = half4(_Color.rgb, 1.0);
                
                // Appliquer l'effet �missif
                half4 emission = half4(_EmissionColor.rgb, 1.0);

                // On combine la couleur de la ligne et l'�mission
                return lineColor + emission;
            }
            ENDCG
        }
    }
}
