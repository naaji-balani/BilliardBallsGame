using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CueLine : MonoBehaviour
{
    private Vector3 lastMousePosition;
    [SerializeField] Transform _cueAnchor, _cue;
    public Color c1 = Color.yellow;
    public Color c2 = Color.red;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Camera _topViewCamera, _3dCamera;
    [SerializeField] Rigidbody[] _allBalls;
    float _forceValue;
    [SerializeField] Slider _forceSlider;
    [SerializeField] bool _isSliderPressed, _isMoving, _hasPlayed;
    private Vector3 cueDirection;
    private GameObject targetBall;
    public GameObject[] targetBalls;



    void Start()
    {
        _3dCamera.enabled = false;

        lineRenderer.widthMultiplier = 0.1f;

        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
        lineRenderer.colorGradient = gradient;
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
        //_cueAnchor.rotation = default;

        //_cueAnchor.gameObject.SetActive(AllBallsStopped());
        //lineRenderer.enabled = AllBallsStopped();

        //if (Input.GetMouseButton(0) && !_isSliderPressed && AllBallsStopped())
        //{
        //    // Get the horizontal movement of the mouse drag
        //    float mouseX = Input.mousePosition.x;
        //    float deltaMouseX = mouseX - lastMousePosition.x;
        //    lastMousePosition = Input.mousePosition;

        //    // Rotate the ball based on the horizontal movement
        //    transform.Rotate(Vector3.up, deltaMouseX);
        //}
        //else
        //{
        //    // Store the last mouse position for the next drag event
        //    lastMousePosition = Input.mousePosition;
        //}

        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    _topViewCamera.enabled = !_topViewCamera.enabled;
        //    _3dCamera.enabled = !_3dCamera.enabled;
        //}

        //Ray a = new Ray(transform.position, transform.forward);
        //Ray b;
        //Ray c;
        //RaycastHit hit;

        //if (Deflect(a, out b, out c, out hit))
        //{
        //    lineRenderer.SetPosition(0, a.origin);
        //    lineRenderer.SetPosition(1, hit.point);

        //    if (hit.collider.CompareTag("Player")) //Other Ball
        //    {
        //        lineRenderer.SetPosition(2, (b.origin));
        //        lineRenderer.SetPosition(3, b.origin + 3 * b.direction);

        //        lineRenderer.SetPosition(4, hit.point);
        //        lineRenderer.SetPosition(5, hit.point + 3 * c.direction);
        //    }
        //    else //Wall
        //    {
        //        lineRenderer.SetPosition(2, hit.point);
        //        lineRenderer.SetPosition(3, hit.point + 3 * a.direction);
        //    }
        //}

        if (Input.GetMouseButton(0))
        {
            // Rotate cue ball
            float mouseX = Input.GetAxis("Mouse X");
            _cueAnchor.transform.Rotate(Vector3.up, mouseX * 5);

            // Show cue direction line
            cueDirection = _cueAnchor.transform.forward;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, cueDirection, out hit))
            {
                if (!_isSliderPressed)
                {
                    if (hit.collider.gameObject.CompareTag("Respawn"))
                    {
                        lineRenderer.enabled = true;
                        lineRenderer.SetPosition(0, transform.position);
                        lineRenderer.SetPosition(1, hit.point);

                        // Reset target ball
                        targetBall = null;
                    }
                    else
                    {
                        // Check for collision with target ball
                        foreach (GameObject ball in targetBalls)
                        {
                            if (hit.collider.gameObject == ball)
                            {
                                // Show target ball direction line
                                targetBall = ball;
                                Vector3 targetDirection = (hit.point - transform.position).normalized;
                                lineRenderer.enabled = true;
                                lineRenderer.SetPosition(0, targetBall.transform.position);
                                lineRenderer.SetPosition(1, targetBall.transform.position + targetDirection * 5f);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public void ApplyForceToCueBall()
    {
        Rigidbody cueBallRigidbody = GetComponent<Rigidbody>();
        cueBallRigidbody.AddForce(cueDirection * 3500, ForceMode.Impulse);
    }


    public void ValueChange()
    {
        _cue.localPosition = new Vector3(_cue.localPosition.x, _cue.localPosition.y, -28 - _forceSlider.value * 6);
    }

    public void SliderDown() => _isSliderPressed = true;

    public void SliderUp()
    {
        _forceValue = Mathf.Round((_forceSlider.value * 3500) * 100) / 100;
        StartCoroutine(SliderRecoil(_forceValue));

        lineRenderer.enabled = false;
        _isSliderPressed = false;
    }

    IEnumerator SliderRecoil(float forceValue)
    {
        while (_forceSlider.value != 0)
        {
            _forceSlider.value -= 5 * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        //GetComponent<Rigidbody>().AddForce(transform.forward * _forceValue);
        ApplyForceToCueBall();
        _cueAnchor.gameObject.SetActive(false);

    }

    bool Deflect(Ray ray, out Ray deflectedTarget, out Ray deflectedCue, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Vector3 normal = hit.normal;
                Vector3 deflectTarget = Vector3.Reflect(ray.direction, normal);
                Vector3 deflectCue = Vector3.Reflect(deflectTarget, normal);

                deflectedTarget = new Ray(hit.point, deflectTarget);
                deflectedCue = new Ray(hit.point, deflectCue);
                return true;
            }
            else
            {
                deflectedTarget = new Ray(hit.point, Vector3.zero);
                deflectedCue = new Ray(hit.point, Vector3.zero);
                return true;
            }
        }

        deflectedTarget = new Ray(Vector3.zero, Vector3.zero);
        deflectedCue = new Ray(Vector3.zero, Vector3.zero);
        return false;
    }
}