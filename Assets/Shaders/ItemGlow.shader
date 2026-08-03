Shader "KeepWandering/ItemGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [HDR] _GlowColor ("Glow Color", Color) = (1, 0.61, 0, 1)
        _GlowWidth ("Glow Width (texels)", Range(0, 64)) = 12
        _GlowFalloff ("Glow Falloff", Range(0.25, 4)) = 1

        _PulseSpeed ("Pulse Speed", Float) = 2
        _PulseDepth ("Pulse Depth", Range(0, 1)) = 0.5

        [Toggle] _IsGlowing ("Is Glowing", Float) = 0

        // Must match the sprite's "Pixels Per Unit" import setting.
        _PixelsPerUnit ("Pixels Per Unit", Float) = 100

        // Optional. xy = UV min, zw = UV max of this sprite inside its atlas.
        // Left at (0,0,1,1) it is a harmless no-op; set it only if a sprite's
        // artwork runs close to the edge of its atlas cell.
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

            // Sample directions per ring. Higher = rounder glow, more texture reads.
            #define TAP_COUNT  16
            // Concentric rings sampled out to the glow radius. More = smoother falloff.
            #define RING_COUNT 4

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                fixed4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4    _MainTex_TexelSize;   // xy = 1/width, 1/height

            fixed4 _Color;
            fixed4 _GlowColor;
            float  _GlowWidth;
            float  _GlowFalloff;
            float  _PulseSpeed;
            float  _PulseDepth;
            float  _IsGlowing;
            float  _PixelsPerUnit;
            float4 _SpriteRect;

            // Alpha of the sprite at uv, forced to 0 outside this sprite's atlas rect
            // so the glow can never pick up a neighbouring sprite's pixels.
            float SampleAlpha(float2 uv)
            {
                float2 lo = step(_SpriteRect.xy, uv);
                float2 hi = step(uv, _SpriteRect.zw);
                float inside = lo.x * lo.y * hi.x * hi.y;
                return tex2D(_MainTex, clamp(uv, _SpriteRect.xy, _SpriteRect.zw)).a * inside;
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float active = step(0.5, _IsGlowing);

                // Push each corner outward by the glow width, and shift its UV by the
                // exactly equivalent amount. Because both offsets are additive and in
                // proportion (uv per object unit = PixelsPerUnit * texelSize), the
                // sprite content maps to precisely the same screen pixels as before -
                // all that is added is transparent margin for the glow to live in.
                float2 dir    = sign(v.vertex.xy);
                float  posPad = (_GlowWidth / max(_PixelsPerUnit, 0.0001)) * active;
                float2 uvPad  = _MainTex_TexelSize.xy * _GlowWidth * active;

                float4 vertexOS = v.vertex;
                vertexOS.xy += dir * posPad;

                o.pos   = UnityObjectToClipPos(vertexOS);
                o.uv    = v.uv + dir * uvPad;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float  srcA = SampleAlpha(i.uv);
                fixed4 base = tex2D(_MainTex, clamp(i.uv, _SpriteRect.xy, _SpriteRect.zw));
                base *= i.color * _Color;
                base.a = srcA * i.color.a * _Color.a;

                if (_IsGlowing < 0.5) return base;

                // Pulse oscillates between (1 - PulseDepth) and 1.
                float pulse = lerp(1.0 - _PulseDepth, 1.0,
                                   sin(_Time.y * _PulseSpeed) * 0.5 + 0.5);

                float2 radiusUV = _MainTex_TexelSize.xy * _GlowWidth;

                // Walk outward ring by ring. The nearest ring that finds solid pixels
                // determines how bright this point in the halo is, giving a smooth
                // falloff from the silhouette outward.
                float reach = 0.0;

                [unroll]
                for (int r = 1; r <= RING_COUNT; r++)
                {
                    float ringFrac = (float)r / (float)RING_COUNT;
                    float ringHit  = 0.0;

                    [unroll]
                    for (int t = 0; t < TAP_COUNT; t++)
                    {
                        // Offset alternate rings by half a step so taps interleave.
                        float angle = 6.28318530718 *
                                      (((float)t + 0.5 * (float)(r % 2)) / (float)TAP_COUNT);
                        float2 tapDir;
                        sincos(angle, tapDir.y, tapDir.x);
                        ringHit = max(ringHit, SampleAlpha(i.uv + tapDir * radiusUV * ringFrac));
                    }

                    // Closer rings contribute more, so intensity decays with distance.
                    reach = max(reach, ringHit * (1.0 - ringFrac + (1.0 / (float)RING_COUNT)));
                }

                float halo = saturate(reach);
                halo = pow(halo, _GlowFalloff);

                // The halo only exists where the sprite itself is not.
                halo *= (1.0 - srcA);

                float3 glowRGB = _GlowColor.rgb * pulse;
                float  glowA   = halo * _GlowColor.a * pulse;

                // Composite the sprite over the glow (source-over).
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
