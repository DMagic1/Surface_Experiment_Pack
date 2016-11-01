#region license
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
	public class SEP_Window : CanvasFader, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
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
		[SerializeField]
		private TextHandler VesselPanelHandle = null;

		private Vector2 mouseStart;
		private Vector3 windowStart;
		private RectTransform rect;

		private static SEP_Window window;
		private ISEP_Window windowInterface;

		private bool dragging;
		private bool resizing;
		private bool expanding;
		private int movingTo;

		private string currentBody;

		private List<SEP_CelestialBodyObject> currentBodies = new List<SEP_CelestialBodyObject>();
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

		public string CurrentBody
		{
			get { return currentBody; }
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
					VesselExpansion.anchoredPosition = new Vector2(movingTo, VesselExpansion.anchoredPosition.y);
					expanding = false;
				}
			}
			else
			{
				VesselExpansion.anchoredPosition = new Vector2(VesselExpansion.anchoredPosition.x - 5, VesselExpansion.anchoredPosition.y);

				if (VesselExpansion.anchoredPosition.x <= movingTo)
				{
					VesselExpansion.anchoredPosition = new Vector2(movingTo, VesselExpansion.anchoredPosition.y);
					expanding = false;
				}
			}
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

			if (windowInterface.ShowAllVessels)
			{
				CreateVesselSections(windowInterface.GetVessels);

				if (VesselExpansion != null)
					VesselExpansion.gameObject.SetActive(false);
			}
			else
			{
				currentBody = windowInterface.CurrentBody;

				CreateVesselSections(windowInterface.GetBodyVessels(windowInterface.CurrentBody));

				CreateBodySections(windowInterface.GetBodies);
			}

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

			rect.sizeDelta = new Vector2(rect.sizeDelta.x, pos.z);

			checkMaxResize((int)rect.sizeDelta.y);
		}

		public void OnExpandToggle(bool isOn)
		{
			if (VesselExpansion == null)
				return;

			expanding = true;

			if (isOn)
			{
				movingTo = 194;

				if (VesselPanelHandle != null)
					VesselPanelHandle.OnTextUpdate.Invoke("<<");
			}
			else
			{
				movingTo = 3;

				if (VesselPanelHandle != null)
					VesselPanelHandle.OnTextUpdate.Invoke(">>");
			}
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

			windowInterface.WindowPos = new Vector3(rect.anchoredPosition.x, rect.anchoredPosition.y, rect.sizeDelta.y);
		}

		public void onBeginResize(BaseEventData eventData)
		{
			if (!(eventData is PointerEventData))
				return;

			resizing = true;
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
			resizing = false;

			if (!(eventData is PointerEventData))
				return;

			if (rect == null)
				return;

			checkMaxResize((int)rect.sizeDelta.y);

			if (windowInterface == null)
				return;

			windowInterface.WindowPos = new Vector3(rect.anchoredPosition.x, rect.anchoredPosition.y, rect.sizeDelta.y);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			FadeIn(false);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!dragging && !resizing)
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
		}

		public void FadeIn(bool overrule)
		{
			Fade(1, fastFadeDuration, null, true, overrule);
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

		public void addVesselSection(IVesselSection section)
		{
			if (section == null)
				return;

			CreateVesselSection(section);
		}

		public void removeVesselSection(IVesselSection section)
		{
			if (section == null)
				return;

			for (int i = currentVessels.Count - 1; i >= 0; i--)
			{
				SEP_VesselSection v = currentVessels[i];

				if (v == null)
					continue;

				if (v.ID != section.ID)
					continue;

				currentVessels.RemoveAt(i);
				Destroy(v.gameObject);
				break;
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

			currentBodies.Add(bodyObject);
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

		public void RemoveBodySection(string body)
		{
			for (int i = currentBodies.Count - 1; i >= 0; i--)
			{
				SEP_CelestialBodyObject b = currentBodies[i];

				if (b == null)
					continue;

				if (b.Body != body)
					continue;

				currentBodies.RemoveAt(i);
				Destroy(b.gameObject);
				break;
			}
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

			for (int i = currentBodies.Count - 1; i >= 0; i--)
			{
				SEP_CelestialBodyObject b = currentBodies[i];

				if (b == null)
					continue;

				if (b.Body == body)
					continue;

				b.DisableBody();
			}

			var vessels = windowInterface.GetBodyVessels(body);

			if (vessels.Count <= 0)
				return;

			currentBody = body;

			windowInterface.CurrentBody = body;

			CreateVesselSections(vessels);
		}

		public void UpdateBodySection(string body, int count)
		{
			for (int i = currentBodies.Count - 1; i >= 0; i--)
			{
				SEP_CelestialBodyObject b = currentBodies[i];

				if (b == null)
					continue;

				if (b.Body != body)
					continue;

				b.UpdateCount(count);
				break;
			}
		}

		public void close()
		{
			Fade(0, fastFadeDuration, Kill, false);
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
