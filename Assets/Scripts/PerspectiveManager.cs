using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveManager : MonoBehaviour
{
    public Material previewMat;
    public GameObject targetForTakenObjects;
    public string getableTag = "Getable";
    public LayerMask layer;
    public float maxRayLength = 100.0f;
    public int getableLayer = 8;

    [SerializeField]
    private GameObject takenObject; // current object we want to affect

    private Camera mainCam;
    private float lastRotationY; // store last rotation 

    private Vector3 scaleMultiplier; // variable to store initial scale
    private float distanceMultiplier; // variable to store initial distance between takeObject and mainCamera
    private float lastRefineDistance = 0; // variable to store last refine vector length
    private Vector3 lastHitPoint = Vector3.zero; // variable to store last hit point
    private Material lastMaterial; // variable to restore takeObject's material

    private LayerMask allLayer = ~(1 << 8);
    
    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        Ray screenRay = mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Debug.DrawRay(screenRay.origin, screenRay.direction * maxRayLength, Color.yellow);

        bool isKeyPressed = Input.GetKey(KeyCode.E);
        

        RaycastHit hit;
        bool isHit = Physics.Raycast(screenRay, out hit, maxRayLength, allLayer);

        if (takenObject == null)
        {
            targetForTakenObjects.transform.position = hit.point;
        }

        if (isKeyPressed && isHit)
        {
            if (hit.transform.tag == getableTag)
            {
                takenObject = hit.collider.gameObject;
                takenObject.gameObject.layer = getableLayer;

                lastMaterial = takenObject.GetComponent<MeshRenderer>().sharedMaterial;
                takenObject.GetComponent<MeshRenderer>().sharedMaterial = previewMat;

                lastRotationY = takenObject.transform.rotation.eulerAngles.y - mainCam.transform.eulerAngles.y;
                distanceMultiplier = Vector3.Distance(mainCam.transform.position, takenObject.transform.position);
                scaleMultiplier = takenObject.transform.localScale;
                takenObject.transform.parent = targetForTakenObjects.transform;

                // Deal With RigidBody
                Rigidbody rigidbody = takenObject.GetComponent<Rigidbody>();
                if (rigidbody == null)
                {
                    rigidbody = takenObject.gameObject.AddComponent<Rigidbody>();
                }

                rigidbody.isKinematic = true;

                foreach (Collider col in takenObject.GetComponents<Collider>())
                {
                    col.isTrigger = true;
                }

                if (takenObject.GetComponent<MeshRenderer>() != null)
                {
                    takenObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    takenObject.GetComponent<MeshRenderer>().receiveShadows = false;
                }

                foreach (Transform child in takenObject.GetComponentsInChildren<Transform>())
                {
                    takenObject.GetComponent<Rigidbody>().isKinematic = true;
                    takenObject.GetComponent<Collider>().isTrigger = true;
                    child.gameObject.layer = getableLayer;
                }
            }
        }

        if (isKeyPressed && takenObject != null)
        {
            // recenter the object to the center of the mesh regardless real pivot point
            Vector3 centerCorrection = Vector3.zero;
            if (takenObject.GetComponent<MeshRenderer>() != null)
            {
                centerCorrection = takenObject.transform.position - takenObject.GetComponent<MeshRenderer>().bounds.center;
            }

            takenObject.transform.position = Vector3.Lerp(takenObject.transform.position, targetForTakenObjects.transform.position + centerCorrection, Time.deltaTime * 5);
            takenObject.transform.rotation = Quaternion.Lerp(takenObject.transform.rotation, Quaternion.Euler(new Vector3(0, lastRotationY + mainCam.transform.eulerAngles.y, 0)), Time.deltaTime * 5);

            Bounds grabObjectColliderBounds = takenObject.GetComponent<Collider>().bounds;
            float maxBound = Mathf.Max(grabObjectColliderBounds.size.x, grabObjectColliderBounds.size.y, grabObjectColliderBounds.size.z);

            // cameraHeight is the distance projected on hit.normal, that means the vertical distance between collision object and the camera
            float cosine = Vector3.Dot(hit.normal, screenRay.direction);
            float cameraHeight = hit.distance * cosine;

            // calculate a vector that squeeze the object out from the hitpoint consider its bound
            float refineDistance = (hit.distance * maxBound / 2.0f) / cameraHeight;

            if (refineDistance < maxRayLength)
            {
                lastRefineDistance = refineDistance;
            }

            if (isHit)
            {
                lastHitPoint = hit.point;
            }
            else
            {
                lastHitPoint = mainCam.transform.position + screenRay.direction * maxRayLength;
            }

            Vector3 refinement = screenRay.direction * lastRefineDistance;
            targetForTakenObjects.transform.position = Vector3.Lerp(targetForTakenObjects.transform.position, lastHitPoint + refinement, Time.deltaTime * 10);
            takenObject.transform.localScale = scaleMultiplier * Vector3.Distance(takenObject.transform.position, mainCam.transform.position) / distanceMultiplier;
        }
        
        if(Input.GetKeyUp(KeyCode.E) && takenObject != null)
        {
            // Realease Object

            Rigidbody rigidbody = takenObject.GetComponent<Rigidbody>();
            rigidbody.isKinematic = false;

            foreach (Collider col in takenObject.GetComponents<Collider>())
            {
                col.isTrigger = false;
            }

            if (takenObject.GetComponent<MeshRenderer>() != null)
            {
                takenObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                takenObject.GetComponent<MeshRenderer>().receiveShadows = true;
            }

            takenObject.GetComponent<MeshRenderer>().sharedMaterial = lastMaterial;
            takenObject.transform.parent = null;
            takenObject.layer = 0;

            foreach (Transform child in takenObject.GetComponentsInChildren<Transform>())
            {
                takenObject.GetComponent<Rigidbody>().isKinematic = false;
                takenObject.GetComponent<Collider>().isTrigger = false;
                child.gameObject.layer = 0;
            }

            takenObject = null;
        }
    }
}
