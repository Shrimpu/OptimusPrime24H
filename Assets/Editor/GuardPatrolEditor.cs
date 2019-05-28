using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GuardPatrol))]
public class GuardPatrolEditor : Editor
{
    private void OnSceneGUI()
    {
        GuardPatrol gp = (GuardPatrol)(target);

        Handles.color = Color.blue;
        Handles.DrawWireArc(gp.transform.position, Vector3.up, Vector3.forward, 360, gp.viewRadiusOnChase);
        Handles.DrawWireArc(gp.transform.position, Vector3.up, Vector3.forward, 360, gp.minDistanceFromTargetOnChase);
    }
}
