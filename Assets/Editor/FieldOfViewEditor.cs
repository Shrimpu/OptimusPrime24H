using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fow = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.viewRadius);
        Vector3 viewAngleA = fow.DirectionFromAngle(-fow.viewAngle / 2, false);
        Vector3 viewAngleB = fow.DirectionFromAngle(fow.viewAngle / 2, false);

        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * fow.viewRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * fow.viewRadius);


        Handles.color = new Color(255, 69, 0);
        Vector3 viewAngleC = fow.DirectionFromAngle(-fow.peripheralVision / 2, false);
        Vector3 viewAngleD = fow.DirectionFromAngle(fow.peripheralVision / 2, false);

        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleC * fow.viewRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleD * fow.viewRadius);
    }
}
