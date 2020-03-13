// Sphere
// s: radius
float sdSphere(float3 p, float s)
{
	return length(p) - s;
}

// Box
// b: size of box in x/y/z
float sdBox(float3 p, float3 b)
{
	float3 d = abs(p) - b;
	return min(max(d.x, max(d.y, d.z)), 0.0) +
		length(max(d, 0.0));
}

// InfBox
// b: size of box in x/y/z
float sd2DBox( in float2 p, in float2 b )
{
    float2 d = abs(p)-b;
    return length(max(d,float2(0,0))) + min(max(d.x,d.y),0.0);
}

//infinite cylinder
float sdCylinder( float2 p, float c )
{
  return length(p)-c;
}

// plane
float sdPlane (float3 p, float4x4 _globalTransform)
{
    
    float plane = mul(_globalTransform, float4(p,1)).x;
    return plane;

}

//triangle prism
float sdTriPrism( float2 p, float2 h )
{
    p.y = p.y;
    p.y += h.x;
    const float k = sqrt(3.0);
    h.x *= 0.5*k;
    p.xy /= h.x;
    p.x = abs(p.x) - 1.0;
    p.y = p.y + 1.0/k;
    if( p.x+k*p.y>0.0 ) p.xy=float2(p.x-k*p.y,-k*p.x-p.y)/2.0;
    p.x -= clamp( p.x, -2.0, 0.0 );
    float d1 = length(p.xy)*sign(-p.y)*h.x;
    float d2 = -h.y;
    return length(max(float2(d1,d2),0.0)) + min(max(d1,d2), 0.);
}

// Cross
// s: size of cross
float sdCross( in float3 p, float b )
{
  float da = sd2DBox(p.xy,float2(b,b));
  float db = sd2DBox(p.yz,float2(b,b));
  float dc = sd2DBox(p.zx,float2(b,b));
  return min(da,min(db,dc));
}
// CylinderCross
// s: size of cross
float sdCylinderCross( in float3 p, float b )
{
  float da = sdCylinder(p.xy,b);
  float db = sdCylinder(p.yz,b);
  float dc = sdCylinder(p.zx,b);
  return min(da,min(db,dc));
}
//trianglecross
float sdtriangleCross( in float3 p, float2 b)
{
  float da = sdTriPrism(p.xy,float2(b.x,b.y* 0.2));
  float db = sdTriPrism(p.zy,float2(b.x,b.y* 0.2));
  
  return min(da,db);
}

// Tetrahedron
float sdTetrahedron(float3 p, float a, float4x4 rotate45)
{
    
  p = mul(rotate45, float4(p,1)).xyz;
  return (max( abs(p.x+p.y)-p.z,abs(p.x-p.y)+p.z)-a)/sqrt(3.);
  
    
}
//pyramid
float sdPyramid( float3 p, float h)
{
  float m2 = h*h + 0.25;
    
  p.xz = abs(p.xz);
  p.xz = (p.z>p.x) ? p.zx : p.xz;
  p.xz -= 0.5;

  float3 q = float3( p.z, h*p.y - 0.5*p.x, h*p.x + 0.5*p.y);
   
  float s = max(-q.x,0.0);
  float t = clamp( (q.y-0.5*p.z)/(m2+0.25), 0.0, 1.0 );
    
  float a = m2*(q.x+s)*(q.x+s) + q.y*q.y;
  float b = m2*(q.x+0.5*t)*(q.x+0.5*t) + (q.y-m2*t)*(q.y-m2*t);
    
  float d2 = min(q.y,-q.x*m2-q.y*0.5) > 0.0 ? 0.0 : min(a,b);
    
  return sqrt( (d2+q.z*q.z)/m2 ) * sign(max(q.z,-p.y));
}


// Octahedron
float sdOctahedron(float3 p, float a, float4x4 rotate45)
{
  p = mul(rotate45, float4(p,1)).xyz;
  return (abs(p.x)+abs(p.y)+abs(p.z)-a)/3;
}

// BOOLEAN OPERATORS //

// Union
float opU(float d1, float d2)
{
	return min(d1, d2);
}

// Subtraction
float opS(float d1, float d2)
{
	return max(-d1, d2);
}

// Intersection
float opI(float d1, float d2)
{
	return max(d1, d2);
}

// Mod Position Axis
float pMod1 (inout float p, float size)
{
	float halfsize = size * 0.5;
	float c = floor((p+halfsize)/size);
	p = fmod(p+halfsize,size)-halfsize;
	p = fmod(-p+halfsize,size)-halfsize;
	return c;
}
float pMod ( float p, float size)
{
    float halfsize = size * 0.5;
    float c = floor((p+halfsize)/size);
    p = fmod(p+halfsize,size)-halfsize;
    p = fmod(p-halfsize,size)+halfsize;
    return p;
}

