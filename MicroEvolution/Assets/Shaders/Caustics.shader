Shader "MicroEvolution/Caustics"
{
    Properties
    {
        _Color ("Color", Color) = (0.55, 0.9, 1, 0.12)
        _Scale ("Scale", Float) = 3.5
        _Speed ("Speed", Float) = 0.35
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            float _Scale;
            float _Speed;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float3 wpos:TEXCOORD1; };
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            float hash(float2 p){ return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
            float noise(float2 p)
            {
                float2 i = floor(p); float2 f = frac(p);
                float a = hash(i);
                float b = hash(i + float2(1,0));
                float c = hash(i + float2(0,1));
                float d = hash(i + float2(1,1));
                float2 u = f*f*(3.0-2.0*f);
                return lerp(a,b,u.x) + (c-a)*u.y*(1.0-u.x) + (d-b)*u.x*u.y;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float t = _Time.y * _Speed;
                float2 p = i.wpos.xy * _Scale;
                float n1 = noise(p + t);
                float n2 = noise(p * 1.7 - t * 0.8 + 10.0);
                float c = pow(saturate(n1 * n2 * 1.8), 2.2);
                fixed4 col = _Color;
                col.a *= c;
                return col;
            }
            ENDCG
        }
    }
}
