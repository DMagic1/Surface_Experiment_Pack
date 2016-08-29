using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SEPScience.Unity
{
	[RequireComponent(typeof(CanvasGroup))]
	public class CanvasFader : MonoBehaviour
	{
		private CanvasGroup canvas;
		private IEnumerator fader;

		protected virtual void Awake()
		{
			canvas = GetComponent<CanvasGroup>();
		}

		public bool Fading
		{
			get { return fader != null; }
		}

		protected void Fade(float to, float duration, Action call = null)
		{
			if (canvas == null)
				return;

			Fade(canvas.alpha, to, duration, call);
		}

		protected void Alpha(float to)
		{
			if (canvas == null)
				return;

			to = Mathf.Clamp01(to);
			canvas.alpha = to;
		}

		private void Fade(float from, float to, float duration, Action call)
		{
			if (fader != null)
				StopCoroutine(fader);

			fader = FadeRoutine(from, to, duration, call);
			StartCoroutine(fader);
		}

		private IEnumerator FadeRoutine(float from, float to, float duration, Action call)
		{
			yield return new WaitForEndOfFrame();

			float f = 0;

			while (f <= 1)
			{
				f += Time.deltaTime / duration;
				Alpha(Mathf.Lerp(from, to, f));
				yield return null;
			}

			if (call != null)
				call.Invoke();

			fader = null;
		}

	}
}
