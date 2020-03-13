using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]

public class RaymarchCam : SceneViewFilter
{
    [SerializeField]
    [Header("Global Settings")]
    private Shader _shader;

    public Material _raymarchMaterial
    {
        get
        {
            if (!_raymarchMat && _shader)
            {
                _raymarchMat = new Material(_shader);
                _raymarchMat.hideFlags = HideFlags.HideAndDontSave;
            }

            return _raymarchMat;
        }
    }

    private Material _raymarchMat;

    public Camera _camera
    {
        get
        {
            if (!_cam)
            {
                _cam = GetComponent < Camera >();
            }
            return _cam;
        }
    }
    private Camera _cam;
    private float _forceFieldRad;


    // all the variables send to the shader Bools are converted to ints because bools are not supported in shaders
    public Transform _directionalLight;
    public Transform _player;
    //public float _maxDistance;
    public float _precision;
    [Header("Visual Settings")]
    public bool _useNormal;
    public bool _useShadow;
    [Range(0, 1)]
    public float _lightIntensity;
    [Range(0, 1)]
    public float _shadowIntensity;
    [Range(0, 1)]
    public float _aoIntensity;
    public Color _mainColor;
    public Color _secColor;
    public Color _skyColor;
    public Color _forceFieldColor;

    [Header("Modulor Settings")]
    public bool _useModulor;
    public Vector3 _modInterval;

    [Header("Fractal Settings")]

    public bool _drawMergerSponge;
    public bool _drawMergerCylinder;
    public bool _drawMergerPyramid;
    public bool _drawNegativeSphere;
    public int _iterations;
    public float _power;
    public float _scaleFactor;
    public Vector3 _modOffsetPos;
    public Vector3 _iterationOffsetPos;
    public Vector3 _iterationOffsetRot;

    [Header("transform Settings")]
    public Vector3 _globalPosition;
    public Vector3 _GlobalRotation;
    public float _GlobalScale;
    public float _smoothRadius;
    public float _innerSphereRad;
    public bool _drawSphere;    
    public bool _drawBox;

    

    [Header("Plane Settings")]
    public bool _useSectionPlane;
    public bool _useSymmetry;
    public Vector3 _sectionPos;
    public Vector3 _sectionRot;
    [HideInInspector]
    public int _renderNr;
    private int _usePlane;
    private int _useMod;

    [HideInInspector]
    public Matrix4x4 _iterationTransform;
    [HideInInspector]
    public Matrix4x4 _sectionTransform;
    [HideInInspector]
    public Matrix4x4 _globalTransform;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        

