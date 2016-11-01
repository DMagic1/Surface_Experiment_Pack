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
		private Image SelectedImage = null;

		private string body;

		public string Body
		{
			get { return body; }
		}

		public void setBody(string b, int count)
		{
			body = b;

			if (BodyTitle != null)
				BodyTitle.OnTextUpdate.Invoke(string.Format("{0}: {1} Station{2}", b, count, count > 1 ? "s" : ""));

			if (SEP_Window.Window == null)
				return;

			if (SelectedImage == null)
				return;

			if (SEP_Window.Window.CurrentBody == body)
				SelectedImage.gameObject.SetActive(true);
			else
				SelectedImage.gameObject.SetActive(false);
		}

		public void UpdateCount(int count)
		{
			if (BodyTitle != null)
				BodyTitle.OnTextUpdate.Invoke(string.Format("{0}: {1} Station{2}", body, count, count > 1 ? "s" : ""));
		}

		public void DisableBody()
		{
			if (SelectedImage != null)
				SelectedImage.gameObject.SetActive(false);
		}

		public void SetBody()
		{
			if (SEP_Window.Window == null)
				return;

			if (SEP_Window.Window.CurrentBody == body)
				return;

			SEP_Window.Window.SetCurrentBody(body);

			if (SelectedImage != null)
				SelectedImage.gameObject.SetActive(true);
		}
	}
}
