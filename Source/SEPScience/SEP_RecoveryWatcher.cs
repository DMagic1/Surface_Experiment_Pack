#region license
/*The MIT License (MIT)
SEPRecoveryWatcher - A watcher for checking returned science and updating science values accordingly

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

namespace SEPScience
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class SEP_RecoveryWatcher : MonoBehaviour
	{
		private static bool loaded;

		private void Start()
		{
			if (loaded)
				Destroy(gameObject);

			loaded = true;

			GameEvents.OnScienceRecieved.Add(watcher);

			DontDestroyOnLoad(gameObject);
		}

		private void watcher(float sci, ScienceSubject sub, ProtoVessel v, bool b)
		{
			if (sub == null)
				return;

			if (!sub.id.StartsWith("SEP"))
				return;

			GameScenes scene = HighLogic.LoadedScene;

			List<ScienceSubject> subjects = ResearchAndDevelopment.GetSubjects();

			if (scene == GameScenes.SPACECENTER || scene == GameScenes.TRACKSTATION)
			{
				//SEPUtilities.log("Recovery detected...\nSubject ID - {0}\nScience Recovered - {1:N2}", logLevels.warning, sub.id, sci);

				SEP_Utilities.checkAndUpdateRelatedSubjects(subjects, sub, sci);
			}
			else if (scene == GameScenes.FLIGHT)
			{
				//SEPUtilities.log("Transmission detected...\nSubject ID - {0}\nScience Recovered - {1:N2}", logLevels.warning, sub.id, sci);

				SEP_Utilities.checkAndUpdateRelatedSubjects(subjects, sub, sci);
			}
		}
	}
}
