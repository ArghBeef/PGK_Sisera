Shader "Custom/URP/HalftoneToon_Emissive"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Tint", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (0.12,0.13,0.2,1)

        _DotScale ("Dot Scale", Float) = 16
        _DotStrength ("Dot Strength", Range(0,1)) = 0.45
        _Threshold ("Light Threshold", Range(0,1)) = 0.45
        _Softness ("Lighting Softness", Range(0.001,0.5)) = 0.18

        _EmissionMask ("Emission Mask", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (1,0,0,1)
        _EmissionStrength ("Emission Strength", Float) = 5

        _PulseSpeed ("Pulse Speed", Float) = 8
        _PulseAmount ("Pulse Amount", Range(0,1)) = 0.4
        _VibrateSpeed ("Vibrate Speed", Float) = 40
        _VibrateAmount ("Vibrate Amount", Range(0,0.05)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_EmissionMask);
            SAMPLER(sampler_EmissionMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _EmissionMask_ST;

                float4 _BaseColor;
                float4 _ShadowColor;

                float _DotScale;
                float _DotStrength;
                float _Threshold;
                float _Softness;

                float4 _EmissionColor;
                float _EmissionStrength;

                float _PulseSpeed;
                float _PulseAmount;
                float _VibrateSpeed;
                float _VibrateAmount;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float2 emissionUV : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionOS = IN.positionOS.xyz;

                // Tiny vertex vibration only on glowing parts
                float2 emissionUV = TRANSFORM_TEX(IN.uv, _EmissionMask);
                float mask = SAMPLE_TEXTURE2D_LOD(_EmissionMask, sampler_EmissionMask, emissionUV, 0).r;

                float vibration =
                    sin(_Time.y * _VibrateSpeed + positionOS.x * 30 + positionOS.y * 20)
                    * _VibrateAmount
                    * mask;

                positionOS += IN.normalOS * vibration;

                OUT.positionWS = TransformObjectToWorld(positionOS);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.emissionUV = emissionUV;

                return OUT;
            }

            float stableDots(float2 uv)
            {
                float2 cell = frac(uv) - 0.5;
                return length(cell);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;
                baseTex *= _BaseColor.rgb;

                float3 normalWS = normalize(IN.normalWS);

                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                float lightAmount = saturate(dot(normalWS, mainLight.direction));
                lightAmount *= mainLight.shadowAttenuation;

                float toon = smoothstep(
                    _Threshold - _Softness,
                    _Threshold + _Softness,
                    lightAmount
                );

                float3 absNormal = abs(normalWS);
                float2 dotUV;

                if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
                    dotUV = IN.positionWS.xz;
                else if (absNormal.x > absNormal.z)
                    dotUV = IN.positionWS.zy;
                else
                    dotUV = IN.positionWS.xy;

                float dotPattern = stableDots(dotUV * _DotScale);

                float shadowAmount = 1.0 - lightAmount;
                float dotSize = lerp(0.04, 0.32, shadowAmount * _DotStrength);

                float dots = 1.0 - smoothstep(dotSize, dotSize + 0.08, dotPattern);

                float shadowMask = saturate((1.0 - toon) + dots * shadowAmount * _DotStrength);

                float3 shadowedColor = baseTex * _ShadowColor.rgb;
                float3 finalColor = lerp(baseTex, shadowedColor, shadowMask);

                // Emission / glowing eyes
                float emissionMask = SAMPLE_TEXTURE2D(_EmissionMask, sampler_EmissionMask, IN.emissionUV).r;

                float pulse =
                    1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

                float flicker =
                    1.0 + sin(_Time.y * _VibrateSpeed) * 0.08;

                float emissionPower = _EmissionStrength * pulse * flicker;

                float3 emission = _EmissionColor.rgb * emissionPower * emissionMask;

                finalColor += emission;

                return half4(finalColor, 1);
            }

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

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

            Varyings ShadowPassVertex(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                return OUT;
            }

            half4 ShadowPassFragment(Varyings IN) : SV_TARGET
            {
                return 0;
            }

            ENDHLSL
        }
    }
}