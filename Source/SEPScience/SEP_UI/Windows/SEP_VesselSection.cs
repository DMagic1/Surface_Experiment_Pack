#region license
/*The MIT License (MIT)
SEP_VesselSection - UI Interface holding info on vessel's with SEP experiments

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

using System;
using System.Collections.Generic;
using System.Linq;
using SEPScience.Unity.Interfaces;
using UnityEngine;

namespace SEPScience.SEP_UI.Windows
{
	public class SEP_VesselSection : IVesselSection
	{
		private string _name;
		private string _ectotal;
		private string _situation;
		private string _expcount;
		private bool _isvisible = true;
		private bool _cantransmit;
		private bool _isconnected;
		private bool _transmitavailable;
		private Guid _id;

		private double maxVesselEC;
		private double currentVesselEC;

		private Vessel vessel;
		private SEPScience.Unity.Unity.SEP_VesselSection vesselUISection;
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
			_id = v.id;

			experiments = SEP_Controller.Instance.getHandlers(v);

			_transmitavailable = SEP_Controller.Instance.TransmissionUpdgrade;
			_cantransmit = experiments.Any(e => e.controllerAutoTransmit);

			currentVesselEC = SEP_Utilities.getTotalVesselEC(v.protoVessel);
			maxVesselEC = SEP_Utilities.getMaxTotalVesselEC(v.protoVessel);

			_ectotal = getECString();

			_situation = getSituationString();

			experimentSections = new List<SEP_ExperimentSection>();
			addExperimentSections();

			_expcount = getExpCountString();

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

			if (vessel.loaded)
				currentVesselEC = SEP_Utilities.getTotalVesselEC(vessel);

			_ectotal = getECString();

			_isconnected = true;
		}

		private string getExpCountString()
		{
			return string.Format("{0} Experiments", experiments.Count);
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
			return string.Format("{0} - {1}", vessel.mainBody.bodyName, ScienceUtil.GetExperimentBiome(vessel.mainBody, vessel.latitude, vessel.longitude));
		}

		private bool getConnectionStatus()
		{
			return true;
		}

		public string Name
		{
			get { return _name; }
		}

		public Guid ID
		{
			get { return _id; }
		}

		public float Signal
		{
			get
			{
				if (vessel == null)
					return 0;

				if (vessel.Connection == null)
					return 0;

				return (float)vessel.Connection.SignalStrength;
			}
		}

		public Sprite SignalSprite
		{
			get
			{
				if (!SEP_Utilities.spritesLoaded)
					return null;

				if (vessel == null)
					return SEP_Utilities.CommNetSprites[0];

				if (vessel.Connection == null)
					return SEP_Utilities.CommNetSprites[0];

				if (SEP_Utilities.CommNetSprites.Length < 5)
					return null;

				switch(vessel.Connection.Signal)
				{
					case CommNet.SignalStrength.None:
						return SEP_Utilities.CommNetSprites[0];
					case CommNet.SignalStrength.Red:
						return SEP_Utilities.CommNetSprites[1];
					case CommNet.SignalStrength.Orange:
						return SEP_Utilities.CommNetSprites[2];
					case CommNet.SignalStrength.Yellow:
						return SEP_Utilities.CommNetSprites[3];
					case CommNet.SignalStrength.Green:
						return SEP_Utilities.CommNetSprites[4];
					default:
						return null;
				}
			}
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
			set
			{
				_cantransmit = value;

				if (!_transmitavailable)
					return;

				if (vessel.loaded)
				{
					var controllers = vessel.FindPartModulesImplementing<ModuleSEPStation>();

					for (int i = controllers.Count - 1; i >= 0; i--)
					{
						ModuleSEPStation station = controllers[i];

						if (station == null)
							continue;

						if (station.autoTransmit != value)
							station.toggleAutoTransmit();
					}
				}
				else
				{
					for (int i = experiments.Count - 1; i >= 0; i--)
					{
						SEP_ExperimentHandler handler = experiments[i];

						if (handler == null)
							continue;

						handler.controllerAutoTransmit = value;
					}

					for (int i = vessel.protoVessel.protoPartSnapshots.Count - 1; i >= 0; i--)
					{
						ProtoPartSnapshot part = vessel.protoVessel.protoPartSnapshots[i];

						if (part == null)
							continue;

						for (int j = part.modules.Count - 1; j >= 0; j--)
						{
							ProtoPartModuleSnapshot mod = part.modules[j];

							if (mod == null)
								continue;

							if (mod.moduleName != "ModuleSEPStation")
								continue;

							mod.moduleValues.SetValue("autoTransmit", value);
						}
					}
				}
			}
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

		public string ExpCount
		{
			get { return _expcount; }
		}

		public IList<IExperimentSection> GetExperiments()
		{
			return new List<IExperimentSection>(experimentSections.ToArray());
		}

		public void setParent(SEPScience.Unity.Unity.SEP_VesselSection section)
		{
			vesselUISection = section;
		}

		public void setAutoTransmit(bool isOn)
		{
			_cantransmit = isOn;

			if (_transmitavailable && vesselUISection != null)
				vesselUISection.setTransmissionSilent(isOn);
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

		public CelestialBody VesselBody
		{
			get
			{
				if (vessel == null)
					return null;

				return vessel.mainBody;
			}
		}

		public void AddExperiment(SEP_ExperimentHandler h)
		{
			if (h == null)
				return;

			if (h.vessel != vessel)
				return;

			SEP_ExperimentSection section = new SEP_ExperimentSection(h, h.vessel);

			if (section == null)
				return;
	
			experiments.Add(h);
			experimentSections.Add(section);

			if (vesselUISection == null)
				return;

			vesselUISection.AddExperimentSection(section);

			_expcount = getExpCountString();

			vesselUISection.setExpCount(_expcount);
		}

		public void RemoveExperiment(SEP_ExperimentHandler h)
		{
			if (h == null)
				return;

			for (int i = experiments.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentHandler handler = experiments[i];

				if (handler == null)
					continue;

				if (handler != h)
					continue;

				experiments.Remove(h);
				break;
			}

			for (int i = experimentSections.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentSection section = experimentSections[i];

				if (section == null)
					continue;

				if (section.Handler != h)
					continue;

				section.OnDestroy();
				experimentSections.Remove(section);
				section = null;

				break;
			}

			_expcount = getExpCountString();

			if (vesselUISection != null)
				vesselUISection.setExpCount(_expcount);
		}

		private void addExperimentSections()
		{
			for (int i = experiments.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentHandler handler = experiments[i];

				if (handler == null)
					return;

				experimentSections.Add(new SEP_ExperimentSection(handler, vessel));
			}
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

			if (vesselUISection != null)
				vesselUISection.setBiome(_situation);
		}
	}
}
