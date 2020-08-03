
//https://github.com/przemyslawzaworski/Unity3D-CG-programming
Shader "BEEP/Highlighter"
{
    Properties
    {
        _normal_multiplier("Normal Multiplier", Range(0.0, 2.0)) = 0.01
        _highlighter_color("Highlighter Color", Color) = (0,0,0,1)
    }
        SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Name "Highlighter"
            CGPROGRAM
            #pragma vertex vertex_shader
            #pragma fragment pixel_shader
            #pragma target 3.0

            float _normal_multiplier;
            float4 _highlighter_color;

            float4 vertex_shader(float4 vertex:POSITION,float3 normal : NORMAL) :SV_POSITION
            {
                float4 clipPosition = UnityObjectToClipPos(vertex);
                float3 clipNormal = mul((float3x3) UNITY_MATRIX_VP, mul((float3x3) UNITY_MATRIX_M, normal));
                float2 offset = normalize(clipNormal.xy) / _ScreenParams.xy * _normal_multiplier * clipPosition.w * 2;
                clipPosition.xy += offset;

                return clipPosition;
            }

            float4 pixel_shader(float4 vertex:SV_POSITION) : COLOR
            {
                return _highlighter_color;
            }
            ENDCG
        }
    }
}