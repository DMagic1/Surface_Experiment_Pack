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
