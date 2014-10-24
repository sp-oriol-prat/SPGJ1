//by Bob Berkebile : Pixelplacement : http://www.pixelplacement.com

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(iTweenPath))]
public class iTweenPathEditor : Editor
{
    iTweenPath _target;
    GUIStyle style = new GUIStyle();
    public static int count = 0;
    
    void OnEnable(){
        //i like bold handle labels since I'm getting old:
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        _target = (iTweenPath)target;
        
        //lock in a default path name:
        if(!_target.initialized){
            _target.initialized = true;
            _target.pathName = "New Path " + ++count;
            _target.initialName = _target.pathName;
        }
    }
    
    public override void OnInspectorGUI(){      
        //path name:
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Path Name");
        _target.pathName = EditorGUILayout.TextField(_target.pathName);
        EditorGUILayout.EndHorizontal();
        
        if(_target.pathName == ""){
            _target.pathName = _target.initialName;
        }
        
        //path color:
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Path Color");
        _target.pathColor = EditorGUILayout.ColorField(_target.pathColor);
        EditorGUILayout.EndHorizontal();
        
        //exploration segment count control:
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Node Count");
        _target.nodeCount =  Mathf.Clamp(EditorGUILayout.IntSlider(_target.nodeCount, 0, 10), 2,100);
        EditorGUILayout.EndHorizontal();


        //add node?
        if(_target.nodeCount > _target.nodes.Count){
            for (int i = 0; i < _target.nodeCount - _target.nodes.Count; i++) {             
                //add the node a little further                     
                Vector3 lastNodePos     = Vector3.zero;                 
                Vector3 previousNodePos = Vector3.zero;
                if (_target.nodes.Count  > 1 )
                    lastNodePos = _target.nodes[Mathf.Clamp((_target.nodes.Count -1), 0, (_target.nodes.Count -1))];                
                if (_target.nodes.Count > 2)
                    previousNodePos = _target.nodes[Mathf.Clamp((_target.nodes.Count -2), 0, (_target.nodes.Count -1))];                
                Vector3 direction = lastNodePos - previousNodePos;
                float distance = direction.magnitude;
                direction.Normalize();                
                _target.nodes.Add(new Vector3(lastNodePos.x + (direction.x * distance), lastNodePos.y, lastNodePos.z + (direction.z * distance)) );   
                //_target.nodes.Add(lastNodePos);
                
                
            }
        }
        /*
        //add node?
        if(_target.nodeCount > _target.nodes.Count){
            for (int i = 0; i < _target.nodeCount - _target.nodes.Count; i++) {
                _target.nodes.Add(Vector3.zero);    
            }
        }*/
    
        //remove node?
        if(_target.nodeCount < _target.nodes.Count){
            if(EditorUtility.DisplayDialog("Remove path node?","Shortening the node list will permantently destory parts of your path. This operation cannot be undone.", "OK", "Cancel")){
                int removeCount = _target.nodes.Count - _target.nodeCount;
                _target.nodes.RemoveRange(_target.nodes.Count-removeCount,removeCount);
            }else{
                _target.nodeCount = _target.nodes.Count;    
            }
        }
                
        //node display:
        EditorGUI.indentLevel = 4;
        for (int i = 0; i < _target.nodes.Count; i++) {
            _target.nodes[i] = EditorGUILayout.Vector3Field("Node " + (i+1), _target.nodes[i]);
        }
        
        //update and redraw:
        if(GUI.changed){
            EditorUtility.SetDirty(_target);            
        }
    }
    
    void OnSceneGUI(){
        if(_target.enabled) { // dkoontz
            if(_target.nodes.Count > 0){
                //allow path adjustment undo:
                Undo.RecordObject(_target,"Adjust iTween Path");
                
                int numnodes = 0;
                
                //node handle display:
                for (int i = 0; i < _target.nodes.Count; i++) 
                {
                    numnodes++;
                    
                    _target.nodes[i] = Handles.PositionHandle(_target.nodes[i], Quaternion.identity);
                    
                    //path begin and end labels:
                    if (i == 0)
                        Handles.Label(_target.nodes[i], "'" + _target.pathName + "' Begin", style);
                    else if (i == (_target.nodes.Count-1))
                        Handles.Label(_target.nodes[i], "'" + _target.pathName + "' End", style);

                    //print longitude
                    if (i < _target.nodes.Count - 1)
                    {
                        Vector3 Direction = (_target.nodes[i+1] - _target.nodes[i]);
                        Direction.Normalize();
                        
                        float Longitude = ((_target.nodes[i+1] - _target.nodes[i]).magnitude) ;
                        Handles.Label((_target.nodes[i] + (Direction * (Longitude / 2.0f))), Longitude.ToString(), style);
                    }
                }     
            }
        } // dkoontz
    }
}