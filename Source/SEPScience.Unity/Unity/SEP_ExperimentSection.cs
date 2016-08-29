using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SEPScience.Unity.Interfaces;

namespace SEPScience.Unity.Unity
{
	public class SEP_ExperimentSection : MonoBehaviour
	{
		[SerializeField]
		private Text Title = null;
		[SerializeField]
		private Slider BaseSlider = null;
		[SerializeField]
		private Slider FrontSlider = null;
		[SerializeField]
		private Text Remaining = null;
		[SerializeField]
		private Image ToggleBackground = null;
		[SerializeField]
		private Sprite PlayIcon = null;
		[SerializeField]
		private Sprite PauseIcon = null;

		private IExperimentSection experimentInterface;
		private SEP_VesselSection parent;

		private void OnDestroy()
		{
			if (parent == null)
				return;

			parent.RemoveExperimentSection(this);

			gameObject.SetActive(false);
		}

		private void Update()
		{
			if (experimentInterface == null)
				return;

			if (!experimentInterface.IsVisible)
				return;						

			experimentInterface.Update();

			if (Remaining != null)
				Remaining.text = experimentInterface.DaysRemaining;

			if (BaseSlider != null && FrontSlider != null)
			{
				BaseSlider.normalizedValue = Mathf.Clamp01(experimentInterface.Calibration);

				FrontSlider.normalizedValue = Mathf.Clamp01(experimentInterface.Progress);
			}

			if (ToggleBackground != null && PlayIcon != null && PauseIcon != null)
				ToggleBackground.sprite = experimentInterface.IsRunning ? PauseIcon : PlayIcon;
		}

		public void toggleVisibility(bool on)
		{
			if (experimentInterface == null)
				return;

			experimentInterface.IsVisible = on;

			gameObject.SetActive(on);
		}

		public bool experimentRunning
		{
			get
			{
				if (experimentInterface == null)
					return false;

				return experimentInterface.IsRunning;
			}
		}

		public void setExperiment(IExperimentSection experiment, SEP_VesselSection vessel)
		{
			if (experiment == null)
				return;

			if (vessel == null)
				return;

			parent = vessel;

			experimentInterface = experiment;

			if (Title != null)
				Title.text = experiment.Name;

			if (Remaining != null)
				Remaining.text = experimentInterface.DaysRemaining;

			if (ToggleBackground != null && PlayIcon != null && PauseIcon != null)
				ToggleBackground.sprite = experimentInterface.IsRunning ? PauseIcon : PlayIcon;

			if (BaseSlider != null && FrontSlider != null)
			{
				BaseSlider.normalizedValue = Mathf.Clamp01(experimentInterface.Calibration);

				FrontSlider.normalizedValue = Mathf.Clamp01(experimentInterface.Progress);
			}

			experimentInterface.setParent(this);
		}

		public void toggleExperiment()
		{
			if (experimentInterface == null)
				return;

			experimentInterface.ToggleExperiment(!experimentInterface.IsRunning);

			if (ToggleBackground == null || PlayIcon == null || PauseIcon == null)
				return;

			ToggleBackground.sprite = experimentInterface.IsRunning ? PauseIcon : PlayIcon;
		}

	}
}
