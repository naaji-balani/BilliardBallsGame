using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CueLine : MonoBehaviour
{
    private Vector3 lastMousePosition;
    [SerializeField] Transform _cueAnchor,_cue;
    public Color c1 = Color.yellow;
    public Color c2 = Color.red;
    public int lengthOfLineRenderer = 4;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Camera _topViewCamera,_3dCamera;
    [SerializeField] Rigidbody[] _allBalls;
    float _forceValue;
    [SerializeField] Slider _forceSlider;
    [SerializeField] bool _isSliderPressed,_isDragging;

    void Start()
    {
        //_3dCamera.enabled = false;

        //lineRenderer.widthMultiplier = 0.1f;
        //lineRenderer.positionCount = lengthOfLineRenderer;

        // A simple 2 color gradient with a fixed alpha of 1.0f.
        //float alpha = 1.0f;
        //Gradient gradient = new Gradient();
        //gradient.SetKeys(
        //    new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
        //    new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        //    );
        //lineRenderer.colorGradient = gradient;
    }

    bool AllBallsStopped()
    {
        for (int i = 0; i < _allBalls.Length; i++)
        {
            if (_allBalls[i].velocity != Vector3.zero) return false;
        }

        return true;
    }


    void Update()
    {
        if (Input.GetMouseButton(0) && !_isSliderPressed && AllBallsStopped())
        {
            _isDragging = true;
            // Get the horizontal movement of the mouse drag
            float mouseX = Input.mousePosition.x;
            float deltaMouseX = mouseX - lastMousePosition.x;
            lastMousePosition = Input.mousePosition;

            // Rotate the ball based on the horizontal movement
            transform.Rotate(Vector3.up, deltaMouseX * 0.1f);
        }

        if (Input.GetMouseButtonUp(0))
        {
            // Store the last mouse position for the next drag event
            lastMousePosition = Input.mousePosition;
            _isDragging = false;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            _topViewCamera.enabled = !_topViewCamera.enabled;
            _3dCamera.enabled = !_3dCamera.enabled;
        }

        Ray a = new Ray(transform.position, transform.forward);
        Ray b;
        RaycastHit hit;

        if (Deflect(a, out b, out hit))
        {
            lineRenderer.SetPosition(0, a.origin);
            lineRenderer.SetPosition(1, hit.point);

            if (hit.collider.CompareTag("Player"))
            {
                lineRenderer.SetPosition(2, (b.origin));
                lineRenderer.SetPosition(3, b.origin + 3 * b.direction);
            }
            else
            {
                lineRenderer.SetPosition(2, hit.point);
                lineRenderer.SetPosition(3, hit.point + 3 * a.direction);
            }
        }

        if (AllBallsStopped())
        {
            lineRenderer.enabled = true;
            _cueAnchor.gameObject.SetActive(true);

        }
        else
        {
            lineRenderer.enabled = false;
            _cueAnchor.gameObject.SetActive(false);
        }
    }

    public void ValueChange()
    {
        _cue.localPosition = new Vector3(_cue.localPosition.x, _cue.localPosition.y, -28 - _forceSlider.value * 6);
    }

    public void SliderDown() => _isSliderPressed = true;

    public void SliderUp()
    {
        _forceValue = Mathf.Round((_forceSlider.value * 5000) * 100) / 100;
        StartCoroutine(SliderRecoil(_forceValue));

        lineRenderer.enabled = false;
        _isSliderPressed = false;
    }

    IEnumerator SliderRecoil(float forceValue)
    {
        while(_forceSlider.value != 0)
        {
            _forceSlider.value -= 5 * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        _cueAnchor.gameObject.SetActive(false);

        GetComponent<Rigidbody>().AddForce(transform.forward * _forceValue);

    }

    bool Deflect(Ray ray, out Ray deflected, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Vector3 normal = hit.normal;
                Vector3 deflect = Vector3.Reflect(ray.direction, normal);

                deflected = new Ray(hit.point, -deflect);
                return true;
            }
            else
            {
                deflected = new Ray(hit.point, Vector3.zero);
                return true;
            }
        }

        deflected = new Ray(Vector3.zero, Vector3.zero);
        return false;
    }

}