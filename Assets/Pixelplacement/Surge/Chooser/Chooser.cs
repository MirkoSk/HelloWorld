﻿/// <summary>
/// SURGE FRAMEWORK
/// Author: Bob Berkebile
/// Email: bobb@pixelplacement.com
/// 
/// Simplify the act of selecting and interacting with things.
/// 
/// </summary>

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Pixelplacement
{
	public class Chooser : MonoBehaviour
	{
		#region Public Events
		public GameObjectEvent OnSelected;
		public GameObjectEvent OnDeselected;
		public GameObjectEvent OnPressed;
		public GameObjectEvent OnReleased;
		#endregion

		#region Public Enums
		public enum Method { Raycast, RaycastAll };
		#endregion

		#region Public Variables
		public bool _cursorPropertiesFolded;
		public bool _unityEventsFolded;
		public Transform source;
		public float raycastDistance = 3;
		public LayerMask layermask = -1;
		public Method method;
		public KeyCode[] pressedInput;
		public Transform cursor;
		public float surfaceOffset;
		public float idleDistance = 1.5f;
		public float stabilityMaxDelta = 0.0127f;
		public float snapToMaxDelta = 0.3048f;
		public float stableSpeed = 2;
		public float unstableSpeed = 10;
		public bool flipForward;
		public bool matchSurfaceNormal = true;
		public bool autoHide;
		#endregion

		#region Public Properties
		public Transform[] Current
		{
			get;
			private set;
		}
		#endregion

		#region Private Variables
		Transform _previousCursor;
		List<Transform> _current = new List<Transform>();
		List<Transform> _previous = new List<Transform>();
		Transform _currentRaycast;
		Transform _previousRaycast;
		Vector3 _targetPosition;
		bool _hidden;
		#endregion

		#region Init
		private void Reset()
		{
			source = transform;
			pressedInput = new KeyCode[] { KeyCode.Mouse0 };
		}
		#endregion

		#region Public Methods
		public void Pressed()
		{
			switch (method)
			{
				case Method.Raycast:
					if (_currentRaycast != null)
					{
						_currentRaycast.SendMessage("Pressed", SendMessageOptions.DontRequireReceiver);
						if (OnPressed != null) OnPressed.Invoke(_currentRaycast.gameObject);
					}
					break;

				case Method.RaycastAll:
					if (_current.Count > 0)
					{
						foreach (var item in _current)
						{
							item.SendMessage("Pressed", SendMessageOptions.DontRequireReceiver);
							if (OnPressed != null) OnPressed.Invoke(item.gameObject);
						}
					}
					break;
			}
		}

		public void Released()
		{
			switch (method)
			{
				case Method.Raycast:
					if (_currentRaycast != null)
					{
						_currentRaycast.SendMessage("Released", SendMessageOptions.DontRequireReceiver);
						if (OnReleased != null) OnReleased.Invoke(_currentRaycast.gameObject);
					}
					break;

				case Method.RaycastAll:
					if (_current.Count > 0)
					{
						foreach (var item in _current)
						{
							item.SendMessage("Released", SendMessageOptions.DontRequireReceiver);
							if (OnReleased != null) OnReleased.Invoke(item.gameObject);
						}
					}
					break;
			}
		}
		#endregion

		#region Loops
		private void Update()
		{
			//cursor setup:
			if (cursor != _previousCursor)
			{
				_previousCursor = cursor;
				if (cursor == null) return;

				foreach (var item in cursor.GetComponentsInChildren<Collider>())
				{
					Debug.Log("Cursor can not contain colliders. Disabling colliders on: " + item.name);
					item.enabled = false;
				}
			}

			//process input:
			if (pressedInput != null)
			{
				foreach (var item in pressedInput)
				{
					if (Input.GetKeyDown(item))
					{
						Pressed();
					}

					if (Input.GetKeyUp(item))
					{
						Released();
					}
				}
			}

			//clear out:
			_current.Clear();

			//raycast:
			RaycastHit hit;
			Physics.Raycast(source.position, source.forward, out hit, raycastDistance, layermask);
			_currentRaycast = hit.transform;

			//cursor visibility:
			if (cursor != null)
			{
				if (autoHide)
				{
					cursor.gameObject.SetActive(hit.transform != null);
				}
				else
				{
					cursor.gameObject.SetActive(true);
				}
			}

			//cursor management:
			if (cursor != null)
			{
				if (hit.transform != null)
				{
					//get position:
					_targetPosition = hit.point + hit.normal * surfaceOffset;

					//get position speed:
					float posSpeed = unstableSpeed;
					float delta = Vector3.Distance(_targetPosition, cursor.position);
					if (delta <= stabilityMaxDelta)
					{
						posSpeed = stableSpeed;
					}
					if (delta >= snapToMaxDelta)
					{
						cursor.position = _targetPosition;
					}
					else
					{
						cursor.position = Vector3.Lerp(cursor.position, _targetPosition, Time.deltaTime * posSpeed);
					}

					//set rotation:
					if (matchSurfaceNormal)
					{
						cursor.rotation = Quaternion.LookRotation(hit.normal);
					}
					else
					{
						cursor.LookAt(source);
					}

					//adjust:
					if (flipForward)
					{
						cursor.Rotate(Vector3.up * 180);
					}
				}
				else
				{
					//put out in front and face source (flip if needed):
					Vector3 inFront = source.position + source.forward * idleDistance;
					float delta = Vector3.Distance(inFront, cursor.position);
					if (delta >= snapToMaxDelta)
					{
						cursor.position = inFront;
					}
					else
					{
						cursor.position = Vector3.Lerp(cursor.position, inFront, Time.deltaTime * unstableSpeed);
					}

					cursor.LookAt(source.position);
					if (flipForward)
					{
						cursor.Rotate(Vector3.up * 180);
					}
				}
			}

			//handle raycast messages:
			if (method == Method.Raycast)
			{
				//select:
				if (_previousRaycast == null && hit.transform != null)
				{
					hit.transform.SendMessage("Selected", SendMessageOptions.DontRequireReceiver);
					if (OnSelected != null) OnSelected.Invoke(hit.transform.gameObject);
				}

				//updated select:
				if (hit.transform != null && _previousRaycast != null && _previousRaycast != hit.transform)
				{
					_previousRaycast.SendMessage("Deselected", SendMessageOptions.DontRequireReceiver);
					if (OnDeselected != null) OnDeselected.Invoke(_previousRaycast.gameObject);
					hit.transform.SendMessage("Selected", SendMessageOptions.DontRequireReceiver);
					if (OnSelected != null) OnSelected.Invoke(hit.transform.gameObject);
				}

				//deselect:
				if (_previousRaycast != null && hit.transform == null)
				{
					_previousRaycast.SendMessage("Deselected", SendMessageOptions.DontRequireReceiver);
					if (OnDeselected != null) OnDeselected.Invoke(_previousRaycast.gameObject);
				}

				//cache:
				_previousRaycast = hit.transform;
			}

			//raycast all:
			if (method == Method.RaycastAll)
			{
				//catalog:
				foreach (var item in Physics.RaycastAll(source.position, source.forward, raycastDistance, layermask))
				{
					_current.Add(item.transform);
				}

				//handle selects:
				if (_current.Count > 0)
				{
					foreach (var item in _current)
					{
						if (_previous.Count == 0 || !_previous.Contains(item))
						{
							item.SendMessage("Selected", SendMessageOptions.DontRequireReceiver);
							if (OnSelected != null) OnSelected.Invoke(item.gameObject);
						}
					}
				}

				//handle deselects:
				if (_previous.Count > 0)
				{
					foreach (var item in _previous)
					{
						if (_current.Count == 0 || !_current.Contains(item))
						{
							item.SendMessage("Deselected", SendMessageOptions.DontRequireReceiver);
							if (OnDeselected != null) OnDeselected.Invoke(item.gameObject);
						}
					}
				}

				//cache:
				_previous.Clear();
				_previous.AddRange(_current);
			}
		}
		#endregion
	}
}