Shader "Hidden/BrightnessThresholdPost"
{
    Properties
    {
        _Threshold ("Brightness Threshold", Range(0,1)) = 0.5
        _MainTex ("MainTex", 2D) = "white" {}
        _PreTex ("Pre Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Threshold;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float brightness = dot(col.rgb, float3(0.299, 0.587, 0.114));

                if (brightness < _Threshold)
                    discard;

                return col;
            }
            ENDCG
        }
    }
}
