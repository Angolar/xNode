﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO; 
using UnityEditor.Callbacks;

[InitializeOnLoad]
public partial class NodeEditorWindow : EditorWindow { 
    public Dictionary<NodePort, Vector2> portConnectionPoints { get { return _portConnectionPoints; } }
    private Dictionary<NodePort, Vector2> _portConnectionPoints = new Dictionary<NodePort, Vector2>();
    private Dictionary<NodePort, Rect> portRects = new Dictionary<NodePort, Rect>();
    public NodeGraphAsset graphAsset;
    public NodeGraph graph { get { return _graph != null ? _graph : _graph = new NodeGraph(); } }
    public NodeGraph _graph; 
    public Vector2 panOffset { get { return _panOffset; } set { _panOffset = value; Repaint(); } }
    private Vector2 _panOffset; 
    public float zoom { get { return _zoom; } set { _zoom = Mathf.Clamp(value, 1f, 5f); Repaint(); } }
    private float _zoom = 1; 

    partial void OnEnable();

    [MenuItem("Window/UNEC")]
    static NodeEditorWindow Init() {
        NodeEditorWindow w = CreateInstance<NodeEditorWindow>();
        w.titleContent = new GUIContent("UNEC");
        w.wantsMouseMove = true;
        w.Show();
        return w;
    }

    public void Save() {
        if (graphAsset == null) SaveAs();
        else if (AssetDatabase.Contains(graphAsset)) {
            graphAsset.json = graph.Serialize();
            EditorUtility.SetDirty(graphAsset);
            AssetDatabase.SaveAssets();
        }
        else SaveAs();
    }

    public void SaveAs() {
        string path = EditorUtility.SaveFilePanelInProject("Save NodeGraph", "NewNodeGraph", "asset", "");
        if (string.IsNullOrEmpty(path)) return;
        else {
            NodeGraphAsset existingGraphAsset = AssetDatabase.LoadAssetAtPath<NodeGraphAsset>(path);
            if (existingGraphAsset != null) graphAsset = existingGraphAsset;
            else {
                graphAsset = new NodeGraphAsset();
                AssetDatabase.CreateAsset(graphAsset, path);
            }
            graphAsset.json = graph.Serialize();
            EditorUtility.SetDirty(graphAsset);
            AssetDatabase.SaveAssets();
        }
    }

    public void New() {
        _graph = new NodeGraph();
    }

    private void DraggableWindow(int windowID) {
        GUI.DragWindow();
    }

    public Vector2 WindowToGridPosition(Vector2 windowPosition) {
        return (windowPosition - (position.size * 0.5f) - (panOffset / zoom)) * zoom;
    }

    public Vector2 GridToWindowPosition(Vector2 gridPosition) {
        return (position.size * 0.5f) + (panOffset / zoom) + (gridPosition/zoom);
    }

    public Vector2 GridToWindowPositionNoClipped(Vector2 gridPosition) {
        Vector2 center = position.size * 0.5f;
        float xOffset = (center.x * zoom + (panOffset.x + gridPosition.x));
        float yOffset = (center.y * zoom + (panOffset.y + gridPosition.y));
        return new Vector2(xOffset, yOffset);
    }

    public void SelectNode(Node node) {
        selectedNode = node;
    }

    [OnOpenAsset(0)]
    public static bool OnOpen(int instanceID, int line) {
        NodeGraphAsset nodeGraphAsset = EditorUtility.InstanceIDToObject(instanceID) as NodeGraphAsset;
        if (nodeGraphAsset != null) {
            NodeEditorWindow w = Init();
            w.graphAsset = nodeGraphAsset;
            w._graph = nodeGraphAsset.nodeGraph;
            return true;
        }
        return false;
    }
}