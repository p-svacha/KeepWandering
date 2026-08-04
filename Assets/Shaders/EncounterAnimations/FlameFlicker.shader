Shader "KeepWandering/FlameFlicker"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _SwaySpeed ("Sway Speed", Float) = 2.5
        _SwayAmount ("Sway Amount (UV units)", Range(0, 0.2)) = 0.035
        _SwayHeightPower ("Sway Height Bias", Range(0.5, 4)) = 2

        _FlickerSpeed ("Flicker Speed", Float) = 9
        _FlickerAmount ("Flicker UV Jitter", Range(0, 0.05)) = 0.01

        _PulseSpeed ("Brightness Pulse Speed", Float) = 4
        _PulseAmount ("Brightness Pulse Amount", Range(0, 0.5)) = 0.12
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "RenderType"        = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderPipeline"    = "UniversalPipeline"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            fixed4    _Color;

            float _SwaySpeed;
            float _SwayAmount;
            float _SwayHeightPower;

            float _FlickerSpeed;
            float _FlickerAmount;

            float _PulseSpeed;
            float _PulseAmount;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // v (0 = base of flame, 1 = tip) biases sway/flicker so the base stays
                // anchored and the tip moves the most, matching how real flame licks bend.
                float heightBias = pow(saturate(i.uv.y), _SwayHeightPower);

                // Slow side-to-side sway.
                float sway = sin(_Time.y * _SwaySpeed) * _SwayAmount * heightBias;

                // Faster, smaller jitter layered on top for a flickering edge.
                float flicker = sin(_Time.y * _FlickerSpeed + i.uv.y * 6.0) * _FlickerAmount * heightBias;

                float2 distortedUV = i.uv + float2(sway + flicker, 0);

                fixed4 texColor = tex2D(_MainTex, distortedUV);
                fixed4 col = texColor * i.color * _Color;

                // Independent brightness pulse, not tied to the horizontal distortion,
                // so the flame breathes as well as sways.
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed + 1.7) * _PulseAmount;
                col.rgb *= pulse;

                return col;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
