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

using System;
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
		private static SEP_AppLauncher instance;
		private static Vector3 _windowPos;
		private ApplicationLauncherButton button;

		private SEP_Window window;
		private SEP_Compact compactWindow;
		private SEP_GameParameters settings;

		private bool processed;
		private bool windowSticky;
		private bool _windowMinimized;
		private bool _isVisible = true;

		private DictionaryValueList<Guid, SEP_VesselSection> vessels;
		private int _currentVessel;
		private string _currentBody;

		public static SEP_AppLauncher Instance
		{
			get { return instance; }
		}

		public bool IsMinimized
		{
			get { return _windowMinimized; }
			set
			{
				_windowMinimized = value;

				if (!_isVisible)
					return;

				Close();

				windowSticky = true;

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

		public float Scale
		{
			get { return settings.scale; }
		}

		public Vector3 WindowPos
		{
			get { return _windowPos; }
			set { _windowPos = value; }
		}

		public IList<IVesselSection> GetVessels
		{
			get
			{
				var v = vessels.GetListEnumerator();

				List<IVesselSection> sections = new List<IVesselSection>();

				while (v.MoveNext())
					sections.Add(v.Current);

				return new List<IVesselSection>(sections);
			}
		}

		public IList<IVesselSection> GetBodyVessels(string body)
		{
			List<IVesselSection> vesselList = new List<IVesselSection>();

			if (string.IsNullOrEmpty(body))
				return vesselList;

			int l = vessels.Count;

			for (int i = 0; i < l; i++)
			{
				SEP_VesselSection vessel = vessels.At(i);

				if (vessel == null)
					continue;

				if (vessel.VesselBody == null)
					continue;

				if (vessel.VesselBody.bodyName != body)
					continue;

				vesselList.Add(vessel);
			}
			
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
						SEP_VesselSection vessel = vessels.At(j);

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
			get
			{
				if (string.IsNullOrEmpty(_currentBody))
				{
					var bodies = GetBodies;

					if (GetBodies.Count > 0)
						_currentBody = GetBodies[0];
				}

				return _currentBody;
			}
			set
			{
				if (GetBodies.Contains(value))
					_currentBody = value;
			}
		}

		public void ChangeVessel(bool forward)
		{
			int i = _currentVessel + (forward ? 1 : -1);

			if (i < 0)
				i = vessels.Count - 1;

			if (i >= vessels.Count)
				i = 0;

			_currentVessel = i;

			if (compactWindow == null)
				return;

			compactWindow.SetNewVessel(vessels.At(i));
		}

		public IVesselSection CurrentVessel
		{
			get
			{
				if (vessels.Count > _currentVessel)
					return vessels.At(_currentVessel);

				if (vessels.Count > 0)
					return vessels.At(0);

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

		public SEP_VesselSection getVesselSection(Vessel v)
		{
			if (vessels.Contains(v.id))
				return vessels[v.id];

			return null;
		}

		private void Awake()
		{
			if (HighLogic.LoadedSceneIsEditor)
				Destroy(gameObject);

			if (instance != null)
				Destroy(gameObject);

			instance = this;

			vessels = new DictionaryValueList<Guid, SEP_VesselSection>();

			if (icon == null)
				icon = GameDatabase.Instance.GetTexture("SurfaceExperimentPackage/Resources/Toolbar_Icon", false);
			
			StartCoroutine(getVessels());

			GameEvents.onGUIApplicationLauncherReady.Add(onReady);
			GameEvents.onGUIApplicationLauncherUnreadifying.Add(onUnreadifying);
			GameEvents.OnGameSettingsApplied.Add(onSettingsApplied);
			SEP_Utilities.onExperimentActivate.Add(onExpActivate);
			SEP_Utilities.onExperimentDeactivate.Add(onExpDeactivate);
		}

		private void Start()
		{
			settings = HighLogic.CurrentGame.Parameters.CustomParams<SEP_GameParameters>();
		}

		private void OnDestroy()
		{
			instance = null;

			GameEvents.onGUIApplicationLauncherReady.Remove(onReady);
			GameEvents.onGUIApplicationLauncherUnreadifying.Remove(onUnreadifying);
			GameEvents.OnGameSettingsApplied.Remove(onSettingsApplied);
			SEP_Utilities.onExperimentActivate.Remove(onExpActivate);
			SEP_Utilities.onExperimentDeactivate.Remove(onExpDeactivate);

			for (int i = vessels.Count - 1; i >= 0; i--)
			{
				SEP_VesselSection v = vessels.At(i);

				if (v == null)
					continue;

				v.OnDestroy();
			}

			if (window != null)
				Destroy(window.gameObject);

			if (compactWindow != null)
				Destroy(compactWindow.gameObject);
		}

		private void onSettingsApplied()
		{
			settings = HighLogic.CurrentGame.Parameters.CustomParams<SEP_GameParameters>();

			if (window != null)
				window.SetScale(settings.scale);

			if (compactWindow != null)
				compactWindow.SetScale(settings.scale);
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
			
			if (!processed)
				button.Disable(false);
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

			processed = true;

			button.Enable(false);
		}

		private void processVesselSections()
		{
			List<Vessel> vs = SEP_Controller.Instance.GetAllVessels();

			for (int i = vs.Count - 1; i >= 0; i--)
			{
				Vessel v = vs[i];

				if (v == null)
					continue;

				vessels.Add(v.id, new SEP_VesselSection(v));
			}
		}

		private void onExpActivate(Vessel v, SEP_ExperimentHandler h)
		{
			if (v == null)
				return;

			if (h == null)
				return;

			if (vessels.Contains(v.id))
				vessels[v.id].AddExperiment(h);
			else
			{

			}
		}

		private void onExpDeactivate(Vessel v, SEP_ExperimentHandler h)
		{
			if (v == null)
				return;

			if (h == null)
				return;

			if (!vessels.Contains(v.id))
				return;

			vessels[v.id].RemoveExperiment(h);
		}

		public void addVesselSection(Vessel v)
		{
			if (v == null)
				return;

			if (vessels.Contains(v.id))
				return;

			IList<string> bodies = GetBodies;

			SEP_VesselSection s = new SEP_VesselSection(v);

			vessels.Add(v.id, s);

			if (_windowMinimized || window == null)
				return;

			if (!settings.showAllVessels)
			{
				string name = v.mainBody.bodyName;

				bool flag = false;

				for (int i = bodies.Count - 1; i >= 0; i--)
				{
					string b = bodies[i];

					if (b != name)
						continue;

					flag = true;
					break;
				}

				if (!flag)
				{
					window.AddBodySection(name);

					if (bodies.Count <= 0)
						window.addVesselSection(s);
				}
				else
					window.UpdateBodySection(name, GetBodyVessels(name).Count);

				if (_currentBody != name)
					return;
			}

			window.addVesselSection(s);
		}

		public void removeVesselSection(Vessel v)
		{
			if (v == null)
				return;

			if (!vessels.Contains(v.id))
				return;

			if (!_windowMinimized && window != null)
			{
				if (!settings.showAllVessels)
				{
					string name = v.mainBody.bodyName;

					int count = GetBodyVessels(name).Count - 1;

					if (count <= 0)
						window.RemoveBodySection(name);
					else
						window.UpdateBodySection(name, count);

					if (_currentBody != name)
						return;
				}

				window.removeVesselSection(vessels[v.id]);
			}
			else if (_windowMinimized && compactWindow != null)
			{
				if (compactWindow.ID == v.id)
					compactWindow.GoNext();
			}

			vessels.Remove(v.id);
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
			if (windowSticky)
			{
				if (_windowMinimized)
				{
					if (compactWindow != null)
						compactWindow.FadeOut();
				}
				else
				{
					if (window != null)
						window.FadeOut();
				}
			}
			else
				Close();
		}

		private Vector3 getAnchor()
		{
			if (_windowPos == Vector3.zero)
			{
				float scale = GameSettings.UI_SCALE;

				int width = Screen.width;
				int height = Screen.height;

				float x = (width - (520 * scale * Scale) ) / scale;
				float y = ((height / 2) - (100 * scale * Scale)) / scale;

				_windowPos = new Vector3(x, y * -1f, 200);				
			}

			return _windowPos;
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
				window.FadeIn(false);

				return;
			}

			GameObject obj = Instantiate(SEP_UI_Loader.WindowPrefab) as GameObject;

			if (obj == null)
				return;

			obj.transform.SetParent(MainCanvasUtil.MainCanvas.transform, false);

			window = obj.GetComponent<SEP_Window>();

			if (window == null)
				return;

			window.setWindow(this);

			window.gameObject.SetActive(true);

			window.SetPosition(getAnchor());

			_isVisible = true;
		}

		private void OpenCompact()
		{
			if (SEP_UI_Loader.CompactPrefab == null)
				return;

			if (compactWindow != null)
			{
				compactWindow.FadeIn(false);

				return;
			}

			GameObject obj = Instantiate(SEP_UI_Loader.CompactPrefab) as GameObject;

			if (obj == null)
				return;

			obj.transform.SetParent(MainCanvasUtil.MainCanvas.transform, false);

			compactWindow = obj.GetComponent<SEP_Compact>();

			if (compactWindow == null)
				return;

			compactWindow.setWindow(this);

			compactWindow.SetPosition(getAnchor());

			compactWindow.gameObject.SetActive(true);

			_isVisible = true;
		}

		private void Close()
		{
			windowSticky = false;

			_isVisible = false;

			if (window != null)
			{
				window.close();
				window = null;
			}

			if (compactWindow != null)
			{
				compactWindow.Close();
				compactWindow = null;
			}
		}
	}
}
