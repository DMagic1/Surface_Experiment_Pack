using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SEPScience
{
	public class SEPUIWindow : MonoBehaviour
	{
		private UIPartActionWindow window;

		private void Start()
		{
			window = gameObject.GetComponentInParent<UIPartActionWindow>();

			if (window == null)
				Destroy(gameObject);

			//SEPUtilities.log("Window Object Assigned", logLevels.log);

			SEPUtilities.onWindowSpawn.Fire(window);
		}

		private void OnDestroy()
		{
			//SEPUtilities.log("Destroy UI Window Prefab script", logLevels.log);

			SEPUtilities.onWindowDestroy.Fire(window);
		}
	}
}
