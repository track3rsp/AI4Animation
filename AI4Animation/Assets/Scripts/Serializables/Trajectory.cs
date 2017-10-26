﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Trajectory {

	public bool Inspect = false;

	public float Width = 0.5f;

	public Point[] Points;

	private const int PastPoints = 6;
	private const int FuturePoints = 5;
	private const int Density = 10;

	public Trajectory() {
		Inspect = false;
		Width = 0.5f;
		Initialise(Vector3.zero, Vector3.forward);
	}

	public void Initialise(Vector3 position, Vector3 direction) {
		Points = new Point[GetPointCount()];
		for(int i=0; i<GetPointCount(); i++) {
			Points[i] = new Point(position, direction);
		}
	}

	public int GetPastPoints() {
		return PastPoints;
	}

	public int GetFuturePoints() {
		return FuturePoints;
	}

	public int GetDensity() {
		return Density;
	}

	public Point GetSample(int index) {
		return Points[index*Density];
	}

	public int GetSampleCount() {
		return PastPoints + FuturePoints + 1;
	}

	public int GetPointCount() {
		return Density*(PastPoints + FuturePoints) + 1;
	}

	public int GetRootSampleIndex() {
		return PastPoints;
	}

	public int GetRootPointIndex() {
		return Density*PastPoints;
	}

	public Point GetRoot() {
		return Points[GetRootPointIndex()];
	}

	public Point GetPrevious() {
		return Points[GetRootPointIndex()-1];
	}

	[System.Serializable]
	public class Point {
		private Vector3 Position;
		private Vector3 Direction;

		public float Stand, Walk, Jog, Crouch, Jump, Bump;

		public Point(Vector3 position, Vector3 direction) {
			SetPosition(position, false);
			SetDirection(direction);

			Stand = 1f;
			Walk = 0f;
			Jog = 0f;
			Crouch = 0f;
			Jump = 0f;
			Bump = 0f;
		}

		public void SetPosition(Vector3 position, bool recalculateHeight) {
			Position = position;
			if(recalculateHeight) {
				Position.y = Utility.GetHeight(Position.x, Position.z, LayerMask.GetMask("Ground"));
				Jump = Utility.GetRise(Position.x, Position.z, LayerMask.GetMask("Ground"));
			}
		}

		public Vector3 GetPosition() {
			return Position;
		}

		public void SetDirection(Vector3 direction) {
			Direction = direction;
		}

		public Vector3 GetDirection() {
			return Direction;
		}

		public float GetHeight() {
			return Position.y;
		}

		public Quaternion GetRotation() {
			return Quaternion.LookRotation(Direction, Vector3.up);
		}

		public Vector3 Project(float distance) {
			Vector3 ortho = Quaternion.Euler(0f, 90f, 0f) * Direction;
			Vector3 proj = Position + distance * ortho.normalized;
			proj.y = Utility.GetHeight(proj.x, proj.z, LayerMask.GetMask("Ground"));
			return proj;
		}
	}

	public void Draw() {
		int step = Density;

		//Connections
		for(int i=0; i<GetPointCount()-step; i+=step) {
			UnityGL.DrawLine(Points[i].GetPosition(), Points[i+step].GetPosition(), 0.01f, Utility.Black);
		}

		//Projections
		for(int i=0; i<GetPointCount(); i+=step) {
			Vector3 right = Points[i].Project(Width/2f);
			Vector3 left = Points[i].Project(-Width/2f);
			UnityGL.DrawCircle(right, 0.01f, Utility.Yellow);
			UnityGL.DrawCircle(left, 0.01f, Utility.Yellow);
		}

		//Directions
		Color transparentDirection = new Color(Utility.Orange.r, Utility.Orange.g, Utility.Orange.b, 0.75f);
		for(int i=0; i<GetPointCount(); i+=step) {
			UnityGL.DrawLine(Points[i].GetPosition(), Points[i].GetPosition() + 0.25f * Points[i].GetDirection(), 0.025f, 0f, transparentDirection);
		}

		//Rises
		Color transparentRise = new Color(Utility.Blue.r, Utility.Blue.g, Utility.Blue.b, 0.75f);
		for(int i=0; i<GetPointCount(); i+=step) {
			UnityGL.DrawLine(Points[i].GetPosition(), Points[i].GetPosition() + 1f * Points[i].Jump * Vector3.up, 0.025f, 0f, transparentRise);
		}

		//Positions
		for(int i=0; i<GetPointCount(); i+=step) {
			if(i % GetDensity() == 0) {
				UnityGL.DrawCircle(Points[i].GetPosition(), 0.025f, Utility.Black);
			} else {
				UnityGL.DrawCircle(Points[i].GetPosition(), 0.005f, Utility.Black);
			}
		}
	}

	#if UNITY_EDITOR
	public void Inspector() {
		Utility.SetGUIColor(Color.grey);
		using(new GUILayout.VerticalScope ("Box")) {
			Utility.ResetGUIColor();
			if(GUILayout.Button("Trajectory")) {
				Inspect = !Inspect;
			}

			if(Inspect) {
				using(new EditorGUILayout.VerticalScope ("Box")) {
					Width = EditorGUILayout.FloatField("Width", Width);
				}
			}
		}
	}
	#endif

}