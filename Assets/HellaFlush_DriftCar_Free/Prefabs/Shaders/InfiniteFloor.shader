Shader "Custom/InfiniteFloor"

{

    Properties

    {

        _MainTex ("Base (RGB)", 2D) = "white" {} 

        _TileScale ("Tile Scale", Float) = 1.0

    }

    SubShader

    {

        Tags { "RenderType"="Opaque" }

        LOD 100



        Pass

        {

            CGPROGRAM

            #pragma vertex vert

            #pragma fragment frag

            #include "UnityCG.cginc"



            sampler2D _MainTex;

            float    _TileScale;



            struct appdata

            {

                float4 vertex : POSITION;

            };



            struct v2f

            {

                float4 pos : SV_POSITION;

                float2 uv  : TEXCOORD0;

            };



            v2f vert (appdata v)

            {

                v2f o;

                // standard MVP

                o.pos = UnityObjectToClipPos(v.vertex);

                // world position of the vertex

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                // use X,Z as UVs, scaled by _TileScale

                o.uv = worldPos.xz * _TileScale;

                return o;

            }



            fixed4 frag (v2f i) : SV_Target

            {

                // sample the texture with tiled world-space UV

                return tex2D(_MainTex, i.uv);

            }

            ENDCG

        }

    }

    FallBack "Diffuse"

}