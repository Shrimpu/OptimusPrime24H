using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FieldOfView))]
public class GuardPatrol : MonoBehaviour
{
    public delegate void PlayerDetected(GameObject GuardOfTheMonth);
    public PlayerDetected playerDetected;

    [Header("General stats")]
    public bool loop;
    public float moveSpeed;
    public float turnSpeed;
    [Header("Detection And Chase")]
    public float timeSuspicious = 0.5f;
    public float watchTimeTillDetected = 2f;
    public float speedOfDetectionLevelDecrease = 0.4f;
    public float viewRadiusOnChase = 12f;
    public float minDistanceFromTargetOnChase = 1.5f;
    public Slider detectionSlider;

    int checkPointIndex = 0;
    bool chasing = false;
    [Space]
    public Checkpoint[] checkpoints;

    FieldOfView fow;

    Coroutine moveCoroutine;

    [System.Serializable]
    public class Checkpoint
    {
        public float timeAtCheckpoint = 1f;
        public Transform checkpointPosition;
    }

    private void Start()
    {
        fow = GetComponent<FieldOfView>();
        fow.canSeeTarget += (target) =>
        {
            if (!chasing)
                StartCoroutine(ChaseTarget(target));
        };
        StartPatrolling();
    }

    public void StopPatrolling()
    {
        StopCoroutine(moveCoroutine);
    }

    public void StartPatrolling()
    {
        moveCoroutine = StartCoroutine(Move(checkpoints[0]));
    }

    Checkpoint GetNextPoint(int currentPoint)
    {
        int nextPoint = 0;
        if (!loop)
        {
            nextPoint = (int)Mathf.PingPong(currentPoint, checkpoints.Length - 1);
        }
        else
        {
            nextPoint = currentPoint % checkpoints.Length;
        }
        return checkpoints[nextPoint];
    }

    IEnumerator Move(Checkpoint startPoint)
    {
        Checkpoint currentPoint = startPoint;

        while (true)
        {
            float framesPast = 0;
            while (true)
            {
                yield return null;
                bool doneRotating = LookAtTarget(currentPoint.checkpointPosition.position);
                if (doneRotating)
                    break;
                framesPast++;
                if (framesPast + 10 > turnSpeed / Time.deltaTime) // Safety exit
                    break;
            }

            while (transform.position != currentPoint.checkpointPosition.position)
            {
                transform.position = Vector3.MoveTowards(transform.position, currentPoint.checkpointPosition.position, moveSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(currentPoint.timeAtCheckpoint);
            currentPoint = GetNextPoint(checkPointIndex);
            checkPointIndex++;
        }
    }

    IEnumerator ChaseTarget(Transform target)
    {
        chasing = true;
        StopPatrolling();
        float timeTargetHasBeenInvisible = 0;
        float timeTargetHasBeenVisible = 0;
        while (true)
        {
            yield return null;
            DisplayDetectionPercent(timeTargetHasBeenVisible);

            Vector3 directionToTarget = target.position - transform.position;
            float distanceToTarget = directionToTarget.magnitude;
            if (distanceToTarget < viewRadiusOnChase && !Physics.Raycast(transform.position, directionToTarget, distanceToTarget, fow.obstacleMask))
            {
                timeTargetHasBeenInvisible = 0;
                LookAtTarget(target.position);
                transform.position = Vector3.MoveTowards(transform.position, target.position - directionToTarget.normalized * minDistanceFromTargetOnChase, moveSpeed * Time.deltaTime);
                timeTargetHasBeenVisible += Time.deltaTime;
                if (timeTargetHasBeenVisible >= watchTimeTillDetected)
                {
                    timeTargetHasBeenVisible = watchTimeTillDetected;
                    chasing = false;
                    playerDetected?.Invoke(gameObject);
                    break;
                }
            }
            else
            {
                timeTargetHasBeenVisible -= Time.deltaTime * speedOfDetectionLevelDecrease;
                if (timeTargetHasBeenVisible <= 0)
                {
                    timeTargetHasBeenVisible = 0;
                    timeTargetHasBeenInvisible += Time.deltaTime;
                    if (timeTargetHasBeenInvisible >= timeSuspicious)
                    {
                        chasing = false;
                        moveCoroutine = StartCoroutine(Move(GetNextPoint(checkPointIndex - 2)));
                        break;
                    }
                }
            }
        }
    }

    bool LookAtTarget(Vector3 target)
    {
        Vector3 targetDir = target - transform.position;

        float step = turnSpeed * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDir);
        if (newDir.normalized == targetDir.normalized || targetDir == Vector3.zero)
        {
            return true;
        }

        return false;
    }

    void DisplayDetectionPercent(float TimeDetected)
    {
        float detectionPercent = TimeDetected / watchTimeTillDetected;
        if (detectionPercent > 0.05f)
        {
            detectionSlider.value = detectionPercent;
            detectionSlider.enabled = true;
        }
        else
        {
            detectionSlider.value = detectionPercent;
            detectionSlider.enabled = false;
        }
    }
}