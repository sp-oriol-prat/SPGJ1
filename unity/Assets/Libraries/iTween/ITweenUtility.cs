using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[AddComponentMenu("- Enigma -/Utilities/iTween Utility")]
[ExecuteInEditMode]
public class ITweenUtility : MonoBehaviour 
{
	//Check para dibujar el path
	public bool alwaysDrawPath;
	//Nueva funcionalidad: Actualizar posicion y rotacion en edit mode
	private Vector3 lastPosition;
	private Vector3 lastRotation;	
	private iTweenPath itweenPath;
	/// <summary>
	/// Inicialización
	/// </summary>
	public void Start()
	{
		transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		transform.rotation = new Quaternion(transform.rotation.x,transform.rotation.y,transform.rotation.z, transform.rotation.w);		
		
		lastPosition = transform.position;
		lastRotation = transform.eulerAngles;		
		alwaysDrawPath = false;

		itweenPath = transform.GetComponent<iTweenPath> ();
	}
	
		
	/// <summary>
	/// Actualiza la posición, orientación y escalado  de todos los nodos del path en funcion del central.
	/// </summary>
	public void Update()
	{
		if(Application.isEditor)
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
			transform.rotation = new Quaternion(transform.rotation.x,transform.rotation.y,transform.rotation.z, transform.rotation.w);		
			
			
			
			Vector3 currentPosition = transform.position;
			
			if (itweenPath != null)
			{				
				//Traslacion
				if (currentPosition != lastPosition)
				{
					Vector3 trasVect = currentPosition - lastPosition;
					
					for (int i=0;i<itweenPath.nodeCount;i++)
					{
						itweenPath.nodes[i] += trasVect;			
					}
					
					lastPosition = currentPosition;
				}
				
				Vector3 currentRotation = transform.eulerAngles;
				
				//Rotacion sobre el eje z
				if (currentRotation.z != lastRotation.z)
				{
					Vector3 diff = lastRotation - currentRotation;
					
					for (int i=0;i<itweenPath.nodeCount;i++)
					{
						GameObject aux = new GameObject();
						
						aux.transform.position = itweenPath.nodes[i];
						
						aux.transform.RotateAround(transform.position,Vector3.back,diff.z);
						itweenPath.nodes[i] = aux.transform.position;
					}
					
					lastRotation = currentRotation;
				}
						
			}
		}
		
	}
	
	/// <summary>
	/// Funcion de pintado del path
	/// </summary>
	void OnDrawGizmos  ()
	{
		if (alwaysDrawPath)
		{
			if(itweenPath != null)
			{
				if (itweenPath.nodes.Count > 0)
				{
					DrawPath(itweenPath.nodes.ToArray(), Color.cyan);
				}
			}
		}
	}
	
	/// <summary>
	/// Pintado del path
	/// </summary>
	/// <param name="path">
	/// A <see cref="Vector3[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param>
	public void DrawPath(Vector3[] path, Color color) {
		if(path.Length>0){
			DrawPathHelper(path, color,"gizmos");
		}
	}
	
	/// <summary>
	/// Función auxiliar de pintado del path
	/// </summary>
	/// <param name="path">
	/// A <see cref="Vector3[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param>
	/// <param name="method">
	/// A <see cref="System.String"/>
	/// </param>
	private void DrawPathHelper(Vector3[] path, Color color, string method)
	{
		Vector3[] vector3s = PathControlPointGenerator(path);
		
		//Line Draw:
		Vector3 prevPt = Interp(vector3s,0);
		Gizmos.color=color;
		int SmoothAmount = path.Length*20;
		for (int i = 1; i <= SmoothAmount; i++) {
			float pm = (float) i / SmoothAmount;
			Vector3 currPt = Interp(vector3s,pm);
			if(method == "gizmos"){
				Gizmos.DrawLine(currPt, prevPt);
			}else if(method == "handles"){
				Debug.LogError("iTween Error: Drawing a path with Handles is temporarily disabled because of compatability issues with Unity 2.6!");
				//UnityEditor.Handles.DrawLine(currPt, prevPt);
			}
			prevPt = currPt;
		}
	}	
	
	/// <summary>
	/// Función generadora de puntos de control
	/// </summary>
	/// <param name="path">
	/// A <see cref="Vector3[]"/>
	/// </param>
	/// <returns>
	/// A <see cref="Vector3[]"/>
	/// </returns>
	private Vector3[] PathControlPointGenerator(Vector3[] path){
		Vector3[] suppliedPath;
		Vector3[] vector3s;
		
		//create and store path points:
		suppliedPath = path;

		//populate calculate path;
		int offset = 2;
		vector3s = new Vector3[suppliedPath.Length+offset];
		Array.Copy(suppliedPath,0,vector3s,1,suppliedPath.Length);
		
		//populate start and end control points:
		//vector3s[0] = vector3s[1] - vector3s[2];
		vector3s[0] = vector3s[1] + (vector3s[1] - vector3s[2]);
		vector3s[vector3s.Length-1] = vector3s[vector3s.Length-2] + (vector3s[vector3s.Length-2] - vector3s[vector3s.Length-3]);
		
		//is this a closed, continuous loop? yes? well then so let's make a continuous Catmull-Rom spline!
		if(vector3s[1] == vector3s[vector3s.Length-2]){
			Vector3[] tmpLoopSpline = new Vector3[vector3s.Length];
			Array.Copy(vector3s,tmpLoopSpline,vector3s.Length);
			tmpLoopSpline[0]=tmpLoopSpline[tmpLoopSpline.Length-3];
			tmpLoopSpline[tmpLoopSpline.Length-1]=tmpLoopSpline[2];
			vector3s=new Vector3[tmpLoopSpline.Length];
			Array.Copy(tmpLoopSpline,vector3s,tmpLoopSpline.Length);
		}	
		
		return(vector3s);
	}
	
	/// <summary>
	/// Función de interpolación
	/// </summary>
	/// <param name="pts">
	/// A <see cref="Vector3[]"/>
	/// </param>
	/// <param name="t">
	/// A <see cref="System.Single"/>
	/// </param>
	/// <returns>
	/// A <see cref="Vector3"/>
	/// </returns>
	private Vector3 Interp(Vector3[] pts, float t){
		int numSections = pts.Length - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float) numSections), numSections - 1);
		float u = t * (float) numSections - (float) currPt;
				
		Vector3 a = pts[currPt];
		Vector3 b = pts[currPt + 1];
		Vector3 c = pts[currPt + 2];
		Vector3 d = pts[currPt + 3];
		
		return .5f * (
			(-a + 3f * b - 3f * c + d) * (u * u * u)
			+ (2f * a - 5f * b + 4f * c - d) * (u * u)
			+ (-a + c) * u
			+ 2f * b
		);
	}	
}


