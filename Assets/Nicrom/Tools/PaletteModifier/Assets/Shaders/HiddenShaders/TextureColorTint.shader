Shader"Hidden/TextureColorTint"
{	
	Properties
	{
		_MainTex ("Texture", 2D) = "grey" {}
        _TintColor ("Tint", COLOR) = (1, 1, 1, 1)
        [HideInInspector] _Linear ("Linear", Float) = 0
	}
	SubShader
	{
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _TintColor;
            float _Linear;
			
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
			
            float4 frag(v2f i) : SV_Target
            {
                float4 tex = tex2D(_MainTex, i.uv);
    
                float3 tint1 = _TintColor.rgb;
                float3 tint2 = LinearToGammaSpace(_TintColor.rgb);
                float3 tint = tint1 * (step((_Linear + 1), 1)) + tint2 * _Linear;
             
                tex.rgb *= tint;

                return tex;
            }
			ENDCG
		}
	}
}
