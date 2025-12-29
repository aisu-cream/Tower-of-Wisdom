using Unity.Cinemachine;
using UnityEngine;

public class OccludeVisionBlocks : MonoBehaviour {

    private CinemachineCamera cmCam;

    // Update is called once per frame
    void LateUpdate() {
        Camera cam = Camera.main;
        if (!cam) return;

        CinemachineBrain brain = cam.GetComponent<CinemachineBrain>();
        if (!brain) return;

        cmCam = brain.ActiveVirtualCamera as CinemachineCamera;
        if (!cmCam) return;

        Vector3 pos = Camera.main.transform.position;
        Vector3 target = cmCam.LookAt.GetComponent<Collider>().bounds.center;
        Vector3 dir = target - pos;
        float distance = dir.magnitude;

        RaycastHit[] hits = Physics.RaycastAll(pos, dir, distance);

        foreach (RaycastHit hit in hits) {
            FadeObstacle hos = hit.collider.GetComponent<FadeObstacle>();
            if (hos) hos.FadeOut();
        }
    }
}