//mirror x plane
float3 sdSymX(float3 p )
{
    p.x = abs(p.x);
    return p;
}

//mirror xz plane
float3 sdSymXZ(float3 p )
{
    p.xz = abs(p.xz);
    return p;
}
//mirror all 3
float3 sdSymXYZ(float3 p )
{
    p.xyz = abs(p.xyz);
    return p;
}

//transform object

float3 sdTransform (float3 p, float4x4 _globalTransform)
{
    return mul(_globalTransform, float4(p,1)).xyz;
}

//Fractals


//mergerSponge
float2 sdMerger( in float3 p, float b, int _iterations, float3 _modOffsetPos , float4x4 _iterationTransform, float4x4 _globalTransform, float _smoothRadius, float _scaleFactor)
{
   
   p = mul(_globalTransform, float4(p,1)).xyz;
   
   
   float2 d = float2(sdBox(p,float3(b- _smoothRadius,b- _smoothRadius,b- _smoothRadius)),0)- _smoothRadius;

   float s = 1.0;
   for( int m=0; m<_iterations; m++ )
   {
        p = mul(_iterationTransform, float4(p,1)).xyz;
        p.x = pMod(p.x,b*_modOffsetPos.x * 2/s);
        p.y = pMod(p.y,b*_modOffsetPos.y * 2/s);
        p.z = pMod(p.z,b*_modOffsetPos.z * 2/s);
      
        s *= _scaleFactor * 3;
        float3 r =(p)*s; 
        float c = (sdCross(r,b- _smoothRadius/s)- _smoothRadius)/s;
        
        if(-c>d.x)
        {
            d.x = -c;
            d = float2( d.x, m);
            
        }
   }  
   return d;
}
//mergerSponge cylinder
float2 sdMergerCyl( in float3 p, float b, int _iterations, float3 _modOffsetPos , float4x4 _iterationTransform, float4x4 _globalTransform, float _smoothRadius, float _scaleFactor)
{
   
   p = mul(_globalTransform, float4(p,1)).xyz;
   
   
   float2 d = float2(sdSphere(p,float3(b- _smoothRadius,b- _smoothRadius,b- _smoothRadius)),0)- _smoothRadius;

   float s = 1.0;
   for( int m=0; m<_iterations; m++ )
   {
        p = mul(_iterationTransform, float4(p,1)).xyz;
        p.x = pMod(p.x,b*_modOffsetPos.x * 2/s);
        p.y = pMod(p.y,b*_modOffsetPos.y * 2/s);
        p.z = pMod(p.z,b*_modOffsetPos.z * 2/s);
      
        s *= _scaleFactor * 3;
        float3 r =(p)*s; 
        float c = (sdCylinderCross(r,b- _smoothRadius/s)- _smoothRadius)/s;
        
        if(-c>d.x)
        {
            d.x = -c;
            d = float2( d.x, m);
            
        }
   }  
   return d;
}
//merger piramid
float2 sdMergerPyr( in float3 p, float b, int _iterations, float3 _modOffsetPos , float4x4 _iterationTransform, float4x4 _globalTransform, float _smoothRadius, float _scaleFactor, float4x4 rotate45)
{
   b = 2*b;
   p = mul(_globalTransform, float4(p,1)).xyz;
   
   
   float2 d = float2(sdPyramid(p / b,sqrt(3)/2) * b,0);

   float s = 1.0;
   for( int m=0; m<_iterations; m++ )
   {
        p = mul(_iterationTransform, float4(p,1)).xyz;
        //p = abs(p);
        p.x = pMod(p.x,b*_modOffsetPos.x * 0.5/s);
        p.y = pMod(p.y,b*_modOffsetPos.y * (sqrt(3)/2)/s);
        p.z = pMod(p.z,b*_modOffsetPos.z * 0.5/s);
      
        s *= _scaleFactor * 2;
        float3 r =(p)*s;
        float c  = (sdtriangleCross(float3(r.x, -r.y,r.z),b / sqrt(3)))/s;
        
        if(-c>d.x)
        {
            d.x = -c;
            d = float2( d.x, m);
            
        }
   }  
   return d;
}

// negative sphere

