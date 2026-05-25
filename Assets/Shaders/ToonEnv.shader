Shader "Custom/ToonEnv"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Tint", Color) = (1,1,1,1)
        _Alpha ("Alpha", Range(0,1)) = 1
        [HideInInspector] _ZWrite ("ZWrite", Float) = 1

        _ShadowColor ("Shadow Tint", Color) = (0.45,0.55,0.35,1)
        _ShadowStrength ("Shadow Strength", Range(0,1)) = 0.65

        _HatchColor ("Hatch Color", Color) = (0.05,0.12,0.04,1)
        _HatchScale ("Hatch Scale", Float) = 10
        _HatchWidth ("Hatch Width", Range(0.01,0.5)) = 0.22
        _HatchStrength ("Hatch Strength", Range(0,1)) = 0.55

        _Ambient ("Minimum Ambient Light", Range(0,1)) = 0.35
        _ShadowSoftness ("Shadow Softness", Range(0.01,1)) = 0.45
        _LightStrength ("Main Light Strength", Range(0,3)) = 1
        _AdditionalLightsStrength ("Point/Spot Lights Strength", Range(0,3)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_ZWrite]
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _ShadowColor;
                float4 _HatchColor;
                float _Alpha;
                float _ZWrite;
                float _ShadowStrength;
                float _HatchScale;
                float _HatchWidth;
                float _HatchStrength;
                float _Ambient;
                float _ShadowSoftness;
                float _LightStrength;
                float _AdditionalLightsStrength;

            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
                return output;
            }

            float Hatch(float3 worldPos)
            {
                float diagonal = worldPos.x + worldPos.z;
                float pattern = frac(diagonal * _HatchScale);
                float dist = abs(pattern - 0.5);
                return 1.0 - smoothstep(_HatchWidth, _HatchWidth + 0.04, dist);
            }

            float3 GetStylizedLight(float3 positionWS, float3 normalWS, float4 shadowCoord, out float lightAmount)
            {
                Light mainLight = GetMainLight(shadowCoord);
                float mainNdotL = saturate(dot(normalWS, mainLight.direction));
                float rawShadow = mainLight.shadowAttenuation;
                float softShadow = smoothstep(0.5 - _ShadowSoftness * 0.5, 0.5 + _ShadowSoftness * 0.5, rawShadow);
                float mainLit = mainNdotL * softShadow * mainLight.distanceAttenuation;

                float totalLight = mainLit * _LightStrength;
                float3 lightColor = mainLight.color * mainLit * _LightStrength;

                #if defined(_ADDITIONAL_LIGHTS)
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint i = 0u; i < pixelLightCount; i++)
                    {
                        Light light = GetAdditionalLight(i, positionWS);
                        float ndotl = saturate(dot(normalWS, light.direction));
                        float attenuated = ndotl * light.distanceAttenuation * light.shadowAttenuation;
                        totalLight += attenuated * _AdditionalLightsStrength;
                        lightColor += light.color * attenuated * _AdditionalLightsStrength;
                    }
                #endif

                lightAmount = saturate(totalLight + _Ambient);
                return max(lightColor, _Ambient.xxx);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float lightAmount;
                float3 lightColor = GetStylizedLight(input.positionWS, normalWS, input.shadowCoord, lightAmount);

                float3 textureColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
                float3 baseColor = textureColor * _BaseColor.rgb;

                float shadowMask = saturate((1.0 - lightAmount) * _ShadowStrength);
                shadowMask = smoothstep(0.05, 0.95, shadowMask);

                float3 litColor = baseColor * lightColor;
                float3 tintedShadow = baseColor * _ShadowColor.rgb;
                float3 colorWithShadow = lerp(litColor, tintedShadow, shadowMask);

                float hatch = Hatch(input.positionWS);
                float hatchMask = hatch * shadowMask * _HatchStrength;
                float3 finalColor = lerp(colorWithShadow, _HatchColor.rgb, hatchMask);

                return float4(finalColor, _BaseColor.a * _Alpha);
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
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; };
            float3 _LightDirection;

            Varyings ShadowVertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                output.positionCS = positionCS;
                return output;
            }

            half4 ShadowFragment(Varyings input) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
