using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CueLine : MonoBehaviour
{
    [SerializeField] Transform[] _impactLinePositions;
    private Vector3 lastMousePosition;
    [SerializeField] Transform _cueAnchor,_cue;
    public Color c1 = Color.yellow;
    public Color c2 = Color.red;
    public int lengthOfLineRenderer = 4;
    [SerializeField] LineRenderer lineRenderer,_impactLine,_cueLine;
    [SerializeField] Camera _topViewCamera,_3dCamera;
    [SerializeField] Rigidbody[] _allBalls;
    float _forceValue;
    [SerializeField] Slider _forceSlider;
    [SerializeField] bool _isSliderPressed,_isDragging;
    private Ray _ray, _outRay;
    private RaycastHit hit;

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
        //for (int i = 0; i < _impactLinePositions.Length; i++) _impactLine.SetPosition(i, _impactLinePositions[i].position);

        if (Input.GetMouseButton(0) && !_isSliderPressed && AllBallsStopped())
        {
            _isDragging = true;
            // Get the horizontal movement of the mouse drag
            float mouseX = Input.mousePosition.x;
            float deltaMouseX = (mouseX - lastMousePosition.x) * 0.2f;
            lastMousePosition = Input.mousePosition;

            // Rotate the ball based on the horizontal movement
            transform.Rotate(Vector3.up,  deltaMouseX);
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

        _ray = new Ray(transform.position, transform.forward) ;

        if (Deflect(_ray, out _outRay, out hit))
        {
            lineRenderer.SetPosition(0, _ray.origin);
            lineRenderer.SetPosition(1, hit.point);

            if (hit.collider.CompareTag("Player"))
            {
                // Calculate direction vector and end point of line
                Vector3 direction = (hit.collider.transform.position - hit.point).normalized;
                Vector3 endPoint = hit.point + direction * 3f;

                // Update last two points of lineRenderer
                lineRenderer.SetPosition(2, hit.collider.transform.position);
                lineRenderer.SetPosition(3, endPoint);

                Vector3 directionOfCueLine = endPoint - hit.collider.transform.position;
                Vector3 normal = new Vector3(-directionOfCueLine.z, 0, directionOfCueLine.x).normalized;

                sbyte multiple;

                if (hit.point.x < hit.collider.gameObject.transform.position.x) multiple = 1;
                else multiple = -1;

                Vector3 endpoint = hit.point + normal * 1.5f * multiple;

                _cueLine.SetPosition(0, hit.point);
                _cueLine.SetPosition(1, endpoint);

                _cueLine.enabled = true;

                //Debug.Log(hit.point.x + "\t " + hit.collider.gameObject.transform.position.x);
                //Debug.Log(Vector3.Angle(_impactLinePositions[0].position - _impactLinePositions[1].position, endPoint - hit.collider.transform.position));
            }
            else
            {
                lineRenderer.SetPosition(2, hit.point);
                lineRenderer.SetPosition(3, hit.point);

                _cueLine.enabled = false;
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
            _cueLine.enabled = false;
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
        _forceValue = Mathf.Round((_forceSlider.value * 2000) * 100) / 100;
        StartCoroutine(SliderRecoil(_forceValue));

        lineRenderer.enabled = false;
        _cueLine.enabled = false;
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
                // Calculate the center of the hit object
                Vector3 center = hit.collider.bounds.center;

                Vector3 normal = hit.normal;
                Vector3 deflect = Vector3.Reflect(ray.direction, normal);

                // Set the deflected Ray to start from the center of the hit object
                deflected = new Ray(center, -deflect);
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