using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SEPScience.Unity.Unity;
using SEPScience.Unity.Interfaces;
using KSP.UI;
using KSP.UI.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace SEPScience.SEP_UI.Windows
{

	[SEP_KSPAddonImproved(SEP_KSPAddonImproved.Startup.TimeElapses, false)]
	public class SEP_AppLauncher : MonoBehaviour, ISEP_Window
	{
		private static Texture icon;
		private ApplicationLauncherButton button;

		private SEP_Window window;
		private GameObject window_Obj;
		private static GameObject window_Prefab;

		private bool windowSticky;
		private bool _windowMinimized;
		private bool _isVisible = true;

		private Vector3 anchor;

		private List<SEP_VesselSection> vessels = new List<SEP_VesselSection>();

		public bool IsMinimized
		{
			get { return _windowMinimized; }
			set { _windowMinimized = value; }
		}

		public bool IsVisible
		{
			get { return _isVisible; }
			set { _isVisible = value; }
		}

		public IList<IVesselSection> GetVessels()
		{
			List<IVesselSection> vesselList = new List<IVesselSection>(vessels.ToArray());

			return vesselList;
		}

		public void SetAppState(bool on)
		{
			if (on)
				button.SetTrue(true);
			else
				button.SetFalse(true);
		}

		public void ProcessStyle(GameObject obj)
		{
			if (obj == null)
				return;

			SEP_UI_Utilities.processComponents(obj);
		}

		public void UpdateWindow()
		{

		}

		private void Awake()
		{
			if (icon == null)
				icon = SEP_UI_Loader.Images.LoadAsset<Texture2D>("toolbar_icon");

			if (window_Prefab == null)
				window_Prefab = SEP_UI_Loader.Prefabs.LoadAsset<GameObject>("sep_window");

			StartCoroutine(getVessels());

			GameEvents.onGUIApplicationLauncherReady.Add(onReady);
			GameEvents.onGUIApplicationLauncherUnreadifying.Add(onUnreadifying);
		}

		private void OnDestroy()
		{
			GameEvents.onGUIApplicationLauncherReady.Remove(onReady);
			GameEvents.onGUIApplicationLauncherUnreadifying.Remove(onUnreadifying);

			for (int i = vessels.Count - 1; i >= 0; i--)
			{
				SEP_VesselSection v = vessels[i];

				if (v == null)
					continue;

				v.OnDestroy();
			}

			if (window != null)
				Destroy(window);

			if (window_Obj != null)
				Destroy(window_Obj);
		}
		
		private void onReady()
		{
			StartCoroutine(onReadyWait());
		}

		private IEnumerator onReadyWait()
		{
			while (!ApplicationLauncher.Ready)
				yield return null;

			while (ApplicationLauncher.Instance == null)
				yield return null;

			button = ApplicationLauncher.Instance.AddModApplication(onTrue, onFalse, onHover, onHoverOut, null, null, ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.TRACKSTATION, icon);
		}

		private void onUnreadifying(GameScenes scene)
		{
			if (ApplicationLauncher.Instance == null)
				return;

			if (button == null)
				return;

			ApplicationLauncher.Instance.RemoveModApplication(button);
		}

		private IEnumerator getVessels()
		{
			while (SEP_Controller.Instance == null)
				yield return null;

			while (!SEP_Controller.Instance.Setup)
				yield return null;

			processVesselSections();
		}

		private void processVesselSections()
		{
			for (int i = SEP_Controller.Instance.Vessels.Count - 1; i >= 0; i--)
			{
				Vessel v = SEP_Controller.Instance.Vessels[i];

				if (v == null)
					continue;

				if (!SEP_Controller.Instance.VesselLoaded(v))
					continue;

				vessels.Add(addVesselSection(v));
			}
		}

		private SEP_VesselSection addVesselSection(Vessel v)
		{
			return new SEP_VesselSection(v);
		}

		private void onTrue()
		{
			Open();

			windowSticky = true;
		}

		private void onFalse()
		{
			Close();
		}

		private void onHover()
		{
			Open();
		}

		private void onHoverOut()
		{
			if (window != null && windowSticky)
				window.FadeOut();

			if (!windowSticky)
				Close();
		}

		private Vector3 getAnchor()
		{
			if (anchor == null)
			{
				if (button == null)
					return Vector3.zero;

				anchor = button.GetAnchor();

				anchor.x -= 30;
			}

			return anchor;
		}

		private void Open()
		{
			if (window_Prefab == null)
				return;

			if (window != null && window_Obj != null)
			{
				//window_Obj.SetActive(true);

				window.gameObject.SetActive(true);

				window.FadeIn();

				SEP_Utilities.log("Setting UI Active...", logLevels.warning);

				return;
			}

			window_Obj = Instantiate(window_Prefab, getAnchor(), Quaternion.identity) as GameObject;

			if (window_Obj == null)
				return;

			SEP_UI_Utilities.processComponents(window_Obj);

			window_Obj.transform.SetParent(ApplicationLauncher.Instance.appSpace, false);

			window = window_Obj.GetComponent<SEP_Window>();

			if (window == null)
				return;

			window.setWindow(this);

			//window_Obj.SetActive(true);

			window.gameObject.SetActive(true);

			SEP_Utilities.log("Opening UI...", logLevels.warning);
		}

		private void Close()
		{
			windowSticky = false;

			if (window == null)
				return;

			window.close();
			return;

			if (window_Obj == null)
				return;

			//window_Obj.SetActive(false);

			window.gameObject.SetActive(false);

			SEP_Utilities.log("Closing UI...", logLevels.warning);
						
			return;

			SEP_Utilities.log("Closing UI...", logLevels.warning);

			Destroy(window_Obj);
		}


	}
}
