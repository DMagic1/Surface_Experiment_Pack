using System;
using System.Collections.Generic;
using UnityEngine;
namespace SEPScience.Unity.Interfaces
{
	public interface IVesselSection
	{
		bool IsConnected { get; }

		bool CanTransmit { get; set; }

		bool AutoTransmitAvailable { get; }

		string ECTotal { get; }

		string Name { get; }

		string Situation { get; }

		bool IsVisible { get; set; }

		IList<IExperimentSection> GetExperiments();

		void ProcessStyle(GameObject obj);

		void PauseAll();

		void StartAll();

		void Update();
	}
}
