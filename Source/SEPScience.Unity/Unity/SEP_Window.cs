using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SEPScience.Unity.Interfaces;

namespace SEPScience.Unity.Unity
{
	[RequireComponent(typeof(RectTransform))]
	public class SEP_Window : CanvasFader, IBeginDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField]
		private Text title = null;
		[SerializeField]
		private Toggle minimizeToggle = null;
		[SerializeField]
		private Button closeButton = null;
		[SerializeField]
		private float fastFadeDuration = 0.2f;
		[SerializeField]
		private float slowFadeDuration = 0.5f;
		[SerializeField]
		private GameObject VesselSectionPrefab = null;
		[SerializeField]
		private Transform VesselSectionTransform = null;

		private Vector2 mouseStart;
		private Vector3 windowStart;
		private RectTransform rect;

		private ISEP_Window windowInterface;

		protected override void Awake()
		{
			base.Awake();

			rect = GetComponent<RectTransform>();
		}

		private void Start()
		{
			Alpha(0);
		}

		private void Update()
		{
			if (windowInterface == null)
				return;

			if (!windowInterface.IsVisible)
				return;

			windowInterface.UpdateWindow();
		}

		public void setWindow(ISEP_Window window)
		{
			if (window == null)
				return;

			windowInterface = window;

			CreateVesselSections(windowInterface.GetVessels());
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (rect == null)
				return;

			mouseStart = eventData.position;
			windowStart = rect.position;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (rect != null)
				rect.position = windowStart + (Vector3)(eventData.position - mouseStart);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			FadeIn();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			Fade(0.6f, slowFadeDuration);
		}

		public void OnClose()
		{
			close();
		}

		public void MinimizeToggle(bool max)
		{
			if (windowInterface == null)
				return;

			windowInterface.IsMinimized = !max;

			setSize(max);
		}

		public void FadeIn()
		{
			Fade(1, fastFadeDuration);
		}

		private void setSize(bool max)
		{
		
		}

		private void CreateVesselSections(IList<IVesselSection> sections)
		{
			if (sections == null)
				return;

			if (sections.Count <= 0)
				return;

			if (VesselSectionPrefab == null)
				return;

			if (VesselSectionTransform == null)
				return;

			for (int i = sections.Count - 1; i >= 0; i--)
			{
				IVesselSection section = sections[i];

				if (section == null)
					continue;

				CreateVesselSection(section);
			}
		}

		private void CreateVesselSection(IVesselSection section)
		{
			GameObject sectionObject = Instantiate(VesselSectionPrefab);

			if (sectionObject == null)
				return;

			windowInterface.ProcessStyle(sectionObject);

			sectionObject.transform.SetParent(VesselSectionTransform, false);

			SEP_VesselSection vSection = sectionObject.GetComponent<SEP_VesselSection>();

			if (vSection == null)
				return;

			vSection.setVessel(section);
		}

		public void addVesselSection(GameObject obj)
		{
			if (obj == null)
				return;
		}

		public void close()
		{
			Fade(0, slowFadeDuration, Destroy);

			gameObject.SetActive(false);
		}

		private void Destroy()
		{
			gameObject.SetActive(false);
			Destroy(gameObject);
		}
	}
}
