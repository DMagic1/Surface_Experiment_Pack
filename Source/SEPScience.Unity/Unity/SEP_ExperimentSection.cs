#region license
/*The MIT License (MIT)
SEP_ExperimentSection - Unity UI element for SEP experiments

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

using UnityEngine;
using UnityEngine.UI;
using SEPScience.Unity.Interfaces;

namespace SEPScience.Unity.Unity
{
	public class SEP_ExperimentSection : MonoBehaviour
	{
		[SerializeField]
		private TextHandler Title = null;
		[SerializeField]
		private Slider BaseSlider = null;
		[SerializeField]
		private Slider FrontSlider = null;
		[SerializeField]
		private TextHandler Remaining = null;
		[SerializeField]
		private Selectable ExperimentSelectable = null;
		[SerializeField]
		private Sprite[] ToggleIcons = null;

		private IExperimentSection experimentInterface;
		private SEP_VesselSection parent;
		private bool toggleState;

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
				Remaining.OnTextUpdate.Invoke(experimentInterface.DaysRemaining);

			if (BaseSlider != null && FrontSlider != null)
			{
				BaseSlider.normalizedValue = Mathf.Clamp01(experimentInterface.Calibration);

				FrontSlider.normalizedValue = Mathf.Clamp01(experimentInterface.Progress);
			}

			UpdateToggleButton(experimentInterface.IsRunning);
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
				Title.OnTextUpdate.Invoke(experiment.Name);

			if (Remaining != null)
				Remaining.OnTextUpdate.Invoke(experimentInterface.DaysRemaining);

			toggleState = !experimentInterface.IsRunning;

			UpdateToggleButton(experimentInterface.IsRunning);

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

			UpdateToggleButton(experimentInterface.IsRunning);
		}

		private void UpdateToggleButton(bool isOn)
		{
			if (toggleState == isOn)
				return;

			toggleState = isOn;

			if (ExperimentSelectable == null)
				return;

			if (ToggleIcons.Length < 6)
				return;

			ExperimentSelectable.image.sprite = isOn ? ToggleIcons[3] : ToggleIcons[0];
			ExperimentSelectable.image.type = Image.Type.Simple;
			ExperimentSelectable.transition = Selectable.Transition.SpriteSwap;

			SpriteState state = ExperimentSelectable.spriteState;
			state.highlightedSprite = isOn ? ToggleIcons[4] : ToggleIcons[1];
			state.pressedSprite = isOn ? ToggleIcons[5] : ToggleIcons[2];
			state.disabledSprite = null;
			ExperimentSelectable.spriteState = state;
		}

	}
}
