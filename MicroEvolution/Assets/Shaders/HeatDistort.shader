Shader "Hidden/MicroEvolution/HeatDistort"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Amount ("Amount", Range(0, 0.05)) = 0.008
        _Speed ("Speed", Range(0, 5)) = 1.2
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Amount;
            float _Speed;
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert(appdata_img v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.texcoord; return o; }
            fixed4 frag(v2f i):SV_Target
            {
                float t = _Time.y * _Speed;
                float2 uv = i.uv;
                uv.x += sin(uv.y * 40.0 + t) * _Amount;
                uv.y += cos(uv.x * 30.0 + t * 0.8) * _Amount * 0.7;
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
