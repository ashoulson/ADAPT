#region License
/*
* Agent Development and Prototyping Testbed
* https://github.com/ashoulson/ADAPT
* 
* Copyright (C) 2011-2015 Alexander Shoulson - ashoulson@gmail.com
*
* This file is part of ADAPT.
* 
* ADAPT is free software: you can redistribute it and/or modify
* it under the terms of the GNU Lesser General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* ADAPT is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Lesser General Public License for more details.
* 
* You should have received a copy of the GNU Lesser General Public License
* along with ADAPT.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(NavmeshBuilder))]
public class NavmeshBuilderEditor : Editor {
    SerializedObject so;
    SerializedProperty sp_center;
    SerializedProperty sp_size;
    SerializedProperty sp_walkableLayers;
    SerializedProperty sp_cellSize;
    SerializedProperty sp_cellHeight;
    SerializedProperty sp_walkableHeight;
    SerializedProperty sp_walkableSlopeAngle;
    SerializedProperty sp_walkableClimb;
    SerializedProperty sp_walkableRadius;
    SerializedProperty sp_monotonePartitioning;
    SerializedProperty sp_minRegionArea;
    SerializedProperty sp_mergeRegionArea;
    SerializedProperty sp_maxEdgeLen;
    SerializedProperty sp_maxSimplificationError;
    SerializedProperty sp_detailSampleDist;
    SerializedProperty sp_detailSampleMaxError;
    SerializedProperty sp_keepIntermediateData;

	[MenuItem("ADAPT/Navmesh Builder")]
	public static void SelectBuilder(MenuCommand mc)
	{
		Selection.activeObject = NavmeshBuilder.Instance;
	}
	
    void OnEnable()
    {
        so = new SerializedObject(target);
        sp_center = so.FindProperty("center");
        sp_size = so.FindProperty("size");
        sp_walkableLayers = so.FindProperty("walkableLayers");
        sp_cellSize = so.FindProperty("cellSize");
        sp_cellHeight = so.FindProperty("cellHeight");
        sp_walkableHeight = so.FindProperty("walkableHeight");
        sp_walkableSlopeAngle = so.FindProperty("walkableSlopeAngle");
        sp_walkableClimb = so.FindProperty("walkableClimb");
        sp_walkableRadius = so.FindProperty("walkableRadius");
        sp_monotonePartitioning = so.FindProperty("monotonePartitioning");
        sp_minRegionArea = so.FindProperty("minRegionArea");
        sp_mergeRegionArea = so.FindProperty("mergeRegionArea");
        sp_maxEdgeLen = so.FindProperty("maxEdgeLen");
        sp_maxSimplificationError = so.FindProperty("maxSimplificationError");
        sp_detailSampleDist = so.FindProperty("detailSampleDist");
        sp_detailSampleMaxError = so.FindProperty("detailSampleMaxError");
        sp_keepIntermediateData = so.FindProperty("keepIntermediateData");
    }

	public override void OnInspectorGUI()
    {
        so.Update();
		
        GUILayout.Label("Bounds");
        EditorGUILayout.PropertyField(sp_center);
        EditorGUILayout.PropertyField(sp_size);
        
        GUILayout.Label("Agent parameters");
        EditorGUILayout.PropertyField(sp_walkableHeight);
        EditorGUILayout.PropertyField(sp_walkableRadius);
        EditorGUILayout.PropertyField(sp_walkableClimb);
        EditorGUILayout.PropertyField(sp_walkableSlopeAngle);
        
        EditorGUILayout.Separator();

        GUILayout.Label("Rasterization");
        EditorGUILayout.PropertyField(sp_cellSize);
        EditorGUILayout.PropertyField(sp_cellHeight);
        
        EditorGUILayout.Separator();
        
        GUILayout.Label("Region generation");
        EditorGUILayout.PropertyField(sp_monotonePartitioning);
        EditorGUILayout.PropertyField(sp_minRegionArea);
        EditorGUILayout.PropertyField(sp_mergeRegionArea);

        EditorGUILayout.Separator();

        GUILayout.Label("Polygonization");
        EditorGUILayout.PropertyField(sp_maxEdgeLen);
        EditorGUILayout.PropertyField(sp_maxSimplificationError);
        
        EditorGUILayout.Separator();

        GUILayout.Label("Detail mesh");
        EditorGUILayout.PropertyField(sp_detailSampleDist);
        EditorGUILayout.PropertyField(sp_detailSampleMaxError);

        EditorGUILayout.Separator();
        
        GUILayout.Label("Navmesh generation");
        EditorGUILayout.PropertyField(sp_walkableLayers);
        EditorGUILayout.PropertyField(sp_keepIntermediateData);

        EditorGUILayout.Separator();

        if (GUILayout.Button("Generate"))
        {
            GameObject go = ((NavmeshBuilder)target).Generate();
			if (go != null)
				Selection.activeObject = go;
            EditorUtility.SetDirty(target);
        }
        
		GUILayout.Label("NOTE: This GameObject can safely be deleted after generating a Navmesh.");
		
        so.ApplyModifiedProperties();
    }
	
    private void OnSceneGUI()
    {
        so.Update();
        NavmeshBuilder nm = ((NavmeshBuilder)target);
        Undo.SetSnapshotTarget(nm, "Navmesh Bounds");
        Color savedHandleColor = Handles.color;
        Handles.color = new Color(145f, 244f, 139f, 255f) / 255f;
        Vector3 p = sp_center.vector3Value;
        Vector3 vector = sp_size.vector3Value * 0.5f;
        Vector3 a = sp_size.vector3Value * 0.5f;
        bool changed = GUI.changed;
        vector.x = this.SizeSlider(p, -Vector3.right, vector.x);
        vector.y = this.SizeSlider(p, -Vector3.up, vector.y);
        vector.z = this.SizeSlider(p, -Vector3.forward, vector.z);
        a.x = this.SizeSlider(p, Vector3.right, a.x);
        a.y = this.SizeSlider(p, Vector3.up, a.y);
        a.z = this.SizeSlider(p, Vector3.forward, a.z);
        if (GUI.changed)
        {
            sp_center.vector3Value = sp_center.vector3Value + (a - vector) * 0.5f;
            sp_size.vector3Value = a + vector;
            so.ApplyModifiedProperties();
        }
        GUI.changed |= changed;
        Handles.color = savedHandleColor;
		nm.transform.position = p;
    }
    
    private float SizeSlider(Vector3 p, Vector3 d, float r)
    {
        Vector3 vector = p + d * r;
        Color color = Handles.color;
        if (Vector3.Dot(vector - Camera.current.transform.position, d) >= 0f)
        {
            Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, Handles.color.a * 0.5f);
        }
        float handleSize = HandleUtility.GetHandleSize(vector);
        bool changed = GUI.changed;
        GUI.changed = false;
        vector = Handles.Slider(vector, d, handleSize * 0.1f, new Handles.DrawCapFunction(Handles.CylinderCap), 0f);
        if (GUI.changed)
        {
            r = Vector3.Dot(vector - p, d);
        }
        GUI.changed |= changed;
        Handles.color = color;
        return r;
    }
}
