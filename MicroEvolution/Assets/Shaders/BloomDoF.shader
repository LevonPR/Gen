Shader "Hidden/MicroEvolution/BloomDoF"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BloomTex ("Bloom", 2D) = "black" {}
        _Intensity ("Bloom Intensity", Range(0, 2)) = 0.85
        _Threshold ("Threshold", Range(0, 1.5)) = 0.55
        _SoftDoF ("Soft DoF", Range(0, 1)) = 0.35
        _Tint ("Grade Tint", Color) = (0.85, 0.95, 1.1, 1)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // 0: extract bright
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragExtract
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Threshold;
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert(appdata_img v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.texcoord; return o; }
            fixed4 fragExtract(v2f i):SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                float br = max(c.r, max(c.g, c.b));
                float k = saturate((br - _Threshold) / max(1e-3, 1.0 - _Threshold));
                return c * k;
            }
            ENDCG
        }

        // 1: blur
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragBlur
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float2 _Direction;
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert(appdata_img v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.texcoord; return o; }
            fixed4 fragBlur(v2f i):SV_Target
            {
                float2 d = _Direction * _MainTex_TexelSize.xy * 2.0;
                fixed4 c = tex2D(_MainTex, i.uv) * 0.227027;
                c += tex2D(_MainTex, i.uv + d) * 0.1945946;
                c += tex2D(_MainTex, i.uv - d) * 0.1945946;
                c += tex2D(_MainTex, i.uv + d*2) * 0.1216216;
                c += tex2D(_MainTex, i.uv - d*2) * 0.1216216;
                c += tex2D(_MainTex, i.uv + d*3) * 0.054054;
                c += tex2D(_MainTex, i.uv - d*3) * 0.054054;
                return c;
            }
            ENDCG
        }

        // 2: combine + soft edge blur + grade
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragCombine
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _BloomTex;
            float4 _MainTex_TexelSize;
            float _Intensity;
            float _SoftDoF;
            fixed4 _Tint;
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert(appdata_img v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.texcoord; return o; }
            fixed4 fragCombine(v2f i):SV_Target
            {
                fixed4 src = tex2D(_MainTex, i.uv);
                // Soft DoF: blur more toward screen edges
                float2 c = i.uv - 0.5;
                float edge = saturate(length(c) * 1.6);
                float2 px = _MainTex_TexelSize.xy * (1.0 + edge * 6.0) * _SoftDoF;
                fixed4 soft = src;
                soft += tex2D(_MainTex, i.uv + float2(px.x, 0));
                soft += tex2D(_MainTex, i.uv - float2(px.x, 0));
                soft += tex2D(_MainTex, i.uv + float2(0, px.y));
                soft += tex2D(_MainTex, i.uv - float2(0, px.y));
                soft *= 0.2;
                src = lerp(src, soft, edge * _SoftDoF);

                fixed4 bloom = tex2D(_BloomTex, i.uv);
                fixed4 col = src + bloom * _Intensity;
                col.rgb *= _Tint.rgb;
                // mild contrast
                col.rgb = saturate((col.rgb - 0.5) * 1.08 + 0.5);
                return col;
            }
            ENDCG
        }
    }
}
