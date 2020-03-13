Shader "Raymarch/RaymarchCam"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            // the distancefuctions are located on another script
            #include "DistanceFunctions.cginc"

            // All hte variables feeded through the camera
            sampler2D _MainTex;
            uniform sampler2D _CameraDepthTexture;
            uniform float4x4 _CamFrustrum, _CamToWorld;
            uniform float _maxDistance;
            uniform float _precision;
            uniform float _lightIntensity;
            uniform float _shadowIntensity;
            uniform float _aoIntensity;
            uniform int _iterations;
            uniform float _power;
            uniform float _scaleFactor;
            uniform float _innerSphereRad;            
            uniform float3 _modInterval;
            uniform float3 _modOffsetPos;
            uniform float3 _modOffsetRot;
            uniform float4x4 _globalTransform;
            uniform float3 _globalPosition;
            uniform float4x4 rotate45;
            uniform float _GlobalScale;
            uniform float _smoothRadius;
            uniform float4x4 _iterationTransform;
            uniform float4x4 _sectionTransform;
            uniform float3 _lightDir;
            uniform float3 _player;
            uniform fixed4 _mainColor;
            uniform fixed4 _secColor;
            uniform fixed4 _skyColor;
            uniform fixed4 _forceFieldColor;
            uniform float _forceFieldRad;
            uniform int _renderNr;
            uniform int _usePlane;
            uniform int _useMod;
            uniform int _useNormal;
            uniform int _useShadow;
            

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ray : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                half index = v.vertex.z;
                v.vertex.z = 0;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv.xy;

                o.ray = _CamFrustrum[(int)index].xyz;

                o.ray /= abs(o.ray.z);

                o.ray = mul(_CamToWorld, o.ray);

                return o;
            }

            // the distancefunction for the forcefield around the player.
            float sdforceField(float3 p)
            {
                //simple sphere
                return sdSphere(p - _player , _forceFieldRad);
            }
          
            // the distancefunction for the fractals
            float2 distanceField(float3 p)
            {
                float2 dist;

                // check if symmetry option is checked
                if(_usePlane == 2){
                    p = sdSymXYZ(p);
                }

                // check if modulor is checed
                if(_useMod == 1){
                    
                    p.x = pMod(p.x, _modInterval.x * _GlobalScale * 2);
                    p.y = pMod(p.y, _modInterval.y * _GlobalScale * 2);
                    p.z = pMod(p.z, _modInterval.z * _GlobalScale * 2);
                }
                //menger Sponge
                if(_renderNr == 1){
                    dist = sdMerger(p,_GlobalScale, _iterations,_modOffsetPos ,_iterationTransform, _globalTransform, _smoothRadius, _scaleFactor);
                }
                //merger cylinder
                else if(_renderNr == 2){
                    dist = sdMergerCyl(p,_GlobalScale, _iterations,_modOffsetPos ,_iterationTransform, _globalTransform, _smoothRadius, _scaleFactor);
                }
                //mergerPyr
                else if(_renderNr == 3){
                    //dist = sdtriangleCross(p, _GlobalScale);
                    dist = sdMergerPyr(p,_GlobalScale, _iterations,_modOffsetPos ,_iterationTransform, _globalTransform, _smoothRadius, _scaleFactor, rotate45);
                } 
                // neg sphere
                else if(_renderNr == 4){
                    dist = sdNegSphere(p,_GlobalScale, _iterations,_modOffsetPos ,_iterationTransform, _globalTransform, _innerSphereRad, _scaleFactor);
                }
                //sphere
                else if(_renderNr == 5){
                    dist = sdSphere(p - _globalPosition ,_GlobalScale);
                }
                //box
                else if(_renderNr == 6){
                    dist = sdBox(p - _globalPosition, _GlobalScale * float3(1,1,1));
                }
                
                //default
                else dist = _maxDistance + 1;

                //check if section plane is checked
                if(_usePlane == 1){
                    float plane = sdPlane(p,_sectionTransform);
                    return max(dist, plane);
                }
                else return dist;
                
                
            }
            // returns the normal in a single point of the fractal
            float3 getNormal(float3 p)
            {

              float d = distanceField(p).x;
                const float2 e = float2(.01, 0);
              float3 n = d - float3(distanceField(p - e.xyy).x,distanceField(p - e.yxy).x,distanceField(p - e.yyx).x);
              return normalize(n);

            }
            // returns the normal of the forcefield
            float3 getNormalForceField(float3 p)
            {

              float d = sdforceField(p);
              const float2 e = float2(.01, 0);
              float3 n = d - float3(sdforceField(p - e.xyy),sdforceField(p - e.yxy),sdforceField(p - e.yyx));
              return normalize(n);

            }
            // calcutates soft shadows in a point
            float shadowCalc( in float3 ro, in float3 rd, float mint, float maxt, float k )
            {
                float res = 1.0;
                float ph = 1e20;
                for( float t=mint; t<maxt; )
                {
                    float h = min(distanceField(ro + rd*t),sdforceField(ro + rd*t));
                    if( h<0.001 )
                        return 0.0;
                    float y = h*h/(2.0*ph);
                    float d = sqrt(h*h-y*y);
                    res = min( res, k*d/max(0.0,t-y) );
                    ph = h;
                    t += h;
                }
                return res;
            }

            // the actual raymarcher
            fixed4 raymarching(float3 ro, float3 rd, float depth)
            {   

                fixed4 result = fixed4(0,0,0,0.5); // default
                int max_iteration = 400; // max amount of steps
                float t = 0; //distance traveled
                bool _forceFieldHit = false;
                float3 _forceFieldNormal;

                for (int i = 0; i < max_iteration; i++)
                {
                    //sends out ray from the camera
                    float3 p = ro + rd * t;
                    
                    //return distance to forcefield
                    float _forceField = sdforceField(p);
                    
                    
                    if (abs( _forceField) < _precision && _forceFieldHit == false) //hit forcefield
                    {
                        _forceFieldNormal = getNormalForceField(p);
                        _forceFieldHit = true;
                    }
                    
                    // check if to far
                    if(t > _maxDistance || t >= depth)
                    {

                        //environment
                        result = fixed4(rd,0);
                        break;

                    }

                    //return distance to fractal
                    float2 d = (distanceField(p));
                    

                    if ((d.x) < _precision) //hit
                    {
                        float3 colorDepth;
                        float light;
                        float shadow;
                        //shading
                        
                        float3 color = float3(_mainColor.rgb*(_iterations-d.y)/_iterations + _secColor.rgb*(d.y)/_iterations);
                        
                        if(_useNormal == 1){
                            float3 n = getNormal(p);
                             light = (dot(-_lightDir, n) * (1 - _lightIntensity) + _lightIntensity); //lambertian shading
                        }
                        else  light = 1;
                        
                        if(_useShadow == 1){
                             shadow = (shadowCalc(p, -_lightDir, 0.1, _maxDistance, 3) * (1 - _shadowIntensity) + _shadowIntensity); // soft shadows

                        }
                        else  shadow = 1;

                        float ao = (1 - 2 * i/float(max_iteration)) * (1 - _aoIntensity) + _aoIntensity; // ambient occlusion
                        float3 colorLight = float3 (color * light * shadow * ao); // multiplying all values between 0 and 1 to return final color
                        colorDepth = float3 (colorLight*(_maxDistance-t)/(_maxDistance) + _skyColor.rgb*(t)/(_maxDistance)); // multiplying with distance
                        
                        if(_forceFieldHit == true)
                        {
                            colorDepth =dot(-rd, _forceFieldNormal)* colorDepth + (1-dot(-rd, _forceFieldNormal))*_forceFieldColor; // multiply by transparant forcefield
                            
                        }
                        
                        
                        result = fixed4(colorDepth ,1);
                        break;

                    }
                    
                    // adds distance to the distance traveled and next point
                    if(_forceFieldHit == false)
                    {
                        
                        
                        // closer points get higher precicion to limit overstepping
                        if((d.x) < 10)
                        {
                            t+=  min(d.x * 0.75f, _forceField);
                        }
                        else if( abs(d.x) < 2)
                        {
                            t+= min(d.x * 0.5f, _forceField);
                        }
                        else t+= min(d.x, _forceField);
                        
                        
                    }
                    else t += d.x;
                    

                }

                return result;
            }
            // the fragment shader
            fixed4 frag (v2f i) : SV_Target
            {
               float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
               depth *= length(i.ray);
               fixed3 col = tex2D(_MainTex, i.uv);
               
               float3 rayDirection = normalize(i.ray.xyz);
               float3 rayOrigin = _WorldSpaceCameraPos;
               fixed4 result = raymarching(rayOrigin, rayDirection, depth);
               return fixed4(col * (1.0 - result.w) + result.xyz * result.w ,1.0);

            }
            ENDCG
        }
    }
}
