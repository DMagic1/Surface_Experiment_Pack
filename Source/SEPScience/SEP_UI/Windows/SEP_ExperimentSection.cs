using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

			float time = handler.experimentTime / handler.calibration;

			time *= 21600;

			time *= next;

			float nowTime = 0;

			if (handler.completion > 0)
				nowTime = (handler.completion / next) * time;

			float f = time - nowTime;

			string units = "";

			if (f <= KSPUtil.dateTimeFormatter.Day)
			{
				f /= KSPUtil.dateTimeFormatter.Hour;
				units = "Hours";
			}
			else
			{
				f /= KSPUtil.dateTimeFormatter.Day;
				units = "Days";
			}

			return string.Format("{0:N0} {1}", f, units);
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
