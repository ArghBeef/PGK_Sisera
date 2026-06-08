Shader "Custom/CartoonWater"
{
    Properties
    {
        _BaseColor ("Base Water Color", Color) = (0.1, 0.55, 0.9, 0.55)
        _DeepColor ("Deep Color", Color) = (0.02, 0.18, 0.45, 0.8)
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)

        _WaveTex ("Wave Noise Texture", 2D) = "white" {}
        _FoamTex ("Foam Noise Texture", 2D) = "white" {}

        _WaveSpeed ("Wave Speed", Vector) = (0.08, 0.04, -0.05, 0.03)
        _WaveScale ("Wave Scale", Float) = 1
        _WaveStrength ("Wave Strength", Float) = 0.08

        _FoamAmount ("Foam Amount", Range(0,1)) = 0.55
        _FoamSharpness ("Foam Sharpness", Range(0.1, 10)) = 4

        _Alpha ("Transparency", Range(0,1)) = 0.65
        _ToonSteps ("Toon Steps", Range(1,6)) = 3
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "CartoonWater"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float wave : TEXCOORD1;
            };

            TEXTURE2D(_WaveTex);
            SAMPLER(sampler_WaveTex);

            TEXTURE2D(_FoamTex);
            SAMPLER(sampler_FoamTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _DeepColor;
                float4 _FoamColor;
                float4 _WaveSpeed;
                float _WaveScale;
                float _WaveStrength;
                float _FoamAmount;
                float _FoamSharpness;
                float _Alpha;
                float _ToonSteps;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float2 uv1 = IN.uv * _WaveScale + _Time.y * _WaveSpeed.xy;
                float2 uv2 = IN.uv * _WaveScale * 1.7 + _Time.y * _WaveSpeed.zw;

                float wave1 = SAMPLE_TEXTURE2D_LOD(_WaveTex, sampler_WaveTex, uv1, 0).r;
                float wave2 = SAMPLE_TEXTURE2D_LOD(_WaveTex, sampler_WaveTex, uv2, 0).r;

                float wave = (wave1 + wave2) * 0.5;

                float3 pos = IN.positionOS.xyz;
                pos.y += (wave - 0.5) * _WaveStrength;

                OUT.positionHCS = TransformObjectToHClip(pos);
                OUT.uv = IN.uv;
                OUT.wave = wave;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 waveUV1 = IN.uv * _WaveScale + _Time.y * _WaveSpeed.xy;
                float2 waveUV2 = IN.uv * _WaveScale * 1.7 + _Time.y * _WaveSpeed.zw;

                float waterNoise1 = SAMPLE_TEXTURE2D(_WaveTex, sampler_WaveTex, waveUV1).r;
                float waterNoise2 = SAMPLE_TEXTURE2D(_WaveTex, sampler_WaveTex, waveUV2).r;

                float waterNoise = (waterNoise1 + waterNoise2) * 0.5;

                float toon = floor(waterNoise * _ToonSteps) / _ToonSteps;

                float4 waterColor = lerp(_DeepColor, _BaseColor, toon);

                float2 foamUV = IN.uv * _WaveScale * 2.5 + _Time.y * float2(0.05, -0.04);
                float foamNoise = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex, foamUV).r;

                float foamMask = pow(saturate(foamNoise - _FoamAmount), _FoamSharpness);
                foamMask *= 8;

                float4 finalColor = lerp(waterColor, _FoamColor, saturate(foamMask));

                finalColor.a = _Alpha;

                return finalColor;
            }
            ENDHLSL
        }
    }
}