Shader "Custom/HalftoneToon"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Tint", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (0.05,0.05,0.08,1)

        _DotScale ("Dot Scale", Float) = 18
        _DotStrength ("Dot Strength", Range(0,1)) = 0.7
        _Threshold ("Light Threshold", Range(0,1)) = 0.45
        _Softness ("Lighting Softness", Range(0.001,0.5)) = 0.12
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _BaseColor;
            float4 _ShadowColor;

            float _DotScale;
            float _DotStrength;
            float _Threshold;
            float _Softness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float stableDots(float2 uv)
            {
                float2 cell = frac(uv) - 0.5;
                return length(cell);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 baseTex = tex2D(_MainTex, i.uv).rgb * _BaseColor.rgb;

                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

                float lightAmount = saturate(dot(normal, lightDir));

                // Softer toon lighting instead of harsh step flicker
                float toon = smoothstep(
                    _Threshold - _Softness,
                    _Threshold + _Softness,
                    lightAmount
                );

                // World-space dots: more stable than screen-space dots
                float3 absNormal = abs(normal);
                float2 dotUV;

                // Project dots from the most visible axis
                if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
                    dotUV = i.worldPos.xz;
                else if (absNormal.x > absNormal.z)
                    dotUV = i.worldPos.zy;
                else
                    dotUV = i.worldPos.xy;

                float dotPattern = stableDots(dotUV * _DotScale);

                float shadowAmount = 1.0 - lightAmount;
                float dotSize = lerp(0.04, 0.32, shadowAmount * _DotStrength);

                // Soft dot edge to reduce noisy flickering
                float dots = 1.0 - smoothstep(dotSize, dotSize + 0.08, dotPattern);

                float shadowMask = saturate((1.0 - toon) + dots * shadowAmount * _DotStrength);

                float3 shadowedColor = baseTex * _ShadowColor.rgb;
                float3 finalColor = lerp(baseTex, shadowedColor, shadowMask);

                return fixed4(finalColor, 1);
            }
            ENDCG
        }
    }
}