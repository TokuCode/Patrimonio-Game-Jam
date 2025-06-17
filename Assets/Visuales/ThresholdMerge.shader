Shader "Unlit/ThresholdMerge"
{
    Properties
    {
        _PreTex ("Pre Render", 2D) = "white" {}
        _PostTex ("Post Render", 2D) = "black" {}
        _Threshold ("Brightness Threshold", Range(0,1)) = 0.5
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

            sampler2D _PreTex;
            sampler2D _PostTex;
            float _Threshold;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 preCol = tex2D(_PreTex, i.uv);
                fixed4 postCol = tex2D(_PostTex, i.uv);

                float brightness = dot(preCol.rgb, float3(0.299, 0.587, 0.114));
                float mask = step(_Threshold, brightness);

                // Si supera el umbral, se reemplaza con preCol
                // Si no, se mantiene lo anterior (postCol)
                return lerp(postCol, preCol, mask);
            }
            ENDCG
        }
    }
}
