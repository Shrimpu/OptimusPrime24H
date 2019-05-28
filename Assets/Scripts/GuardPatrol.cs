using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(FieldOfView))]
public class GuardPatrol : MonoBehaviour
{
    public delegate void PlayerDetected(GameObject employeeOfTheMonth);
    public PlayerDetected playerDetected;
    public delegate void ActiveState(MoveState state, float speed);
    public ActiveState stateChanged;

    public enum MoveState { walking, idle, chasing }
    public MoveState currentMoveState = MoveState.idle;

    enum GuardStates { Patrolling, searching, chasing, inspectingAnomaly }
    GuardStates currentState = GuardStates.Patrolling;

    [Header("General stats")]
    public bool loop;
    public float moveSpeed = 1.5f;
    public float defaultTurnSpeed = 4f;
    [Header("Detection And Chase")]
    public float chaseSpeed = 2f;
    public float turnSpeenOnSearch = 1.2f;
    public float timeSuspicious = 0.5f;
    public float watchTimeTillDetected = 2f;
    public float speedOfDetectionLevelDecrease = 0.4f;
    public float viewRadiusOnChase = 12f;
    public float minDistanceFromTargetOnChase = 1.5f;
    public Slider detectionSlider;

    int checkPointIndex = 0;
    [Space]
    public Checkpoint[] checkpoints;

    NavMeshAgent agent;

    FieldOfView fow;

    Coroutine moveCoroutine;

    [System.Serializable]
    public class Checkpoint
    {
        public float timeAtCheckpoint = 1f;
        public Transform checkpointPosition;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        fow = GetComponent<FieldOfView>();
    }

    private void Start()
    {

        fow.canSeeTarget += (target) =>
        {
            if (currentState != GuardStates.chasing)
            {
                StopAllCoroutines();
                StartCoroutine(ChaseTarget(target));
            }
        };
        fow.targetInPeripheral += (target) =>
        {
            if (currentState != GuardStates.chasing)
            {
                if (currentState == GuardStates.searching || currentState == GuardStates.inspectingAnomaly)
                    StopAllCoroutines();
                StartCoroutine(LookForTarget(target));
            }
        };
        StartPatrolling();
    }

    public void StopPatrolling()
    {
        if (currentState == GuardStates.Patrolling)
            StopCoroutine(moveCoroutine);
    }

    public void StartPatrolling()
    {
        moveCoroutine = StartCoroutine(Move(checkpoints[0]));
    }

    void ChangeAndInvokeMoveState(MoveState newMoveState, float currentSpeed)
    {
        currentMoveState = newMoveState;
        stateChanged?.Invoke(currentMoveState, currentSpeed);
    }

    public void Alert(Vector3 sourcePosition)
    {
        Vector3 direction = sourcePosition - transform.position;
        if (!Physics.Raycast(transform.position, direction, out RaycastHit hit, direction.magnitude, fow.obstacleMask))
        {
            InspectPosition(sourcePosition - direction.normalized * minDistanceFromTargetOnChase);
        }
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
        currentState = GuardStates.Patrolling;
        Checkpoint currentPoint = startPoint;

        while (true)
        {
            ChangeAndInvokeMoveState(MoveState.idle, 0);
            int framesPast = 0;
            while (true)
            {
                yield return null;
                bool doneRotating = LookAtTarget(currentPoint.checkpointPosition.position, defaultTurnSpeed);
                if (doneRotating)
                    break;
                framesPast++;
                if (framesPast + 10 > defaultTurnSpeed / Time.deltaTime) // Safety exit
                    break;
            }

            while (true)
            {
                if (currentMoveState != MoveState.walking)
                {
                    ChangeAndInvokeMoveState(MoveState.walking, moveSpeed);
                    agent.SetDestination(currentPoint.checkpointPosition.position);
                    agent.speed = moveSpeed;
                }
                yield return new WaitForSeconds(0.1f);
                if (agent.velocity.magnitude < 0.02f)
                {
                    break;
                }
            }


            yield return new WaitForSeconds(currentPoint.timeAtCheckpoint);
            checkPointIndex++;
            currentPoint = GetNextPoint(checkPointIndex);
        }
    }

