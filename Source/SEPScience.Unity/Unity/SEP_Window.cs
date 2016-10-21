﻿#region license
/*The MIT License (MIT)
SEP_Window - Unity UI element for the main SEP window

Copyright (c) 2016 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System.Collections.Generic;
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
		private ScrollRect scrollRect = null;
		[SerializeField]
		private float fastFadeDuration = 0.2f;
		[SerializeField]
		private float slowFadeDuration = 0.5f;
		[SerializeField]
		private GameObject VesselSectionPrefab = null;
		[SerializeField]
		private Transform VesselSectionTransform = null;
		[SerializeField]
		private GameObject BodyObjectPrefab = null;
		[SerializeField]
		private Transform BodyObjectTransform = null;
		[SerializeField]
		private RectTransform VesselExpansion = null;

		private Vector2 mouseStart;
		private Vector3 windowStart;
		private RectTransform rect;

		private static SEP_Window window;
		private ISEP_Window windowInterface;

		private bool expanding;
		private int movingTo;

		private List<SEP_VesselSection> currentVessels = new List<SEP_VesselSection>();

		public ISEP_Window WindowInterface
		{
			get { return windowInterface; }
		}

		public static SEP_Window Window
		{
			get { return window; }
		}

		public ScrollRect ScrollRect
		{
			get { return scrollRect; }
		}

		protected override void Awake()
		{
			window = this;

			base.Awake();

			rect = GetComponent<RectTransform>();
		}

		private void Start()
		{
			Alpha(1);
		}

		private void Update()
		{
			if (!expanding)
				return;

			if (VesselExpansion == null)
				return;

			float currentX = VesselExpansion.anchoredPosition.x;

			if (currentX < movingTo)
			{
				VesselExpansion.anchoredPosition = new Vector2(VesselExpansion.anchoredPosition.x + 5, VesselExpansion.anchoredPosition.y);

				if (VesselExpansion.anchoredPosition.x >= movingTo)
				{
					VesselExpansion.anchoredPosition = new Vector2(100, VesselExpansion.anchoredPosition.y);
					expanding = false;
				}
			}
			else
			{
				VesselExpansion.anchoredPosition = new Vector2(VesselExpansion.anchoredPosition.x - 5, VesselExpansion.anchoredPosition.y);

				if (VesselExpansion.anchoredPosition.x <= movingTo)
				{
					VesselExpansion.anchoredPosition = new Vector2(-90, VesselExpansion.anchoredPosition.y);
					expanding = false;
				}
			}
			if (windowInterface == null)
				return;

			if (!windowInterface.IsVisible)
				return;

			windowInterface.UpdateWindow();			
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

			checkMaxResize((int)rect.sizeDelta.y);
		}

		public void setWindow(ISEP_Window window)
		{
			if (window == null)
				return;

			windowInterface = window;

			if (windowInterface.ShowAllVessels)
			{
				CreateVesselSections(windowInterface.GetVessels);

				if (VesselExpansion != null)
					VesselExpansion.gameObject.SetActive(false);
			}
			else
			{
				CreateVesselSections(windowInterface.GetBodyVessels(windowInterface.CurrentBody));

				CreateBodySections(windowInterface.GetBodies);
			}
		}

		private void CreateBodySections(IList<string> bodies)
		{
			if (windowInterface == null)
				return;

			if (bodies == null)
				return;

			for (int i = bodies.Count - 1; i >= 0; i--)
			{
				string b = bodies[i];

				if (string.IsNullOrEmpty(b))
					continue;

				IList<IVesselSection> vessels = windowInterface.GetBodyVessels(b);

				if (vessels.Count <= 0)
					continue;

				CreateBodySection(b, vessels.Count);
			}
		}

		private void CreateBodySection(string body, int count)
		{
			if (BodyObjectPrefab == null || BodyObjectTransform == null)
				return;

			GameObject sectionObject = Instantiate(BodyObjectPrefab);

			if (sectionObject == null)
				return;

			sectionObject.transform.SetParent(BodyObjectTransform, false);

			SEP_CelestialBodyObject bodyObject = sectionObject.GetComponent<SEP_CelestialBodyObject>();

			if (bodyObject == null)
				return;

			bodyObject.setBody(body, count);
		}

		public void AddBodySection(string body)
		{
			if (windowInterface == null)
				return;

			IList<IVesselSection> vessels = windowInterface.GetBodyVessels(body);

			if (vessels.Count <= 0)
				return;

			CreateBodySection(body, vessels.Count);
		}

		public void SetCurrentBody(string body)
		{
			if (windowInterface == null)
				return;

			for (int i = currentVessels.Count - 1; i >= 0; i--)
			{
				SEP_VesselSection vessel = currentVessels[i];

				if (vessel == null)
					continue;

				vessel.gameObject.SetActive(false);

				Destroy(vessel.gameObject);
			}

			CreateVesselSections(windowInterface.GetBodyVessels(body));
		}

		public void OnExpandToggle(bool isOn)
		{
			if (VesselExpansion == null)
				return;

			expanding = true;

			if (isOn)
				movingTo = 100;
			else
				movingTo = -90;
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
			FadeOut();
		}

		public void OnClose()
		{
			if (windowInterface == null)
				return;

			windowInterface.SetAppState(false);
		}

		public void MinimizeToggle()
		{
			if (windowInterface == null)
				return;

			windowInterface.IsMinimized = true;

			Fade(0, fastFadeDuration, Kill);
		}

		public void FadeIn()
		{
			Fade(1, fastFadeDuration);
		}

		public void FadeOut()
		{
			Fade(0.6f, slowFadeDuration);
		}

		private void CreateVesselSections(IList<IVesselSection> sections)
		{
			if (sections == null)
				return;

			if (sections.Count <= 0)
				return;

			currentVessels.Clear();

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
			if (VesselSectionPrefab == null || VesselSectionTransform == null)
				return;

			GameObject sectionObject = Instantiate(VesselSectionPrefab);

			if (sectionObject == null)
				return;

			sectionObject.transform.SetParent(VesselSectionTransform, false);

			SEP_VesselSection vSection = sectionObject.GetComponent<SEP_VesselSection>();

			if (vSection == null)
				return;

			vSection.setVessel(section);

			currentVessels.Add(vSection);
		}

		public void addVesselSection(GameObject obj)
		{
			if (obj == null)
				return;
		}

		public void close()
		{
			Fade(0, fastFadeDuration, Hide);
		}

		private void Hide()
		{
			gameObject.SetActive(false);
		}

		private void Kill()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}
	}
}
