Shader "KeepWandering/ItemGlowOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [HDR] _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width (texels)", Range(0, 64)) = 8
        _PulseSpeed ("Pulse Speed", Float) = 2
        _PulseWidth ("Pulse Width", Range(0, 1)) = 0.5
        [Toggle] _IsGlowing ("Is Glowing", Float) = 0

        // xy = UV min, zw = UV max of this sprite within the atlas texture.
        // Defaults to the full texture, which is correct for single (non-atlas) sprites.
        _SpriteRect ("Sprite UV Rect (xy = min, zw = max)", Vector) = (0, 0, 1, 1)

        // --- Standard UI properties, so this also works on UnityEngine.UI.Image ---
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "RenderType"        = "Transparent"
            "IgnoreProjector"   = "True"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.0
            #include "UnityCG.cginc"

            // Number of sample directions per ring. Higher = rounder outline, more texture reads.
            #define TAP_COUNT  16
            // Number of concentric rings. 2 avoids gaps in thin shapes at large widths.
            #define RING_COUNT 2

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4    _MainTex_TexelSize;   // xy = 1/width, 1/height

            fixed4 _Color;
            fixed4 _GlowColor;
            float  _OutlineWidth;
            float  _PulseSpeed;
            float  _PulseWidth;
            float  _IsGlowing;
            float4 _SpriteRect;

            // Outline thickness expressed in UV units, per axis.
            float2 OutlinePadUV()
            {
                return _MainTex_TexelSize.xy * _OutlineWidth;
            }

            // 1 if the UV lies within this sprite's rect in the atlas, 0 otherwise.
            // This is what stops the outline from sampling a neighbouring sprite.
            float InsideSpriteRect(float2 uv)
            {
                float2 lo = step(_SpriteRect.xy, uv);          // 1 where uv >= min
                float2 hi = step(uv, _SpriteRect.zw);          // 1 where uv <= max
                return lo.x * lo.y * hi.x * hi.y;
            }

            // Alpha of the sprite at uv, forced to 0 outside the sprite's own rect.
            float SampleAlphaMasked(float2 uv)
            {
                float inside = InsideSpriteRect(uv);
                float2 clamped = clamp(uv, _SpriteRect.xy, _SpriteRect.zw);
                return tex2D(_MainTex, clamped).a * inside;
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float2 uvMin  = _SpriteRect.xy;
                float2 uvMax  = _SpriteRect.zw;
                float2 uvSize = max(uvMax - uvMin, 1e-6);

                // Grow the quad by exactly the outline width on every side, so there are
                // real pixels for the glow to be rasterized into. The UVs are scaled about
                // the same centre by the same factor, so the sprite itself is unchanged in
                // size and position - only transparent margin is added around it.
                float2 scale = (uvSize + 2.0 * OutlinePadUV()) / uvSize;

                // No expansion at all when not glowing, so nothing shifts or overdraws.
                scale = lerp(float2(1, 1), scale, step(0.5, _IsGlowing));

                float2 uvCenter = (uvMin + uvMax) * 0.5;

                float4 vertexOS = v.vertex;
                vertexOS.xy *= scale;              // requires a centred pivot (see notes)

                o.pos   = UnityObjectToClipPos(vertexOS);
                o.uv    = uvCenter + (v.uv - uvCenter) * scale;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Raw sprite alpha, masked so the added margin reads as empty.
                float  srcA = SampleAlphaMasked(i.uv);
                fixed4 base = tex2D(_MainTex, clamp(i.uv, _SpriteRect.xy, _SpriteRect.zw));
                base *= i.color * _Color;
                base.a = srcA * i.color.a * _Color.a;

                if (_IsGlowing < 0.5) return base;

                // Pulse between (1 - PulseWidth) and 1.
                float pulse = lerp(1.0 - _PulseWidth, 1.0,
                                   sin(_Time.y * _PulseSpeed) * 0.5 + 0.5);

                // Dilate the alpha outward in rings to build the outline mask.
                float2 pad = OutlinePadUV();
                float dilated = 0.0;

                [unroll]
                for (int r = 1; r <= RING_COUNT; r++)
                {
                    float ringScale = (float)r / (float)RING_COUNT;

                    [unroll]
                    for (int t = 0; t < TAP_COUNT; t++)
                    {
                        float angle = 6.28318530718 * ((float)t / (float)TAP_COUNT);
                        float2 dir;
                        sincos(angle, dir.y, dir.x);
                        dilated = max(dilated, SampleAlphaMasked(i.uv + dir * pad * ringScale));
                    }
                }

                // The ring is whatever the dilation covers that the sprite itself does not.
                float outline = saturate(dilated - srcA);

                float3 glowRGB = _GlowColor.rgb * pulse;
                float  glowA   = outline * _GlowColor.a * pulse;

                // Composite the sprite over the glow ("source over").
                float  outA   = base.a + glowA * (1.0 - base.a);
                float3 outRGB = (base.rgb * base.a + glowRGB * glowA * (1.0 - base.a))
                                / max(outA, 1e-5);

                return fixed4(outRGB, outA);
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
