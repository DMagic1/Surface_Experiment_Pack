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
		private Button expandButton = null;
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

		private Vector3 windowResizeStart;
		private float heightStart;

		private ISEP_Window windowInterface;

		protected override void Awake()
		{
			base.Awake();

			rect = GetComponent<RectTransform>();
		}

		private void Start()
		{
			Alpha(1);
		}

		private void Update()
		{
			if (windowInterface == null)
				return;

			if (!windowInterface.IsVisible)
				return;

			windowInterface.UpdateWindow();
		}

		public void onBeginResize(BaseEventData eventData)
		{
			print("[SEP UI] Begin Drag...");

			if (rect == null)
				return;

			if (!(eventData is PointerEventData))
				return;


		}

		public void onResize(BaseEventData eventData)
		{
			if (rect == null)
				return;

			if (!(eventData is PointerEventData))
				return;

			rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y - ((PointerEventData)eventData).delta.y);

			checkMaxResize((int)rect.sizeDelta.y);
		}

		private void checkMaxResize(int num)
		{
			if (rect.sizeDelta.y < 200)
				num = 200;
			else if (rect.sizeDelta.y > 800)
				num = 800;

			rect.sizeDelta = new Vector2(rect.sizeDelta.x, num);
		}

		public void onEndResize(BaseEventData eventData)
		{
			if (!(eventData is PointerEventData))
				return;

			print("[SEP UI] End Drag...");
			checkMaxResize((int)rect.sizeDelta.y);
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
			if (rect == null)
				return;

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
