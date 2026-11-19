Shader "Custom/CompleteSurfaceShader_URP"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _SpecMap ("Specular Map (RGB)", 2D) = "white" {}
        _SpecColor ("Specular Color", Color) = (1,1,1,1)
        _Shininess ("Shininess", Range(0.01, 100)) = 20.0
        _HeightMap ("Height Map", 2D) = "black" {}
        _Parallax ("Height Scale", Range(0.005, 0.08)) = 0.02
        _AOMap ("Ambient Occlusion Map", 2D) = "white" {}
        _EmissionMap ("Emissive Map", 2D) = "black" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _EMISSION

            // Librerías core e iluminación de URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float2 uv               : TEXCOORD0;
                float3 lightDirTangent  : TEXCOORD1;
                float3 viewDirTangent   : TEXCOORD2;
            };

            // Declaración de texturas y samplers en URP
            TEXTURE2D(_MainTex);     SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);     SAMPLER(sampler_BumpMap);
            TEXTURE2D(_SpecMap);     SAMPLER(sampler_SpecMap);
            TEXTURE2D(_HeightMap);   SAMPLER(sampler_HeightMap);
            TEXTURE2D(_AOMap);       SAMPLER(sampler_AOMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);

            // CBUFFER necesario para compatibilidad con SRP Batcher
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _SpecColor;
                float _Shininess;
                float _Parallax;
                half4 _EmissionColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                // Cálculo de espacio tangente con funciones de URP
                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                float3x3 worldToTangent = float3x3(
                    normalInput.tangentWS,
                    normalInput.bitangentWS,
                    normalInput.normalWS
                );

                // Obtener luz principal de URP
                Light mainLight = GetMainLight();
                OUT.lightDirTangent = mul(worldToTangent, mainLight.direction);

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(positionWS);
                OUT.viewDirTangent = mul(worldToTangent, viewDirWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 V = normalize(IN.viewDirTangent);

                // 1. Height Map (Parallax)
                float height = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, IN.uv).r;
                float2 offset = V.xy * (height * _Parallax);
                float2 uv = IN.uv + offset;

                // 2. Albedo y AO
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _Color;
                half ao = SAMPLE_TEXTURE2D(_AOMap, sampler_AOMap, uv).r;

                // 3. Normal Map
                float3 N = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv));
                float3 L = normalize(IN.lightDirTangent);

                // Obtener color de luz principal de URP
                Light mainLight = GetMainLight();
                half3 lightColor = mainLight.color;

                // 4. Difusa
                float diff = max(0.0, dot(N, L));
                half3 diffuse = lightColor * albedo.rgb * diff;

                // 5. Specular Map (Blinn-Phong)
                float3 H = normalize(L + V);
                float specPower = max(0.0, dot(N, H));
                float specFactor = pow(specPower, _Shininess);
                half3 specMask = SAMPLE_TEXTURE2D(_SpecMap, sampler_SpecMap, uv).rgb;
                half3 specular = lightColor * _SpecColor.rgb * specFactor * specMask;

                // 6. Emissive Map
                half3 emission = half3(0, 0, 0);
                #if defined(_EMISSION)
                    emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv).rgb * _EmissionColor.rgb;
                #endif

                // Combinación final (usando luz ambiental de URP)
                half3 ambient = SampleSH(N) * albedo.rgb * ao;
                half3 finalColor = (ambient + diffuse + specular) * ao + emission;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}