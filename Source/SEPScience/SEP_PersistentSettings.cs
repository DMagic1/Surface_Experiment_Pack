#region license
/*The MIT License (MIT)
SEP_PersistentSettings - A persistent storage module for saving settings to disk
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
using System.Reflection;
using System.IO;
using UnityEngine;

namespace SEPScience
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class SEP_PersistentSettings : MonoBehaviour
	{
		[Persistent]
		public bool showAllVessels;
		[Persistent]
		public bool fadeout = true;
		[Persistent]
		public float scale = 1;

		private const string fileName = "PluginData/Settings.cfg";
		private string fullPath;
		private SEP_GameParameters settings;

		private static SEP_PersistentSettings instance;

		public static SEP_PersistentSettings Instance
		{
			get { return instance; }
		}

		private void Awake()
		{
			if (instance != null)
				Destroy(gameObject);

			DontDestroyOnLoad(gameObject);
			
			instance = this;

			fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName).Replace("\\", "/");
			GameEvents.OnGameSettingsApplied.Add(SettingsApplied);

			if (Load())
				SEP_Utilities.log("[SEP Science] Settings file loaded", logLevels.log);
		}

		//private void OnDestroy()
		//{
		//	instance = null;

		//	GameEvents.OnGameSettingsApplied.Remove(SettingsApplied);
		//}

		public void SettingsApplied()
		{
			if (HighLogic.CurrentGame != null)
				settings = HighLogic.CurrentGame.Parameters.CustomParams<SEP_GameParameters>();

			if (settings == null)
				return;

			if (settings.useAsDefault)
			{
				showAllVessels = settings.showAllVessels;
				fadeout = settings.fadeOut;
				scale = settings.scale;

				if (Save())
					SEP_Utilities.log("[SEP Science] Settings file saved", logLevels.log);
			}
		}

		public bool Load()
		{
			bool b = false;

			try
			{
				if (File.Exists(fullPath))
				{
					ConfigNode node = ConfigNode.Load(fullPath);
					ConfigNode unwrapped = node.GetNode(GetType().Name);
					ConfigNode.LoadObjectFromConfig(this, unwrapped);
					b = true;
				}
				else
				{
					SEP_Utilities.log("[SEP Science] Settings file could not be found [{0}]", logLevels.warning, fullPath);
					b = false;
				}
			}
			catch (Exception e)
			{
				SEP_Utilities.log("[SEP Science] Error while loading settings file from [{0}]\n{1}", logLevels.error, fullPath, e);
				b = false;
			}

			return b;
		}

		public bool Save()
		{
			bool b = false;

			try
			{
				ConfigNode node = AsConfigNode();
				ConfigNode wrapper = new ConfigNode(GetType().Name);
				wrapper.AddNode(node);
				wrapper.Save(fullPath);
				b = true;
			}
			catch (Exception e)
			{
					SEP_Utilities.log("[SEP Science] Error while saving settings file from [{0}]\n{1}", logLevels.error, fullPath, e);
				b = false;
			}

			return b;
		}

		private ConfigNode AsConfigNode()
		{
			try
			{
				ConfigNode node = new ConfigNode(GetType().Name);

				node = ConfigNode.CreateConfigFromObject(this, node);
				return node;
			}
			catch (Exception e)
			{
					SEP_Utilities.log("[SEP Science] Failed to generate settings file node...\n{0}", logLevels.error, e);
				return new ConfigNode(GetType().Name);
			}
		}
	}
}
