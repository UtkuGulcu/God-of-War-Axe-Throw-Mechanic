using System.Collections;
using UnityEngine;
using Cinemachine;

public class Player : MonoBehaviour
{

    #region public variables

    [SerializeField] float movementSpeed;
    [SerializeField] float rotationSpeed;
    [SerializeField] float throwForce;
    [SerializeField] GameObject  CM_ThirdPerson;
    [SerializeField] GameObject CM_Aim;
    public bool axeIsOnHand;
    public bool isRecalling;

    #endregion

    #region public references

    [SerializeField] GameObject UI;
    [SerializeField] GameObject axe;
    [SerializeField] Transform rightHandTransform;
    [SerializeField] Transform curvePoint;

    #endregion

    #region private variables

    float horizontalInput;
    float verticalInput;
    float initalCameraRotationY;
    float recallTime;
    float distance;
    float recallDuration;
    Vector3 axeInitalPosition;
    Vector3 movementVector;
    Quaternion toRotation;
    WaitForSeconds waitThrow = new WaitForSeconds(1.2f);
    WaitForSeconds waitRecall = new WaitForSeconds(0.1f);
    bool isAiming;

    #endregion

    #region private references

    Rigidbody rbPlayer;
    Rigidbody rbAxe;
    Animator animator;

    #endregion

    private void Awake()
    {
        rbPlayer = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        rbAxe = axe.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        initalCameraRotationY = Camera.main.transform.eulerAngles.y;
        UI.SetActive(false);
        ToggleCamera(0);
        axeIsOnHand = true;
        recallTime = 0;
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        movementVector = new Vector3(horizontalInput, 0, verticalInput);
        movementVector = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * movementVector;

        if (movementVector != Vector3.zero )
        {
            //toRotation = Quaternion.LookRotation(movementVector.normalized);
            toRotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y - initalCameraRotationY - transform.rotation.y, 0);
        }

        animator.SetBool("isWalking", movementVector != Vector3.zero);

        if (Input.GetAxis("L2") == 1 || Input.GetMouseButton(1))
        {
            Aim();

            if (Input.GetAxis("R2") == 1 || Input.GetMouseButtonDown(0) && axeIsOnHand)
            {
                StartCoroutine(PlayThrowingAnimation());
            }
        }
        else
        {
            isAiming = false;
            ToggleCamera(0);
            animator.SetBool("isAiming", false);
            UI.SetActive(false);
        }

        if (Input.GetButtonDown("Recall") && !axeIsOnHand)
        {
            RecallAxe();
        }

        if (animator.GetBool("isRecalling"))
        {
            if (recallTime < recallDuration)
            {
                recallTime += Time.deltaTime;
                axe.transform.position = GetBQCPoint(recallTime / recallDuration, axeInitalPosition, curvePoint.position, rightHandTransform.position);
            }
            else
            {
                isRecalling = false;
                animator.SetBool("isRecalling", isRecalling);
                ResetAxe();
            }
        }
        
        animator.SetFloat("VelocityX", horizontalInput);
        animator.SetFloat("VelocityZ", verticalInput);

        if (!isAiming && movementVector != Vector3.zero)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        rbPlayer.velocity = movementVector * movementSpeed;
    }

    void Aim()
    {
        isAiming = true;
        ToggleCamera(1);
        animator.SetBool("isAiming", true);
        UI.SetActive(true);
        transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y - initalCameraRotationY - transform.rotation.y, 0);
    }

    void RecallAxe()
    {
        axeInitalPosition = axe.transform.position;
        recallTime = 0;
        rbAxe.velocity = Vector3.zero;
        rbAxe.isKinematic = true;
        isRecalling = true;
        distance = Vector3.Distance(axe.transform.position, transform.position);
        recallDuration = Mathf.Clamp((distance / 50), 0.5f, 2.5f);
        axe.GetComponent<Axe>().axeIsStuck = false;
        animator.SetBool("isRecalling", true);
    }

    void ResetAxe()
    {
        StartCoroutine(ShakeCamera(Mathf.Clamp(Vector3.Distance(transform.position, axeInitalPosition) / 10, 0.2f, 4), 0.1f));
        axeIsOnHand = true;
        axe.transform.SetParent(rightHandTransform);
        axe.transform.localPosition = new Vector3(-0.05439994f, 0.04740001f, 0.007399991f);
        axe.transform.localEulerAngles = new Vector3(184.092f, -171.131f, -473.83f);
    }


    void ToggleCamera(int selection)
    {
        CM_ThirdPerson.gameObject.SetActive(selection == 0);
        CM_Aim.gameObject.SetActive(selection == 1); 
    }

    IEnumerator PlayThrowingAnimation()
    {
        animator.SetBool("isThrowing", true);
        yield return waitThrow;
        animator.SetBool("isThrowing", false);
    }

    public void DetachAxe()
    {
        axe.transform.parent = null;
        axe.transform.eulerAngles = new Vector3(181.279f, -268.883f, -552.614f);
        axeIsOnHand = false;
        rbAxe.isKinematic = false;
        rbAxe.AddForce(Camera.main.transform.forward * throwForce * Time.deltaTime, ForceMode.Impulse);
    }

    IEnumerator ShakeCamera(float intensity, float time)
    {
        CM_ThirdPerson.GetComponent<CinemachineFreeLook>().GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = intensity;
        CM_ThirdPerson.GetComponent<CinemachineFreeLook>().GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = intensity;
        CM_ThirdPerson.GetComponent<CinemachineFreeLook>().GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = intensity;

        CM_Aim.GetComponent<CinemachineFreeLook>().GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = intensity;
        CM_Aim.GetComponent<CinemachineFreeLook>().GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = intensity;
        CM_Aim.GetComponent<CinemachineFreeLook>().GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = intensity;

        yield return new WaitForSeconds(time);

        CM_ThirdPerson.GetComponent<CinemachineFreeLook>().GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        CM_ThirdPerson.GetComponent<CinemachineFreeLook>().GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        CM_ThirdPerson.GetComponent<CinemachineFreeLook>().GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;

        CM_Aim.GetComponent<CinemachineFreeLook>().GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        CM_Aim.GetComponent<CinemachineFreeLook>().GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        CM_Aim.GetComponent<CinemachineFreeLook>().GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
    }

    Vector3 GetBQCPoint(float time, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - time;
        float tt = time * time;
        float uu = u * u;

        Vector3 p = (uu * p0) + (2 * u * time * p1) + (tt * p2);
        return p;
    }
}
