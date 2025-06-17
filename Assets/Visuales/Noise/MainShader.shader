Shader "Unlit/MainShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DisplaceAmount ("Displacement Amount", Range(0, 1)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _DisplaceAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calcular centro y distancia normalizada
                float2 center = float2(0.5, 0.5);
                float2 delta = i.uv - center;
                float dist = length(delta);
                float maxDist = 0.5; // porque UV va de 0 a 1
                float intensity = saturate(dist / maxDist); // 0 en el centro, 1 en el borde

                // Leer ruido y remapear de [0,1] a [-1,1]
                float2 noiseUV = i.uv;
                float2 noise = tex2D(_NoiseTex, noiseUV).rg * 2.0 - 1.0;

                // Calcular offset con dirección de ruido y escala
                float2 offset = noise * _DisplaceAmount * intensity;

                // Desplazar UV para muestreo de color
                float2 displacedUV = i.uv + offset;

                fixed4 col = tex2D(_MainTex, displacedUV);

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
