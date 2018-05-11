/// <summary>
/// SURGE FRAMEWORK
/// Author: Bob Berkebile
/// Email: bobb@pixelplacement.com
/// 
/// Simple system for turning anything into a button.
/// 
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif

namespace Pixelplacement
{
	[RequireComponent(typeof(Collider))]
	[ExecuteInEditMode]
	public class ColliderButton : MonoBehaviour
	{
		#region Public Events
		public UnityEvent OnSelected;
		public UnityEvent OnDeselected;
		public UnityEvent OnClick;
		public UnityEvent OnPressed;
		public UnityEvent OnReleased;
		#endregion

		#region Public Enums
		public enum EaseType { EaseOut, EaseOutBack };
		#endregion

		#region Private Enums
		enum ColliderType { Sphere, Capsule, Box };
		#endregion

		#region Public Variables
		public KeyCode[] keyInput;
		public bool _unityEventsFolded;
		public bool _scaleResponseFolded;
		public bool _colorResponseFolded;
		public bool applyColor = true;
		public bool applyScale;
		public LayerMask collisionLayerMask = -1;
		public Renderer colorRendererTarget;
		public Image colorImageTarget;
		public Color normalColor = Color.white;
		public Color selectedColor = Color.gray;
		public Color pressedColor = Color.green;
		public float colorDuration = .1f;
		public Transform scaleTarget;
		public Vector3 normalScale;
		public Vector3 selectedScale;
		public Vector3 pressedScale;
		public float scaleDuration = .1f;
		public EaseType scaleEaseType;
		public bool resizeGUIBoxCollider = true;
		public Vector2 guiBoxColliderPadding;
		#endregion

		#region Private Variables
		bool _clicking;
		ColliderType _colliderType;
		BoxCollider _boxCollider;
		CapsuleCollider _capsuleCollider;
		SphereCollider _sphereCollider;
		int _selectedCount;
		bool _colliderSelected;
		bool _pressed;
		bool _released;
		bool _vrRunning;
		RectTransform _rectTransform;
		EventTrigger _eventTrigger;
		EventTrigger.Entry _pressedEventTrigger;
		EventTrigger.Entry _releasedEventTrigger;
		EventTrigger.Entry _enterEventTrigger;
		EventTrigger.Entry _exitEventTrigger;
		#endregion

		#region Init
		private void Reset()
		{
			keyInput = new KeyCode[] { KeyCode.Mouse0 };

			//color setup:
			colorRendererTarget = GetComponent<Renderer>();
			colorImageTarget = GetComponent<Image>();

			//scale setup:
			scaleTarget = transform;
			normalScale = transform.localScale;
			selectedScale = transform.localScale * 1.15f;
			pressedScale = transform.localScale * 1.25f;

			//set initial size on gui collider:
			_rectTransform = GetComponent<RectTransform>();
			_boxCollider = GetComponent<BoxCollider>();
			if (_rectTransform != null && _boxCollider != null) ResizeGUIBoxCollider(_boxCollider);
		}

		private void Awake()
		{
			if (!Application.isPlaying) return;

			//rect and event triggers:
			_rectTransform = GetComponent<RectTransform>();
			if (_rectTransform != null) 
			{
				_eventTrigger = gameObject.AddComponent<EventTrigger>();
				_pressedEventTrigger = new EventTrigger.Entry();
				_pressedEventTrigger.eventID = EventTriggerType.PointerDown;
				_releasedEventTrigger = new EventTrigger.Entry();
				_releasedEventTrigger.eventID = EventTriggerType.PointerUp;
				_enterEventTrigger = new EventTrigger.Entry();
				_enterEventTrigger.eventID = EventTriggerType.PointerEnter;
				_exitEventTrigger = new EventTrigger.Entry();
				_exitEventTrigger.eventID = EventTriggerType.PointerExit;
			}

			//collider type:
			Collider collider = GetComponent<Collider>();
			if (collider.GetType() == typeof(BoxCollider))
			{
				_colliderType = ColliderType.Box;
				_boxCollider = GetComponent<BoxCollider>();
			}

			if (collider.GetType() == typeof(SphereCollider))
			{
				_colliderType = ColliderType.Sphere;
				_sphereCollider = GetComponent<SphereCollider>();
			}

			if (collider.GetType() == typeof(CapsuleCollider))
			{
				_colliderType = ColliderType.Capsule;
				_capsuleCollider = GetComponent<CapsuleCollider>();
			}
		}
		#endregion

