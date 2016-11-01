using System;
using System.Collections.Generic;
using SEPScience.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SEPScience.Unity.Unity
{
	public class SEP_Compact : CanvasFader, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField]
		private GameObject VesselPrefab = null;
		[SerializeField]
		private Transform VesselTransform = null;
		[SerializeField]
		private float fastFadeDuration = 0.2f;
		[SerializeField]
		private float slowFadeDuration = 0.5f;

		private bool dragging;
		private Vector2 mouseStart;
		private Vector3 windowStart;
		private RectTransform rect;
		
		private ISEP_Window windowInterface;
		private SEP_VesselSection currentVessel;

		private Guid id;

		public Guid ID
		{
			get { return id; }
		}

		protected override void Awake()
		{
			base.Awake();

			rect = GetComponent<RectTransform>();
		}

		private void Start()
		{
			Alpha(1);
		}

		public void setWindow(ISEP_Window window)
		{
			if (window == null)
				return;

			windowInterface = window;

			CreateVesselSection(windowInterface.CurrentVessel);

			transform.localScale *= window.Scale;
		}

		public void SetScale(float scale)
		{
			Vector3 one = Vector3.one;

			transform.localScale = one * scale;
		}

		public void SetPosition(Vector3 pos)
		{
			if (rect == null)
				return;

			rect.anchoredPosition = new Vector3(pos.x, pos.y, 1);
		}

		public void SetNewVessel(IVesselSection vessel)
		{
			if (vessel == null)
				return;

			if (currentVessel != null)
			{
				currentVessel.gameObject.SetActive(false);

				Destroy(currentVessel.gameObject);
			}

			id = vessel.ID;

			CreateVesselSection(vessel);
		}

		private void CreateVesselSection(IVesselSection section)
		{
			if (VesselPrefab == null || VesselTransform == null)
				return;

			GameObject sectionObject = Instantiate(VesselPrefab);

			if (sectionObject == null)
				return;

			sectionObject.transform.SetParent(VesselTransform, false);

			SEP_VesselSection vSection = sectionObject.GetComponent<SEP_VesselSection>();

			if (vSection == null)
				return;

			vSection.setVessel(section);

			currentVessel = vSection;
		}

		public void GoBack()
		{
			if (windowInterface == null)
				return;

			windowInterface.ChangeVessel(false);
		}

		public void GoNext()
		{
			if (windowInterface == null)
				return;

			windowInterface.ChangeVessel(true);
		}

		public void OnClose()
		{
			if (windowInterface == null)
				return;

			windowInterface.SetAppState(false);
		}

		public void Close()
		{
			Fade(0, fastFadeDuration, Kill, false);
		}

		private void Hide()
		{
			gameObject.SetActive(false);
		}

		public void Maximize()
		{
			if (windowInterface == null)
				return;

			windowInterface.IsMinimized = false;
		}

		private void Kill()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (rect == null)
				return;

			dragging = true;

			mouseStart = eventData.position;
			windowStart = rect.position;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (rect == null)
				return;

			rect.position = windowStart + (Vector3)(eventData.position - mouseStart);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			dragging = false;

			if (rect == null)
				return;

			if (windowInterface == null)
				return;

			windowInterface.WindowPos = new Vector3(rect.anchoredPosition.x, rect.anchoredPosition.y, windowInterface.WindowPos.z);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			FadeIn(false);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!dragging)
				FadeOut();
		}

		public void FadeIn(bool overrule)
		{
			Fade(1, fastFadeDuration, null, true, overrule);
		}

		public void FadeOut()
		{
			Fade(0.6f, slowFadeDuration);
		}

	}
}
