Shader "Custom/ColorBlindnessFilter"
{
    Properties
    {
        _Severity("Correction Intensity", Range(-1.0, 1.0)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        ZWrite Off
        Cull Off

        Pass
        {
            Name "ColorBlindnessPass"

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float _Severity;

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Imagem original da câmera
                float4 col = SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.texcoord
                );

                float3 rgb = col.rgb;

                // sRGB -> Linear

                float3 linearRgb = pow(
                    max(rgb, 0.0),
                    2.2
                );

                // PARÂMETRO DO FILTRO
                // i varia de -1 a +1
                // i < 0:
                // transfere informação do vermelho para o azul
                // i > 0:
                // transfere informação do verde para o azul

                float i = clamp(_Severity, -1.0, 1.0);

                float alpha = 0.0;
                float beta = 0.0;

                if (i < 0.0)
                {
                    alpha = -i;
                }
                else
                {
                    beta = i;
                }

                float gamma = 1.0 - (alpha + beta);

                // FILTRO DE ACESSIBILIDADE

                float3 correctedRgb;

                correctedRgb.r = linearRgb.r;
                correctedRgb.g = linearRgb.g;

                correctedRgb.b =
                    (linearRgb.r * alpha) +
                    (linearRgb.g * beta) +
                    (linearRgb.b * gamma);

                correctedRgb = saturate(correctedRgb);

                // Linear -> sRGB

                rgb = pow(
                    max(correctedRgb, 0.0),
                    1.0 / 2.2
                );

                return float4(rgb, col.a);
            }

            ENDHLSL
        }
    }
}