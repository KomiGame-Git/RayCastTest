using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class RayCastTest_Enemy : MonoBehaviour
{
    [SerializeField]
    private GameObject target;
    [SerializeField]
    private float LookingArea = 5f;
    [SerializeField]
    private float FOV = 60f;
    private float distance;
    private float Y_Angle;

    [SerializeField]
    private List<Transform> points;
    private int destPoint = 0;
    private NavMeshAgent agent;
    private EnemyMode enemyMode;

    private TextMeshProUGUI DebugText;
    private TextMeshProUGUI LookingText;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        this.enemyMode = EnemyMode.Patrol;
        agent.autoBraking = false;
        GotoNextPoint();

        DebugText = GameObject.FindGameObjectWithTag("DebugText").GetComponent<TextMeshProUGUI>();
        LookingText = GameObject.FindGameObjectWithTag("LookingText").GetComponent<TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f && this.enemyMode == EnemyMode.Patrol)
        {
            GotoNextPoint();
        }
        if (this.enemyMode == EnemyMode.Chase)
        {
            agent.destination = target.transform.position;
        }

        Vector3 toTargetVector = target.transform.position - transform.position;
        //距離
        distance = Vector3.Distance(this.transform.position, target.transform.position);
        //Forward(正面方向)とTargetへのRayの間の角度
        Y_Angle = Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward,Vector3.up).normalized, Vector3.ProjectOnPlane(toTargetVector,Vector3.up).normalized, Vector3.up);

        DebugText.text = $"Playerとの距離{distance.ToString("f3")}\nTargetRayとFoward間の角度{Y_Angle}";

        Vector3 inFOVtoTargetVector = Quaternion.Euler(0f, Mathf.Clamp(Y_Angle, -this.FOV, this.FOV), 0f) * transform.forward;

        Ray forwardRay = new(transform.position, transform.forward);
        Ray targetRay = new(transform.position, toTargetVector.normalized);
        Ray inFOVtoTargetRay = new(transform.position, inFOVtoTargetVector);

        Debug.DrawRay(forwardRay.origin, forwardRay.direction * LookingArea, Color.green);
        Debug.DrawRay(targetRay.origin, targetRay.direction * LookingArea, Color.red);
        Debug.DrawRay(inFOVtoTargetRay.origin, inFOVtoTargetRay.direction * LookingArea, Color.yellow);

        if (Physics.Raycast(inFOVtoTargetRay, out RaycastHit hit, LookingArea))
        {
            if (hit.transform.gameObject.tag == target.tag)
            {
                LookingText.text = "見つかった！";
                if (this.enemyMode == EnemyMode.Patrol)
                {
                    this.enemyMode = EnemyMode.Chase;
                    this.destPoint = this.destPoint == 0 ? 2 : this.destPoint - 1;
                }
            }
            else
            {
                LookingText.text = "";
                if (this.enemyMode == EnemyMode.Chase)
                {
                    this.enemyMode = EnemyMode.Patrol;
                    GotoNextPoint();
                }
            }

        }
        else
        {
            LookingText.text = "";
            if (this.enemyMode == EnemyMode.Chase)
            {
                this.enemyMode = EnemyMode.Patrol;
                GotoNextPoint();
            }
        }

    }

    void GotoNextPoint()
    {
        if (points.Count == 0)
        {
            return;
        }
        else
        {
            agent.destination = points[destPoint].position;
            destPoint = (destPoint + 1) % points.Count;
        }
    }

}

enum EnemyMode
{
    Patrol,
    Chase,
}
