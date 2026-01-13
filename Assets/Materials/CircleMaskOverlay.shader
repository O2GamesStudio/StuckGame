Shader "UI/CircleMaskOverlay_URP"
{
    Properties
    {
        [HideInInspector] _MainTex ("Main Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Overlay Color", Color) = (0,0,0,0.5)
        _Center ("Center (0-1)", Vector) = (0.5,0.5,0,0)
        _Radius ("Radius (0-1)", Float) = 0.3
        _Feather ("Feather", Float) = 0.01
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP 핵심 라이브러리 포함
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // SRP Batcher 호환을 위한 CBUFFER 선언
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _Center;
                float _Radius;
                float _Feather;
            CBUFFER_END

            Varyings vert (Attributes input)
            {
                Varyings output;
                // 오브젝트 공간 좌표를 클립 공간으로 변환 (URP 방식)
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // // 원점 중심 계산
                // float2 d = input.uv - _Center.xy;

                // // 화면 비율 보정 (URP _ScreenParams 사용)
                // float aspect = _ScreenParams.x / _ScreenParams.y;
                // d.x *= aspect;

                // float dist = length(d);

                // // 부드러운 경계 계산
                // half mask = smoothstep(_Radius, _Radius + _Feather, dist);

                // half4 color = _BaseColor;
                // color.a *= mask;

                // return color;
                
                //원을 선명하게
                float2 d = input.uv - _Center.xy;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                d.x *= aspect;

                float dist = length(d);

                // 1픽셀 정도의 전이 폭
                float w = fwidth(dist);

                // 안쪽(투명) / 바깥(검정) 구조 유지하려면 dist가 Radius보다 크면 1이 되게
                half mask = smoothstep(_Radius, _Radius + w, dist);

                half4 color = _BaseColor;
                color.a *= mask;
                return color;

            }
            ENDHLSL
        }
    }
}