float2 sdNegSphere (in float3 p, float b, int _iterations, float3 _modOffsetPos , float4x4 _iterationTransform, float4x4 _globalTransform, float _sphere1, float _scaleFactor)
{
    p = mul(_globalTransform, float4(p,1)).xyz;
   
   
   float2 d = float2(sdBox(p,float3(b,b,b)),0);

   float s = 1.0;
   for( int m=0; m<_iterations; m++ )
   {
        p = mul(_iterationTransform, float4(p,1)).xyz;
        p.x = pMod(p.x,b*_modOffsetPos.x * 2/s);
        p.y = pMod(p.y,b*_modOffsetPos.y * 2/s);
        p.z = pMod(p.z,b*_modOffsetPos.z * 2/s);
        s *= _sphere1;
        float3 r =(p)*s; 
        float c = sdSphere(r,b)/s;
        s *= _scaleFactor * 3;

        if(-c>d.x)
        {
            d.x = -c;
            d = float2( d.x, m);
            
        }
   }  
   return d;

}
/*
// serpinski triangle
float2 sdSierpinski(float3 p, float b, float3 _modOffsetPos, int _iterations, float _scaleFactor, float4x4 rotate45)
{
    float2 d = sdTetrahedron(float3(p.x-b*0.705,p.y-b,p.z-b*0.705), b, rotate45);
    float s = 1.0;
    for( int m=0; m<_iterations; m++ )
    {
        float modX = pMod1(p.x,b*_modOffsetPos.x/s);
        float modY = pMod1(p.y,b*_modOffsetPos.y/s);
        float modZ = pMod1(p.z,b*_modOffsetPos.z/s);

        float3 r =(p)*s;
        //float c = (sdTetrahedron(float3(r.x,r.y,r.z),b, rotate45))/s;
        float c = min(sdTetrahedron(float3(r.x-b*0.705,r.y-b,r.z-b*0.705),b, rotate45),sdTetrahedron(float3(r.x+b*0.705,r.y+b,r.z+b*0.705),b, rotate45))/s;
        s *= _scaleFactor;
        if(-c>d.x)
        {
            d.x = -c;
            d = float2( d.x, m);
            
        }

    }
    return d;
}

//scale=2
//bailout=1000
float sdSierpinski(float3 p, float b, float3 _modOffsetPos, int _iterations, float _scaleFactor, float4x4 rotate45)
{
   r= length(p);
   for(i=0;i<_iterations && r<1000;i++){
      rotate1(x,y,z);
      
      if(x+y<0){x1=-y;y=-x;x=x1;}
      if(x+z<0){x1=-z;z=-x;x=x1;}
      if(y+z<0){y1=-z;z=-y;y=y1;}
      
      rotate2(x,y,z);

      x=b*x-CX*(scale-1);
      y=b*y-CY*(scale-1);
      z=b*z-CZ*(scale-1);
      r=length(p);
   }
   return (sqrt(r)-2)*b^(-i);//the estimated distance
}
*/
 


//mandelbulb
float mandelbulb (in float3 p,float _power,float b,float _iterations,float _smoothRadius){
    
    
    float3 w = p;
    float m = dot(w,w);
    float dr = 1.0;
  
    int iterations = 0;


    for (int i = 0; i < _iterations ; i++) {
        iterations = i;
       
        dr = pow(sqrt(m),_power-1) *_power*dr +1;
        //dz = 8.0*pow(m,3.5)*dz + 1.0;
        
        float r = length(w);
        float b = _power * acos( w.z/r);
        float a = _power * atan( w.y/ w.x );
        w = p + pow(r,_power) * float3( sin(b)*cos(a), sin(b)*sin(a), cos(b) );
        

        m = dot(w,w);
        if( m > 256.0 )
            break;
    }
    float dst = 0.25*log(m)* sqrt(m)/dr;
    return float2(dst*_smoothRadius, iterations);
   
}

//mandelbulb2
float mandelbulb2 (in float3 p,float _power,float b,float _iterations,float _smoothRadius){
    
    
    float3 w = p;
    float m = dot(w,w);
    float dr = 1.0;
  
    int iterations = 0;


    for (int i = 0; i < _iterations ; i++) {
        iterations = i;
       
        float m2 = m*m;
        float m4 = m2*m2;
        dr = _power*sqrt(m4*m2*m)*dr + 1.0;
        
        float x = w.x; float x2 = x*x; float x4 = x2*x2;
        float y = w.y; float y2 = y*y; float y4 = y2*y2;
        float z = w.z; float z2 = z*z; float z4 = z2*z2;

        float k3 = x2 + z2;
        float k2 = 1/sqrt( k3*k3*k3*k3*k3*k3*k3 );
        float k1 = x4 + y4 + z4 - 6.0*y2*z2 - 6.0*x2*y2 + 2.0*z2*x2;
        float k4 = x2 - y2 + z2;

        w.x =  64.0*x*y*z*(x2-z2)*k4*(x4-6.0*x2*z2+z4)*k1*k2;
        w.y = -16.0*y2*k3*k4*k4 + k1*k1;
        w.z = -8.0*y*k4*(x4*x4 - 28.0*x4*x2*z2 + 70.0*x4*z4 - 28.0*x2*z2*z4 + z4*z4)*k1*k2;
        

        m = dot(w,w);
        if( m > 256.0 )
            break;
    }
    float dst = 0.25*log(m)* sqrt(m)/dr;
    return float2(dst*_smoothRadius, iterations);
   
}



