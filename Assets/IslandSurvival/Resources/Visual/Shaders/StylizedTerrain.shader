Shader "IslandSurvival/Stylized Terrain Blend"
{
    Properties
    {
        _DrySandColor ("Dry Sand", Color) = (0.96,0.76,0.43,1)
        _WetSandColor ("Wet Sand", Color) = (0.76,0.56,0.32,1)
        _GrassColor ("Grass", Color) = (0.20,0.62,0.25,1)
        _DirtColor ("Dirt", Color) = (0.52,0.34,0.16,1)
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
                half4 _DrySandColor, _WetSandColor, _GrassColor, _DirtColor;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; half3 normalOS : NORMAL; half4 color : COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half3 normalWS : TEXCOORD1; half4 weights : COLOR; half fog : TEXCOORD2; UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0; UNITY_SETUP_INSTANCE_ID(input); UNITY_TRANSFER_INSTANCE_ID(input, output); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS; output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS); output.weights = input.color;
                output.fog = ComputeFogFactor(position.positionCS.z); return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 weights = max(input.weights, 0.0001h); weights /= weights.r + weights.g + weights.b + weights.a;
                half3 albedo = _DrySandColor.rgb * weights.r + _WetSandColor.rgb * weights.g + _GrassColor.rgb * weights.b + _DirtColor.rgb * weights.a;
                half broadVariation = sin(input.positionWS.x * 0.11h + input.positionWS.z * 0.047h) * sin(input.positionWS.z * 0.13h - input.positionWS.x * 0.031h);
                half fineVariation = sin(input.positionWS.x * 0.43h + input.positionWS.z * 0.37h);
                albedo *= 1.0h + broadVariation * 0.045h + fineVariation * 0.012h;
                half3 normal = normalize(input.normalWS); Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half diffuse = saturate(dot(normal, mainLight.direction));
                half3 light = SampleSH(normal) + mainLight.color * (0.28h + diffuse * 0.72h) * mainLight.shadowAttenuation;
                return half4(MixFog(albedo * light, input.fog), 1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