        if (_drawMergerSponge == true || _drawMergerCylinder == true || _drawMergerPyramid == true || _drawNegativeSphere == true)
        {
            
            if (_drawMergerSponge) _renderNr = 1;
            if (_drawMergerCylinder) _renderNr = 2;
            if (_drawMergerPyramid) _renderNr = 3;
            if (_drawNegativeSphere) _renderNr = 4;
            

            // Construct a Model Matrix for the iteration transform
            _iterationTransform = Matrix4x4.TRS(
            _iterationOffsetPos,
            Quaternion.identity,
            Vector3.one);
            _iterationTransform *= Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.Euler(_iterationOffsetRot),
                Vector3.one);
            // Send the matrix to our shader
            _raymarchMaterial.SetMatrix("_iterationTransform", _iterationTransform.inverse);
            _raymarchMaterial.SetVector("_modInterval", _modInterval);
            _raymarchMaterial.SetVector("_modOffsetPos", _modOffsetPos);
            _raymarchMaterial.SetFloat("_scaleFactor", _scaleFactor);
            _raymarchMaterial.SetFloat("_innerSphereRad", _innerSphereRad);

        }
        
        else if (_drawSphere == true)
        {
            _renderNr = 5;
            
        }

        else if (_drawBox == true)
        {
            _renderNr = 6;
         
        }
        
        if (_useModulor)
        {
            _useMod = 1;
            _raymarchMaterial.SetVector("_modInterval", _modInterval);
        }
        else _useMod = 0;

        if (_useSectionPlane)
        {
            _usePlane = 1;
            // Construct a Model Matrix for the sectionplane
            _sectionTransform = Matrix4x4.TRS(
            _sectionPos,
            Quaternion.identity,
            Vector3.one);
            _sectionTransform *= Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.Euler(_sectionRot),
                Vector3.one);
            // Send the matrix to our shader
            _raymarchMaterial.SetMatrix("_sectionTransform", _sectionTransform.inverse);
        }
        else if (_useSymmetry)
        {
            _usePlane = 2;
            // Construct a Model Matrix for the sectionplane
            _sectionTransform = Matrix4x4.TRS(
            _sectionPos,
            Quaternion.identity,
            Vector3.one);
            _sectionTransform *= Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.Euler(_sectionRot),
                Vector3.one);
            // Send the matrix to our shader
            _raymarchMaterial.SetMatrix("_sectionTransform", _sectionTransform.inverse);
        }
        else _usePlane = 0;


        // Construct a Model Matrix for the global transform
        _globalTransform = Matrix4x4.TRS(
        _globalPosition,
        Quaternion.identity,
        Vector3.one);
        _globalTransform *= Matrix4x4.TRS(
            Vector3.zero,
            Quaternion.Euler(_GlobalRotation),
            Vector3.one);
        // Send the matrix to our shader
        _raymarchMaterial.SetMatrix("_globalTransform", _globalTransform.inverse);
        _raymarchMaterial.SetVector("_globalPosition", _globalPosition);


        if (_useNormal) _raymarchMaterial.SetInt("_useNormal", 1);
        else _raymarchMaterial.SetInt("_useNormal", 0);

        if (_useShadow) _raymarchMaterial.SetInt("_useShadow", 1);
        else _raymarchMaterial.SetInt("_useShadow", 0);

        if (!_raymarchMaterial)
        {
            Graphics.Blit(source, destination);
            return;
        }
        _forceFieldRad = _player.gameObject.GetComponent<SphereCollider>().radius;

        _raymarchMaterial.SetMatrix("_CamFrustrum", CamFrustrum(_camera));
        _raymarchMaterial.SetMatrix("_CamToWorld", _camera.cameraToWorldMatrix);
        _raymarchMaterial.SetFloat("_maxDistance", Camera.main.farClipPlane);
        _raymarchMaterial.SetFloat("_precision", _precision);
        _raymarchMaterial.SetFloat("_lightIntensity", _lightIntensity);
        _raymarchMaterial.SetFloat("_shadowIntensity", _shadowIntensity);
        _raymarchMaterial.SetFloat("_aoIntensity", _aoIntensity);
        _raymarchMaterial.SetInt("_iterations", _iterations);
        _raymarchMaterial.SetFloat("_power", _power);
        _raymarchMaterial.SetVector("_lightDir", _directionalLight ? _directionalLight.forward : Vector3.down);
        _raymarchMaterial.SetVector("_player", _player ? _player.position : Vector3.zero);
        _raymarchMaterial.SetColor("_mainColor", _mainColor);
        _raymarchMaterial.SetColor("_secColor", _secColor);
        _raymarchMaterial.SetColor("_skyColor", _skyColor);
        _raymarchMaterial.SetColor("_forceFieldColor", _forceFieldColor);
        _raymarchMaterial.SetFloat("_forceFieldRad", _forceFieldRad);
        _raymarchMaterial.SetFloat("_GlobalScale", _GlobalScale);
        _raymarchMaterial.SetFloat("_smoothRadius", _smoothRadius);


        _raymarchMaterial.SetInt("_renderNr", _renderNr);
        _raymarchMaterial.SetInt("_usePlane", _usePlane);
        _raymarchMaterial.SetInt("_useMod", _useMod);

        RenderTexture.active = destination;
        _raymarchMaterial.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();
        _raymarchMaterial.SetPass(0);
        GL.Begin(GL.QUADS);

        //BL
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);

        //BR
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);

        //TR
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);

        //TL
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();
    }

    private Matrix4x4 CamFrustrum(Camera cam)
    {
        Matrix4x4 frustrum = Matrix4x4.identity;
        float fov = Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad);

        Vector3 goUp = Vector3.up * fov;
        Vector3 goRight = Vector3.right * fov * cam.aspect;

        Vector3 TL = (-Vector3.forward - goRight + goUp);
        Vector3 TR = (-Vector3.forward + goRight + goUp);
        Vector3 BL = (-Vector3.forward - goRight - goUp);
        Vector3 BR = (-Vector3.forward + goRight - goUp);

        frustrum.SetRow(0, TL);
        frustrum.SetRow(1, TR);
        frustrum.SetRow(2, BR);
        frustrum.SetRow(3, BL);


        return frustrum;
    }

}
