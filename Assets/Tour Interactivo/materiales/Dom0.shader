Shader "Custom/HDRI_Dome_URP"
{
    Properties
    {
        [MainTexture] _MainTex ("HDRI Texture (2D Lat-Long)", 2D) = "white" {}
        _Exposure ("Exposure (Brillo)", Range(0.0, 5.0)) = 1.0
        _Rotation ("Rotation (Grados)", Range(0.0, 360.0)) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "Queue" = "Background" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Pass
        {
            Name "DomeUnlit"
            // Dibuja solo el interior de la esfera
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float _Exposure;
                float _Rotation;
            CBUFFER_END

            // Función para rotar en el eje Y
            float3 RotateAroundY(float3 dir, float degrees)
            {
                float rad = radians(degrees);
                float s = sin(rad);
                float c = cos(rad);
                return float3(
                    dir.x * c - dir.z * s,
                    dir.y,
                    dir.x * s + dir.z * c
                );
            }

            // Convierte dirección 3D a coordenadas UV equirectangulares (Latitud-Longitud)
            float2 DirectionToEquirectangularUV(float3 dir)
            {
                float3 d = normalize(dir);
                float u = atan2(d.x, d.z) / (2.0 * PI) + 0.5;
                float v = asin(clamp(d.y, -1.0, 1.0)) / PI + 0.5;
                return float2(u, v);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Rotamos la dirección de muestreo
                float3 dir = RotateAroundY(IN.positionOS, _Rotation);
                
                // Calculamos UV esféricas para evitar distorsiones en los polos
                float2 uv = DirectionToEquirectangularUV(dir);

                // Muestreamos el HDRI y multiplicamos por la exposición
                half4 hdriColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                return half4(hdriColor.rgb * _Exposure, 1.0);
            }
            ENDHLSL
        }
    }
}