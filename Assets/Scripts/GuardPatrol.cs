using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardPatrol : MonoBehaviour
{
    public bool loop;
    public float moveSpeed;
    public float turnSpeed;

    public Checkpoint[] checkpoints;

    [System.Serializable]
    public class Checkpoint
    {
        public float timeAtCheckpoint = 1f;
        public Transform checkpointPosition;
    }

    private void Start()
    {
        StartCoroutine(Move());
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

    IEnumerator Move()
    {
        int i = 0;
        Checkpoint currentPoint = checkpoints[0];

        while (true)
        {
            while (transform.position != currentPoint.checkpointPosition.position)
            {
                LookAtTarget(currentPoint.checkpointPosition.position);
                transform.position = Vector3.MoveTowards(transform.position, currentPoint.checkpointPosition.position, moveSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(currentPoint.timeAtCheckpoint);
            currentPoint = GetNextPoint(i);
            i++;
        }
    }

    void LookAtTarget(Vector3 target)
    {
        Vector3 targetRotation = target - transform.position;

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.forward, targetRotation), turnSpeed * Time.deltaTime);
    }
}
