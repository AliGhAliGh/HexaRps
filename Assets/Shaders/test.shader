Shader "Example/Portal"
{
    SubShader
    {
        Tags { "Queue" = "Geometry+1" "RenderType" = "Opaque" }
        Pass
        {
            // Step 1: Render the portal mask to the stencil buffer
            Stencil
            {
                Ref 1              // Set stencil buffer value to 1
                Comp Always        // Always pass stencil test
                Pass Replace       // Replace stencil value with Ref
            }
            ColorMask 0            // Do not render to color buffer
        }

        Pass
        {
            // Step 2: Render the scene inside the portal where stencil == 1
            Stencil
            {
                Ref 1              // Compare against stencil buffer value 1
                Comp Equal         // Pass only if stencil == 1
                Pass Keep          // Keep the stencil buffer value
            }

            // Render a placeholder color or your custom scene content
            // You can use a texture or procedural content
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
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

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv); // Render texture content
            }
            ENDCG
        }

        Pass
        {
            // Step 3: Render the portal frame on top of everything
            // No stencil operations here; just render the frame
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _FrameTex;
            float4 _FrameTex_ST;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _FrameTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_FrameTex, i.uv); // Render portal frame texture
            }
            ENDCG
        }
    }
}