    IEnumerator LookForTarget(Transform target)
    {
        StopPatrolling();
        ChangeAndInvokeMoveState(MoveState.idle, 0);
        Vector3 lastKnownPosition = target.position;
        currentState = GuardStates.searching;
        agent.SetDestination(transform.position);
        int framesPast = 0;
        while (true)
        {
            bool lookingAtLastKnownPosition = LookAtTarget(lastKnownPosition, turnSpeenOnSearch);
            if (lookingAtLastKnownPosition)
                break;
            framesPast++;
            if (framesPast + 10 > 3 / Time.deltaTime) // Safety exit
                break;
            yield return null;
        }
        StartPatrolling();
    }

    IEnumerator ChaseTarget(Transform target)
    {
        currentState = GuardStates.chasing;
        StopPatrolling();
        float timeTargetHasBeenInvisible = 0;
        float timeTargetHasBeenVisible = 0;
        agent.speed = chaseSpeed;

        while (true)
        {
            yield return null;
            DisplayDetectionPercent(timeTargetHasBeenVisible);

            Vector3 directionToTarget = target.position - transform.position;
            float distanceToTarget = directionToTarget.magnitude;
            if (distanceToTarget < viewRadiusOnChase && !Physics.Raycast(transform.position, directionToTarget, distanceToTarget, fow.obstacleMask))
            {
                if (currentMoveState != MoveState.chasing)
                    ChangeAndInvokeMoveState(MoveState.chasing, chaseSpeed);
                timeTargetHasBeenInvisible = 0;
                LookAtTarget(target.position, defaultTurnSpeed);
                //transform.position = Vector3.MoveTowards(transform.position, target.position - directionToTarget.normalized * minDistanceFromTargetOnChase, chaseSpeed * Time.deltaTime);
                agent.SetDestination(target.position - directionToTarget.normalized * minDistanceFromTargetOnChase);
                timeTargetHasBeenVisible += Time.deltaTime;
                if (timeTargetHasBeenVisible >= watchTimeTillDetected)
                {
                    timeTargetHasBeenVisible = watchTimeTillDetected;
                    playerDetected?.Invoke(gameObject);
                    break;
                }
            }
            else if (agent.velocity.magnitude < 0.02f)
            {
                if (currentMoveState != MoveState.idle)
                    ChangeAndInvokeMoveState(MoveState.idle, 0);
                timeTargetHasBeenVisible -= Time.deltaTime * speedOfDetectionLevelDecrease;
                if (timeTargetHasBeenVisible <= 0)
                {
                    timeTargetHasBeenVisible = 0;
                    timeTargetHasBeenInvisible += Time.deltaTime;
                    if (timeTargetHasBeenInvisible >= timeSuspicious)
                    {
                        break;
                    }
                }
            }
        }
        StopAllCoroutines();
        moveCoroutine = StartCoroutine(Move(GetNextPoint(checkPointIndex - 2)));
    }

    IEnumerator InspectPosition(Vector3 position)
    {
        StopPatrolling();
        currentState = GuardStates.inspectingAnomaly;
        agent.SetDestination(transform.position);

        ChangeAndInvokeMoveState(MoveState.idle, 0);
        int framesPast = 0;
        while (true)
        {
            yield return null;
            bool doneRotating = LookAtTarget(position, defaultTurnSpeed);
            if (doneRotating)
                break;
            framesPast++;
            if (framesPast + 10 > defaultTurnSpeed / Time.deltaTime) // Safety exit
                break;
        }

        agent.speed = moveSpeed;
        while (true)
        {
            //transform.position = Vector3.MoveTowards(transform.position, position, moveSpeed * Time.deltaTime);
            if (currentMoveState != MoveState.walking)
            {
                ChangeAndInvokeMoveState(MoveState.walking, moveSpeed);
                agent.SetDestination(position - (position - transform.position).normalized * minDistanceFromTargetOnChase);
            }
            yield return new WaitForSeconds(0.1f);
            if (agent.velocity.magnitude < 0.02f)
            {
                break;
            }
        }

        yield return new WaitForSeconds(0.5f);
        StartPatrolling();
    }

    bool LookAtTarget(Vector3 target, float turnSpeed)
    {
        Vector3 targetDir = target - transform.position;
        targetDir.y = transform.position.y;

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