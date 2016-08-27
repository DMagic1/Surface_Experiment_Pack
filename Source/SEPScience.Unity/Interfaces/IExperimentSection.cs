using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SEPScience.Unity.Interfaces
{
	public interface IExperimentSection
	{

		string Name { get; }

		string DaysRemaining { get; }

		float Progress { get; }

		float Calibration { get; }

		bool IsRunning { get; }

		bool IsVisible { get; set; }

		bool CanTransmit { get; }

		void ToggleExperiment(bool on);

		void Update();
	}
}
