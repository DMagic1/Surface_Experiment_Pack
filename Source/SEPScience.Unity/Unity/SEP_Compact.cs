﻿using System;
using System.Collections.Generic;
using SEPScience.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SEPScience.Unity.Unity
{
	public class SEP_Compact : CanvasFader, IBeginDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField]
		private GameObject VesselPrefab = null;
		[SerializeField]
		private Transform VesselTransform = null;
		[SerializeField]
		private float fastFadeDuration = 0.2f;
		[SerializeField]
		private float slowFadeDuration = 0.5f;

		private Vector2 mouseStart;
		private Vector3 windowStart;
		private RectTransform rect;
		
		private ISEP_Window windowInterface;
		private SEP_VesselSection currentVessel;

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

		public void Close()
		{
			Fade(0, fastFadeDuration, Hide);
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

			Fade(0, fastFadeDuration, Kill);
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

		public void FadeIn()
		{
			Fade(1, fastFadeDuration);
		}

		public void FadeOut()
		{
			Fade(0.6f, slowFadeDuration);
		}

	}
}