		#region Flow
		private void OnEnable()
		{
			if (!Application.isPlaying) return;
		
			if (_rectTransform != null)
			{
				//event registrations:
				_pressedEventTrigger.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
				_eventTrigger.triggers.Add(_pressedEventTrigger);
				_releasedEventTrigger.callback.AddListener((data) => { OnPointerUpDelegate((PointerEventData)data); });
				_eventTrigger.triggers.Add(_releasedEventTrigger);
				_enterEventTrigger.callback.AddListener((data) => { OnPointerEnterDelegate((PointerEventData)data); });
				_eventTrigger.triggers.Add(_enterEventTrigger);
				_exitEventTrigger.callback.AddListener((data) => { OnPointerExitDelegate((PointerEventData)data); });
				_eventTrigger.triggers.Add(_exitEventTrigger);
			}
		}

		private void OnDisable()
		{
			if (!Application.isPlaying) return;

			if (_rectTransform != null)
			{
				//event deregistrations:
				_pressedEventTrigger.callback.RemoveAllListeners();
				_eventTrigger.triggers.Remove(_pressedEventTrigger);
				_releasedEventTrigger.callback.RemoveAllListeners();
				_eventTrigger.triggers.Remove(_releasedEventTrigger);
				_enterEventTrigger.callback.RemoveAllListeners();
				_eventTrigger.triggers.Remove(_enterEventTrigger);
				_exitEventTrigger.callback.RemoveAllListeners();
				_eventTrigger.triggers.Remove(_exitEventTrigger);
			}
		}
		#endregion

		#region Loops
		private void Update()
		{
			//update gui colliders:
			if (resizeGUIBoxCollider && _rectTransform != null && _boxCollider != null)
			{
				//fit a box collider:
				if (_boxCollider != null)
				{
					ResizeGUIBoxCollider(_boxCollider);
				}
			}

			//for in editor updating of the gui collider:
			if (!Application.isPlaying) return;

			//VR status:
#if UNITY_2017_2_OR_NEWER
			_vrRunning = (XRSettings.isDeviceActive);
#else
			_vrRunning = (VRSettings.isDeviceActive);
#endif

			//check collider collisions:
			int colliderCount = 0;
			switch (_colliderType)
			{
				case ColliderType.Sphere:
					colliderCount = Physics.OverlapSphere(transform.position + _sphereCollider.center, _sphereCollider.bounds.extents.z, collisionLayerMask).Length;
					break;

				case ColliderType.Box:
					colliderCount = Physics.OverlapBox(transform.position + _boxCollider.center, _boxCollider.bounds.extents, transform.rotation, collisionLayerMask).Length;
					break;

				case ColliderType.Capsule:
					float halfHeight = (_capsuleCollider.height * .5f) * transform.lossyScale.y;
					float radius = _capsuleCollider.radius * transform.lossyScale.z;
					Vector3 top = transform.TransformPoint(_capsuleCollider.center + Vector3.up * halfHeight);
					Vector3 bottom = transform.TransformPoint(_capsuleCollider.center + Vector3.down * halfHeight);
					colliderCount = Physics.OverlapCapsule(top, bottom, radius, collisionLayerMask).Length;
					break;
			}

			//collider collision started:
			if (!_colliderSelected && colliderCount > 1)
			{
				_colliderSelected = true;
				Selected();
			}

			//collider collision ended:
			if (_colliderSelected && colliderCount == 1)
			{
				_colliderSelected = false;
				Deselected();
			}

			//process input:
			if (keyInput != null && _selectedCount > 0)
			{
				foreach (var item in keyInput)
				{
					if (Input.GetKeyDown(item))
					{
						if (_selectedCount == 0) return;
						Pressed();
					}

					if (Input.GetKeyUp(item))
					{
						Released();
					}
				}
			}
		}
		#endregion

		#region Event Handlers
		private void OnPointerDownDelegate(PointerEventData data)
		{
			Pressed();
		}

		private void OnPointerUpDelegate(PointerEventData data)
		{
			Released();
		}

		private void OnPointerEnterDelegate(PointerEventData data)
		{
			Selected();
		}

		private void OnPointerExitDelegate(PointerEventData data)
		{
			Deselected();
		}

		private void OnMouseDown()
		{
			if (_vrRunning) return;
			Pressed();
		}

