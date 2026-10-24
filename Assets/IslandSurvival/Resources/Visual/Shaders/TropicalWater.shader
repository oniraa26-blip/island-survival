Shader "IslandSurvival/Tropical Water"
{
    Properties
    {
        _DeepColor ("Deep Color", Color) = (0.015,0.31,0.48,0.88)
        _ShallowColor ("Shallow Color", Color) = (0.08,0.75,0.72,0.78)
        _WaveAmplitude ("Wave Amplitude", Range(0,0.2)) = 0.055
        _WaveSpeed ("Wave Speed", Range(0,3)) = 0.8
        _Lagoon ("Lagoon", Range(0,1)) = 0
        _FoamOnly ("Foam Only", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }
        Pass
        {
            Name "WaterForward"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor, _ShallowColor;
                half _WaveAmplitude, _WaveSpeed, _Lagoon, _FoamOnly;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half fog : TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0; UNITY_SETUP_INSTANCE_ID(input); UNITY_TRANSFER_INSTANCE_ID(input, output); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float3 positionOS = input.positionOS.xyz;
                float3 initialWS = TransformObjectToWorld(positionOS);
                positionOS.y += (sin(initialWS.x * 0.09 + _Time.y * _WaveSpeed) + sin(initialWS.z * 0.14 - _Time.y * _WaveSpeed * 0.7)) * _WaveAmplitude;
                VertexPositionInputs position = GetVertexPositionInputs(positionOS);
                output.positionCS = position.positionCS; output.positionWS = position.positionWS;
                output.fog = ComputeFogFactor(position.positionCS.z); return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half coast = saturate((input.positionWS.z + 78.0h) / 80.0h);
                half shallow = max(coast, _Lagoon);
                half pulse = sin((input.positionWS.x + input.positionWS.z) * 0.12h + _Time.y * 1.4h) * 0.035h;
                half4 color = lerp(_DeepColor, _ShallowColor, saturate(shallow + pulse));
                color = lerp(color, _ShallowColor, _FoamOnly);
                color.rgb = MixFog(color.rgb, input.fog); return color;
            }
            ENDHLSL
        }
    }
}
