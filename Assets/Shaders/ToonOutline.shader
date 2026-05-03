Shader "Custom/Toon Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0.8, 1, 1)
        _OutlineWidth ("Outline Width", Float) = 0.04
        _Alpha ("Alpha", Range(0,1)) = 0.65
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Comic Outline"

            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float4 _OutlineColor;
            float _OutlineWidth;
            float _Alpha;

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 positionOS = input.positionOS.xyz;
                float3 normalOS = normalize(input.normalOS);

                positionOS += normalOS * _OutlineWidth;

                output.positionHCS = TransformObjectToHClip(positionOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(_OutlineColor.rgb, _Alpha);
            }

            ENDHLSL
        }
    }
}