#region license
/*The MIT License (MIT)
ModuleSEPECViewer - Part Module for adding resource info to EVA right-click menus

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
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SEPScience
{
	public class ModuleSEPECViewer : PartModule
	{
		private UIPartActionWindow window;

		public override void OnStart(PartModule.StartState state)
		{
			SEP_Utilities.onWindowSpawn.Add(onWindowSpawn);
			SEP_Utilities.onWindowDestroy.Add(onWindowDestroy);
		}

		private void OnDestroy()
		{
			SEP_Utilities.onWindowSpawn.Remove(onWindowSpawn);
			SEP_Utilities.onWindowDestroy.Remove(onWindowDestroy);
		}

		private void LateUpdate()
		{
			if (window == null)
				return;

			if (UIPartActionController.Instance == null)
				return;

			if (!SEP_Utilities.UIWindowReflectionLoaded)
				return;

			if (FlightGlobals.ActiveVessel == vessel)
				return;

			bool update = false;

			int l = part.Resources.Count;

			for (int i = 0; i < l; i++)
			{
				PartResource r = part.Resources[i];

				try
				{
					window.GetType().InvokeMember("AddResourceFlightControl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreReturn | BindingFlags.InvokeMethod, null, window, new object[] { r });
					update = true;
				}
				catch (Exception e)
				{
					SEP_Utilities.log("Error in adding EC Field to unfocused SEP UI Part Action Window\n{0}", logLevels.error, e);
					continue;
				}

				try
				{
					var items = SEP_Utilities.UIActionListField(window).GetValue(window) as List<UIPartActionItem>;

					int c = items.Count;

					for (int j = 0; j < c; j++)
					{
						UIPartActionItem item = items[j];

						if (item is UIPartActionResource)
							item.UpdateItem();
					}
				}
				catch (Exception e)
				{
					SEP_Utilities.log("Error in setting KSP Field on unfocused UI Part Action Window\n{0}", logLevels.error, e);
				}
			}

			if (update)
			{
				try
				{
					window.GetType().InvokeMember("PointerUpdate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreReturn | BindingFlags.InvokeMethod, null, window, null);
				}
				catch (Exception e)
				{
					SEP_Utilities.log("Error in updating unfocused UI Part Action Window position\n{0}", logLevels.error, e);
				}
			}
		}

		private void onWindowSpawn(UIPartActionWindow win)
		{
			if (win == null)
				return;

			if (win.part.flightID != part.flightID)
				return;

			if (FlightGlobals.ActiveVessel == vessel)
				return;

			window = win;
		}

		private void onWindowDestroy(UIPartActionWindow win)
		{
			if (win == null)
				return;

			if (win.part.flightID != part.flightID)
				return;

			if (window == null)
				return;

			window = null;
		}
	}
}
