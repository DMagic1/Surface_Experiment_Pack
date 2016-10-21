#region license
/*The MIT License (MIT)
SEP_AppLauncher - App launcher button for controlling the SEP window

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

using System.Collections;
using System.Collections.Generic;
using SEPScience.Unity.Unity;
using SEPScience.Unity.Interfaces;
using KSP.UI.Screens;
using UnityEngine;

namespace SEPScience.SEP_UI.Windows
{

	[KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
	public class SEP_AppLauncher : MonoBehaviour, ISEP_Window
	{
		private static Texture icon;
		private ApplicationLauncherButton button;

		private SEP_Window window;
		private SEP_Compact compactWindow;
		private SEP_GameParameters settings;

		private bool windowSticky;
		private bool _windowMinimized;
		private bool _isVisible = true;

		private Vector3 anchor;

		private List<SEP_VesselSection> vessels = new List<SEP_VesselSection>();
		private int _currentVessel;
		private string _currentBody;

		public bool IsMinimized
		{
			get { return _windowMinimized; }
			set
			{
				_windowMinimized = value;

				if (!_isVisible)
					return;

				if (value)
					OpenCompact();
				else
					OpenStandard();
			}
		}

		public bool IsVisible
		{
			get { return _isVisible; }
			set { _isVisible = value; }
		}

		public bool ShowAllVessels
		{
			get { return settings.showAllVessels; }
		}

		public IList<IVesselSection> GetVessels
		{
			get
			{
				List<IVesselSection> vesselList = new List<IVesselSection>(vessels.ToArray());

				return vesselList;
			}
		}

		public IList<IVesselSection> GetBodyVessels(string body)
		{
			List<IVesselSection> vesselList = new List<IVesselSection>();

			int l = vessels.Count;

			for (int i = 0; i < l; i++)
			{
				SEP_VesselSection vessel = vessels[i];

				if (vessel == null)
					continue;

				if (vessel.VesselBody == null)
					continue;

				vesselList.Add(vessel);
			}

			if (vesselList.Count > 0)
				_currentBody = body;

			return vesselList;
		}

		public IList<string> GetBodies
		{
			get
			{
				List<string> bodies = new List<string>();

				for (int i = FlightGlobals.Bodies.Count - 1; i >= 0; i--)
				{
					CelestialBody body = FlightGlobals.Bodies[i];

					if (body == null)
						continue;

					for (int j = vessels.Count - 1; j >= 0; j--)
					{
						SEP_VesselSection vessel = vessels[j];

						if (vessel == null)
							continue;

						if (vessel.VesselBody != body)
							continue;

						bodies.Add(body.bodyName);
						break;
					}
				}

				return bodies;
			}
		}

		public string CurrentBody
		{
			get { return _currentBody; }
		}

		public void ChangeVessel(bool forward)
		{
			int i = _currentVessel + (forward ? 1 : -1);

			if (i < 0)
				_currentVessel = vessels.Count - 1;

			if (i > vessels.Count)
				_currentVessel = 0;

			if (compactWindow == null)
				return;

			compactWindow.SetNewVessel(vessels[_currentVessel]);
		}

		public IVesselSection CurrentVessel
		{
			get
			{
				if (vessels.Count > _currentVessel)
					return vessels[_currentVessel];

				if (vessels.Count > 0)
					return vessels[0];

				return null;
			}
		}

		public void SetAppState(bool on)
		{
			if (on)
				button.SetTrue(true);
			else
				button.SetFalse(true);
		}

		public void UpdateWindow()
		{

		}

		private void Awake()
		{
			if (HighLogic.LoadedSceneIsEditor)
				Destroy(gameObject);

			if (icon == null)
				icon = GameDatabase.Instance.GetTexture("GameData/SEPScience/Resources/toolbar_icon", false);
			
			StartCoroutine(getVessels());

			GameEvents.onGUIApplicationLauncherReady.Add(onReady);
			GameEvents.onGUIApplicationLauncherUnreadifying.Add(onUnreadifying);
		}

		private void Start()
		{
			settings = HighLogic.CurrentGame.Parameters.CustomParams<SEP_GameParameters>();
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
				Destroy(window.gameObject);
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
			if (_windowMinimized)
				OpenCompact();
			else
				OpenStandard();
		}

		private void OpenStandard()
		{
			if (SEP_UI_Loader.WindowPrefab == null)
				return;

			if (window != null)
			{
				window.gameObject.SetActive(true);

				window.FadeIn();

				return;
			}

			GameObject obj = Instantiate(SEP_UI_Loader.WindowPrefab, getAnchor(), Quaternion.identity) as GameObject;

			if (obj == null)
				return;

			obj.transform.SetParent(ApplicationLauncher.Instance.appSpace, false);

			window = obj.GetComponent<SEP_Window>();

			if (window == null)
				return;

			window.setWindow(this);

			window.gameObject.SetActive(true);
		}

		private void OpenCompact()
		{
			if (SEP_UI_Loader.CompactPrefab == null)
				return;

			if (compactWindow != null)
			{
				compactWindow.gameObject.SetActive(true);

				compactWindow.FadeIn();

				return;
			}

			GameObject obj = Instantiate(SEP_UI_Loader.CompactPrefab, getAnchor(), Quaternion.identity) as GameObject;

			if (obj == null)
				return;

			obj.transform.SetParent(ApplicationLauncher.Instance.appSpace, false);

			compactWindow = obj.GetComponent<SEP_Compact>();

			if (compactWindow == null)
				return;

			compactWindow.setWindow(this);

			compactWindow.gameObject.SetActive(true);
		}

		private void Close()
		{
			windowSticky = false;

			if (window != null)
				window.close();

			if (compactWindow != null)
				compactWindow.Close();
		}


	}
}
