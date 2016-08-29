

using System.Collections.Generic;
using UnityEngine;
using SEPScience.Unity.Unity;

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

		string ExpCount { get; }

		bool IsVisible { get; set; }

		IList<IExperimentSection> GetExperiments();

		void ProcessStyle(GameObject obj);

		void setParent(SEP_VesselSection section);

		void PauseAll();

		void StartAll();

		void Update();
	}
}
