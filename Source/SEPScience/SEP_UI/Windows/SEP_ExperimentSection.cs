#region license
/*The MIT License (MIT)
SEP_ExperimentSection - UI interface for holding info about SEP experiments

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

using SEPScience.Unity.Interfaces;
using UnityEngine;

namespace SEPScience.SEP_UI.Windows
{
	public class SEP_ExperimentSection : IExperimentSection
	{
		private string _name;
		private string _daysremaining;
		private bool _isrunning;
		private bool _isvisible = true;
		private bool _cantransmit;
		private float _calibration;
		private float _progress;

		private Vessel vessel;
		private SEPScience.Unity.Unity.SEP_ExperimentSection experimentUISection;
		private SEP_ExperimentHandler handler;

		public SEP_ExperimentSection(SEP_ExperimentHandler h, Vessel v)
		{
			if (h == null)
				return;

			if (v == null)
				return;

			vessel = v;

			handler = h;
			_name = h.experimentTitle;
			_cantransmit = h.canTransmit;

			_daysremaining = getDaysRemaining();
			_progress = h.completion;
			_calibration = h.calibration;
			_isrunning = h.experimentRunning;
		}

		public void OnDestroy()
		{
			if (experimentUISection != null)
				MonoBehaviour.Destroy(experimentUISection);
		}

		public string Name
		{
			get { return _name; }
		}

		public string DaysRemaining
		{
			get { return _daysremaining; }
		}

		public float Calibration
		{
			get { return _calibration; }
		}

		public float Progress
		{
			get { return _progress; }
		}

		public bool IsRunning
		{
			get { return _isrunning; }
		}

		public bool IsVisible
		{
			get { return _isvisible; }
			set { _isvisible = value; }
		}

		public bool CanTransmit
		{
			get { return _cantransmit; }
		}

		public SEP_ExperimentHandler Handler
		{
			get { return handler; }
		}

		public void ToggleExperiment(bool on)
		{
			if (on && !handler.experimentRunning)
			{
				if (vessel.loaded && handler.host != null)
					handler.host.DeployExperiment();
				else
				{
					handler.experimentRunning = true;
					handler.lastBackgroundCheck = Planetarium.GetUniversalTime();
				}

				_isrunning = true;
			}
			else if (!on && handler.experimentRunning)
			{
				if (vessel.loaded && handler.host != null)
					handler.host.PauseExperiment();
				else
					handler.experimentRunning = false;

				_isrunning = false;
			}
		}

		public void setParent(SEPScience.Unity.Unity.SEP_ExperimentSection section)
		{
			experimentUISection = section;
		}

		public void Update()
		{
			if (handler == null)
				return;

			_daysremaining = getDaysRemaining();
			_progress = handler.completion;
			_calibration = handler.calibration;
			_isrunning = handler.experimentRunning;
		}

		private string getDaysRemaining()
		{
			if (handler == null)
				return "Error...";

			float next = getNextCompletion(handler.completion);

			if (handler.completion >= next)
				return "Complete";

			if (handler.calibration <= 0)
				return "∞";

			if (!handler.experimentRunning)
				return "";

			float calib = handler.calibration;

			if (SEP_Controller.Instance.UsingCommNet)
			{
				if (vessel.Connection != null)
				{
					float signal = (float)vessel.Connection.SignalStrength - 0.5f;

					if (signal < 0)
						signal /= 2;

					float bonus = calib * signal;

					calib += bonus;
				}
			}

			float time = handler.experimentTime / calib;

			time *= 21600;

			time *= next;

			float nowTime = 0;

			if (handler.completion > 0)
				nowTime = (handler.completion / next) * time;

			float f = time - nowTime;

			return KSPUtil.PrintTime(f, 2, false);
		}

		private float getNextCompletion(float f)
		{
			if (f < 0.5f)
				return 0.5f;
			if (f < 0.75f)
				return 0.75f;
			return 1f;
		}

	}
}
