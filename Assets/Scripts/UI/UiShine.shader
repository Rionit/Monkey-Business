Shader "UI/HoverShine_Reset"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}

        _ShineColor("Shine Color", Color) = (1,1,1,1)
        _ShineAngle("Shine Angle", Range(0, 360)) = 45

        _ShineWidth("Shine Width", Range(0.005, 0.2)) = 0.08
        _ShineSpeed("Shine Speed", Float) = 1
        _Intensity("Intensity", Range(0,5)) = 1

        _Hover("Hover", Range(0,1)) = 0
        _ShineTime("Shine Time", Float) = 0
    }

        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float4 color : COLOR;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 color : COLOR;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;

                float4 _ShineColor;
                float _ShineAngle;
                float _ShineWidth;
                float _ShineSpeed;
                float _Intensity;
                float _Hover;
                float _ShineTime;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.color = v.color;
                    return o;
                }

                float2 dirFromAngle(float a)
                {
                    float r = radians(a);
                    return float2(cos(r), sin(r));
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                    if (_Hover < 0.01)
                        return col;

                    float2 dir = dirFromAngle(_ShineAngle);
                    float proj = dot(i.uv, dir);

                    // 🔥 controlled animation time (NOT global time)
                    float t = _ShineTime * _ShineSpeed;

                    float shine =
                        smoothstep(t, t + _ShineWidth, proj) -
                        smoothstep(t + _ShineWidth, t + _ShineWidth * 2, proj);

                    shine *= _Intensity * _Hover;

                    col.rgb += _ShineColor.rgb * shine;

                    return col;
                }
                ENDCG
            }
        }
}