Shader "Unlit/RefractionURP"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _Opacity("Opacity", Range(0, 1)) = 1
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _IndexOfRefraction("Index of Refraction", Range(-1, 1)) = 0
        _ChromaticAberration("Chromatic Aberration", Range(0, 0.3)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
            };

            sampler2D _BaseMap;
            sampler2D _NormalMap;
            sampler2D _CameraOpaqueTexture;
            float _Opacity;
            float _Smoothness;
            float _Metallic;
            float _IndexOfRefraction;
            float _ChromaticAberration;

            Varyings vert(Attributes input)
            {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                o.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                o.worldPos = worldPos;
                o.worldNormal = TransformObjectToWorldNormal(input.normalOS);
                o.uv = input.uv;
                o.screenPos = ComputeScreenPos(o.positionHCS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 normal = UnpackNormal(tex2D(_NormalMap, i.uv));
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float distortion = (1.0 - saturate(dot(normal, viewDir))) * _IndexOfRefraction;
                float2 offset = normal.xy * distortion;

                float2 redUV = screenUV + offset;
                float2 greenUV = screenUV + offset * (1.0 - _ChromaticAberration);
                float2 blueUV = screenUV + offset * (1.0 + _ChromaticAberration);

                half red = tex2D(_CameraOpaqueTexture, redUV).r;
                half green = tex2D(_CameraOpaqueTexture, greenUV).g;
                half blue = tex2D(_CameraOpaqueTexture, blueUV).b;

                float3 refractionColor = float3(red, green, blue);
                float4 baseColor = tex2D(_BaseMap, i.uv);
                float alpha = baseColor.a * _Opacity;

                return float4(lerp(refractionColor, baseColor.rgb, alpha), alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Forward"
}
