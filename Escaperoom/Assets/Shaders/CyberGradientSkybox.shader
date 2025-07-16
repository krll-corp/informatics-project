Shader "Skybox/CyberGradientSkybox"
{
    Properties
    {
        _Color1 ("Top Color", Color) = (0.0, 0.1, 0.4, 1.0)
        _Color2 ("Bottom Color", Color) = (0.0, 0.6, 1.0, 1.0)
        _PulseStrength ("Pulse Strength", Range(0, 1)) = 0.3
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 1.0
        _RadialGradient ("Radial Falloff", Range(0.0, 10.0)) = 3.0
    }

    SubShader
    {
        Tags { "Queue" = "Background" }
        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 direction : TEXCOORD0;
            };

            float4 _Color1;
            float4 _Color2;
            float _PulseStrength;
            float _PulseSpeed;
            float _RadialGradient;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.direction = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.direction);
                float t = dir.y * 0.5 + 0.5;

                // Radial glow
                float radial = length(dir.xy);
                float radialFalloff = exp(-radial * _RadialGradient);

                // Pulse animation
                float pulse = sin(_Time.y * _PulseSpeed + radial * 5.0) * _PulseStrength;

                // Gradient blend with animation
                float blend = saturate(t + pulse * radialFalloff);

                return lerp(_Color1, _Color2, blend);
            }
            ENDCG
        }
    }
    FallBack "Skybox/Procedural"
}
