using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SEPScience.Unity.Unity
{
	public class SEP_CelestialBodyObject : MonoBehaviour
	{
		[SerializeField]
		private TextHandler BodyTitle = null;
		[SerializeField]
		private TextHandler VesselCount = null;

		private string Body;

		public void setBody(string body, int count)
		{
			Body = body;

			if (BodyTitle != null)
				BodyTitle.OnTextUpdate.Invoke(body);

			if (VesselCount != null)
				VesselCount.OnTextUpdate.Invoke(count + " Vessels");
		}

		public void SetBody()
		{
			if (SEP_Window.Window == null)
				return;

			SEP_Window.Window.SetCurrentBody(Body);
		}
	}
}
