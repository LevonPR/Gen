Shader "MicroEvolution/GodRay"
{
    Properties
    {
        _Color ("Color", Color) = (0.55, 0.85, 1, 0.12)
        _MainTex ("Soft", 2D) = "white" {}
        _Power ("Falloff", Range(0.2, 4)) = 1.6
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Power;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert(appdata v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=TRANSFORM_TEX(v.uv,_MainTex); return o; }
            fixed4 frag(v2f i):SV_Target
            {
                float2 uv = i.uv;
                float beam = pow(saturate(1.0 - abs(uv.x - 0.5) * 2.0), _Power);
                float fade = pow(saturate(uv.y), 0.65) * pow(saturate(1.0 - uv.y), 0.35);
                fixed4 c = _Color;
                c.a *= beam * fade;
                return c;
            }
            ENDCG
        }
    }
}