		private void OnMouseUp()
		{
			if (_vrRunning) return;
			Released();
			if (Application.isMobilePlatform)
			{
				Deselected();
			}
		}

		private void OnMouseEnter()
		{
			if (Application.isMobilePlatform) return;
			if (_vrRunning) return;
			Selected();
		}

		private void OnMouseExit()
		{
			if (_vrRunning) return;
			Deselected();
		}

		public virtual void Deselected()
		{
			_selectedCount--;
			if (_selectedCount > 0) return;
			if (!Application.isMobilePlatform)
			{
				if (OnDeselected != null) OnDeselected.Invoke();
			}
			_clicking = false;
			ColorNormal();
			ScaleNormal();
		}

		public virtual void Selected()
		{
			_selectedCount++;
			if (_selectedCount != 1) return;

			_pressed = false;
			_released = false;

			if (OnSelected != null) OnSelected.Invoke();
			_clicking = false;
			ColorSelected();
			ScaleSelected();
		}

		public virtual void Pressed()
		{
			if (_pressed) return;
			_pressed = true;
			_released = false;
			
			if (OnPressed != null) OnPressed.Invoke();
			_clicking = true;
			ColorPressed();
			ScalePressed();
		}

		public virtual void Released()
		{
			if (_released) return;
			_pressed = false;
			_released = true;

			if (_clicking)
			{
				if (OnClick != null) OnClick.Invoke();
			}
			_clicking = false;
			if (OnReleased != null) OnReleased.Invoke();
			if (_selectedCount != 0)
			{
				ColorSelected();
				ScaleSelected();
			}
		}
		#endregion

		#region Private Methods
		private void ResizeGUIBoxCollider(BoxCollider boxCollider)
		{
			boxCollider.size = new Vector3(_rectTransform.rect.width + guiBoxColliderPadding.x, _rectTransform.rect.height + guiBoxColliderPadding.y, _boxCollider.size.z);

			float centerX = (Mathf.Abs(_rectTransform.pivot.x - 1) - .5f) * boxCollider.size.x;
			float centerY = (Mathf.Abs(_rectTransform.pivot.y - 1) - .5f) * boxCollider.size.y;
			boxCollider.center = new Vector3(centerX, centerY, boxCollider.center.z);
		}

		private void ColorNormal()
		{
			if (!applyColor) return;
			if (colorRendererTarget != null) Tween.Color(colorRendererTarget, normalColor, colorDuration, 0);
			if (colorImageTarget != null) Tween.Color(colorImageTarget, normalColor, colorDuration, 0);
		}

		private void ColorSelected()
		{
			if (!applyColor) return;
			if (colorRendererTarget != null) Tween.Color(colorRendererTarget, selectedColor, colorDuration, 0);
			if (colorImageTarget != null) Tween.Color(colorImageTarget, selectedColor, colorDuration, 0);
		}

		private void ColorPressed()
		{
			if (!applyColor) return;
			if (colorRendererTarget != null) Tween.Color(colorRendererTarget, pressedColor, colorDuration, 0);
			if (colorImageTarget != null) Tween.Color(colorImageTarget, pressedColor, colorDuration, 0);
		}

		private void ScaleNormal()
		{
			if (!applyScale) return;
			AnimationCurve curve = null;
			switch (scaleEaseType)
			{
				case EaseType.EaseOut:
					curve = Tween.EaseOutStrong;
					break;

				case EaseType.EaseOutBack:
					curve = Tween.EaseOutBack;
					break;
			}
			Tween.LocalScale(scaleTarget, normalScale, scaleDuration, 0, curve);
		}

		private void ScaleSelected()
		{
			if (!applyScale) return;
			AnimationCurve curve = null;
			switch (scaleEaseType)
			{
				case EaseType.EaseOut:
					curve = Tween.EaseOutStrong;
					break;

				case EaseType.EaseOutBack:
					curve = Tween.EaseOutBack;
					break;
			}
			Tween.LocalScale(scaleTarget, selectedScale, scaleDuration, 0, curve);
		}

		private void ScalePressed()
		{
			if (!applyScale) return;
			AnimationCurve curve = null;
			switch (scaleEaseType)
			{
				case EaseType.EaseOut:
					curve = Tween.EaseOutStrong;
					break;

				case EaseType.EaseOutBack:
					curve = Tween.EaseOutBack;
					break;
			}
			Tween.LocalScale(scaleTarget, pressedScale, scaleDuration, 0, curve);
		}
		#endregion
	}
}