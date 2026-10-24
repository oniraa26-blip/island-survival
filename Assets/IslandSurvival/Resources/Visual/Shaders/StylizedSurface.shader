Shader "IslandSurvival/Stylized Surface"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.08
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Smoothness;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; half3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half3 normalWS : TEXCOORD1; half fog : TEXCOORD2; UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input); UNITY_TRANSFER_INSTANCE_ID(input, output); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS; output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.fog = ComputeFogFactor(position.positionCS.z); return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half3 normal = normalize(input.normalWS);
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half diffuse = saturate(dot(normal, mainLight.direction));
                half3 light = SampleSH(normal) + mainLight.color * (0.25h + diffuse * 0.75h) * mainLight.shadowAttenuation;
                half3 color = MixFog(_BaseColor.rgb * light, input.fog);
                return half4(color, _BaseColor.a);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
