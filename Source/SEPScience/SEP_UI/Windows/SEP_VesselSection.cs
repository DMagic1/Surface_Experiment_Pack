using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SEPScience.Unity.Interfaces;
using UnityEngine;

namespace SEPScience.SEP_UI.Windows
{
	public class SEP_VesselSection : IVesselSection
	{
		private string _name;
		private string _ectotal;
		private string _situation;
		private bool _isvisible = true;
		private bool _cantransmit;
		private bool _isconnected;
		private bool _transmitavailable;

		private double maxVesselEC;
		private double currentVesselEC;

		private Vessel vessel;
		private List<SEP_ExperimentHandler> experiments = new List<SEP_ExperimentHandler>();
		private List<SEP_ExperimentSection> experimentSections = new List<SEP_ExperimentSection>();

		public SEP_VesselSection(Vessel v)
		{
			if (SEP_Controller.Instance == null)
				return;

			if (v == null)
				return;

			vessel = v;
			_name = v.vesselName;

			experiments = SEP_Controller.Instance.getHandlers(v);

			_transmitavailable = SEP_Controller.Instance.TransmissionUpdgrade;
			_cantransmit = experiments.Any(e => e.controllerAutoTransmit);

			currentVesselEC = SEP_Utilities.getTotalVesselEC(v.protoVessel);
			maxVesselEC = SEP_Utilities.getMaxTotalVesselEC(v.protoVessel);

			_ectotal = getECString();

			_situation = getSituationString();

			experimentSections = new List<SEP_ExperimentSection>();
			addExperimentSections();

			GameEvents.onVesselWasModified.Add(onVesselModified);
			GameEvents.onVesselSituationChange.Add(onVesselSituationChange);
		}

		public void OnDestroy()
		{
			GameEvents.onVesselWasModified.Remove(onVesselModified);
			GameEvents.onVesselSituationChange.Remove(onVesselSituationChange);
		}

		public void Update()
		{
			if (vessel == null)
				return;

			if (vessel.protoVessel == null)
				return;

			currentVesselEC = SEP_Utilities.getTotalVesselEC(vessel.protoVessel);

			_ectotal = getECString();

			_isconnected = true;
		}

		private string getECString()
		{
			if (vessel.loaded)
				return string.Format("{0:N0} / {1:N0} EC", currentVesselEC, maxVesselEC);
			else
				return "";
		}

		private string getSituationString()
		{
			return string.Format("{0} - Surface - {1}", vessel.mainBody.bodyName, ScienceUtil.GetExperimentBiome(vessel.mainBody, vessel.latitude, vessel.longitude));
		}

		private bool getConnectionStatus()
		{
			return true;
		}

		public string Name
		{
			get { return _name; }
		}

		public bool IsVisible
		{
			get { return _isvisible; }
			set { _isvisible = value; }
		}

		public bool IsConnected
		{
			get { return _isconnected; }
		}

		public bool CanTransmit
		{
			get { return _cantransmit; }
			set { _cantransmit = value; }
		}

		public bool AutoTransmitAvailable
		{
			get { return _transmitavailable; }
		}

		public string ECTotal
		{
			get { return _ectotal; }
		}

		public string Situation
		{
			get { return _situation; }
		}

		public IList<IExperimentSection> GetExperiments()
		{
			return new List<IExperimentSection>(experimentSections.ToArray());
		}

		public void ProcessStyle(GameObject obj)
		{
			if (obj == null)
				return;

			SEP_UI_Utilities.processComponents(obj);
		}

		public void StartAll()
		{
			if (!_isconnected)
				return;

			for (int i = experiments.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentHandler handler = experiments[i];

				if (handler == null)
					continue;

				if (handler.experimentRunning)
					continue;

				if (vessel.loaded && handler.host != null)
					handler.host.DeployExperiment();
				else
				{
					handler.experimentRunning = true;
					handler.lastBackgroundCheck = Planetarium.GetUniversalTime();
				}
			}
		}

		public void PauseAll()
		{
			if (!_isconnected)
				return;

			for (int i = experiments.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentHandler handler = experiments[i];

				if (handler == null)
					continue;

				if (!handler.experimentRunning)
					continue;

				if (vessel.loaded && handler.host != null)
					handler.host.PauseExperiment();
				else
					handler.experimentRunning = false;
			}
		}

		private void addExperimentSections()
		{
			for (int i = experiments.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentHandler handler = experiments[i];

				if (handler == null)
					return;

				experimentSections.Add(addExperimentSection(experiments[i]));
			}
		}

		private SEP_ExperimentSection addExperimentSection(SEP_ExperimentHandler handler)
		{
			return new SEP_ExperimentSection(handler, vessel);
		}

		private void onVesselModified(Vessel v)
		{
			if (v == null)
				return;

			if (v != vessel)
				return;

			maxVesselEC = SEP_Utilities.getMaxTotalVesselEC(v.protoVessel);

			_ectotal = getECString();
		}

		private void onVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> V)
		{
			if (V.host == null)
				return;

			if (V.host != vessel)
				return;

			_situation = getSituationString();
		}
	}
}
