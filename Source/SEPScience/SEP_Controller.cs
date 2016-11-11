#region license
/*The MIT License (MIT)
SEPController - Monobehaviour for handling all data gathering

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KSP.UI.Screens.Flight;
using FinePrint.Utilities;
using UnityEngine;
using SEPScience.SEP_UI.Windows;

namespace SEPScience
{
	[KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
	public class SEP_Controller : MonoBehaviour
	{
		private static bool running;
		private static SEP_Controller instance;

		private const string transmissionNode = "advElectrics";
		private double lastUpdate;
		private double updateRate = 3950;
		private bool setup;
		private bool transmissionUpgrade;
		private bool usingCommNet = true;
		private DictionaryValueList<Vessel, List<SEP_ExperimentHandler>> experiments = new DictionaryValueList<Vessel, List<SEP_ExperimentHandler>>();

		public static SEP_Controller Instance
		{
			get { return instance; }
		}

		public bool Setup
		{
			get { return setup; }
		}

		public bool TransmissionUpdgrade
		{
			get { return transmissionUpgrade; }
		}

		public bool UsingCommNet
		{
			get { return usingCommNet; }
		}

		private void Start()
		{
			if (HighLogic.LoadedSceneIsEditor)
			{
				running = false;
				Destroy(gameObject);
			}

			if (!SEP_Utilities.partModulesLoaded)
				SEP_Utilities.loadPartModules();

			if (!SEP_Utilities.antennaModulesLoaded)
				SEP_Utilities.loadAntennaParts();

			if (!SEP_Utilities.spritesLoaded)
				StartCoroutine(loadSprites());

			if (ResearchAndDevelopment.GetTechnologyState(transmissionNode) == RDTech.State.Available)
				transmissionUpgrade = true;
			else
				transmissionUpgrade = false;

			if (running)
				Destroy(gameObject);

			if (HighLogic.LoadedSceneIsFlight)
				StartCoroutine(attachWindowListener());

			instance = this;

			running = true;

			usingCommNet = HighLogic.CurrentGame.Parameters.Difficulty.EnableCommNet;

			GameEvents.onLevelWasLoaded.Add(onReady);
			GameEvents.OnGameSettingsApplied.Add(onSettingsApplied);
		}

		private void OnDestroy()
		{
			running = false;

			GameEvents.onLevelWasLoaded.Remove(onReady);
			GameEvents.OnGameSettingsApplied.Remove(onSettingsApplied);

			var handlers = experiments.Values.SelectMany(e => e).ToList();

			int l = handlers.Count;

			for (int i = 0; i < l; i++)
			{
				SEP_ExperimentHandler handler = handlers[i];

				handler.OnDestroy();
			}
		}

		private void Update()
		{
			if (!setup)
				return;

			double timeWarpMultiplier = 1;

			if (TimeWarp.CurrentRate >= 100)
				timeWarpMultiplier = 0.1;
			else if (TimeWarp.CurrentRate >= 1000)
				timeWarpMultiplier = 0.5;
			else if (TimeWarp.CurrentRate >= 10000)
				timeWarpMultiplier = 2.5;
			else if (TimeWarp.CurrentRate >= 100000)
				timeWarpMultiplier = 25;
			else if (TimeWarp.CurrentRate >= 1000000)
				timeWarpMultiplier = 100;

			double nextUpdate = updateRate * timeWarpMultiplier;

			if (Planetarium.GetUniversalTime() - lastUpdate > nextUpdate)
			{
				lastUpdate = Planetarium.GetUniversalTime();

				experimentCheck(lastUpdate);
			}
		}

		private void onSettingsApplied()
		{
			usingCommNet = HighLogic.CurrentGame.Parameters.Difficulty.EnableCommNet;
		}

		private void onReady(GameScenes scene)
		{
			if (scene == GameScenes.FLIGHT || scene == GameScenes.TRACKSTATION || scene == GameScenes.SPACECENTER)
				StartCoroutine(startup());
		}

		private IEnumerator loadSprites()
		{
			while (TelemetryUpdate.Instance == null)
				yield return null;

			SEP_Utilities.loadSprites(TelemetryUpdate.Instance.SS0, TelemetryUpdate.Instance.SS1, TelemetryUpdate.Instance.SS2, TelemetryUpdate.Instance.SS3, TelemetryUpdate.Instance.SS4);
		}

		private IEnumerator attachWindowListener()
		{
			while (UIPartActionController.Instance == null)
				yield return null;

			while (UIPartActionController.Instance.windowPrefab == null)
				yield return null;

			SEP_Utilities.attachWindowPrefab();
		}

		private IEnumerator startup()
		{
			//SEPUtilities.log("Delayed SEP Controller Starting...", logLevels.warning);

			int timer = 0;

			while (timer < 30)
			{
				timer++;
				yield return null;
			}

			//SEPUtilities.log("SEP Controller Starting...", logLevels.warning);

			populateExperiments();
		}

		private void populateExperiments()
		{
			experiments = new DictionaryValueList<Vessel, List<SEP_ExperimentHandler>>();

			int l = FlightGlobals.Vessels.Count;

			for (int i = 0; i < l; i++)
			{
				Vessel v = FlightGlobals.Vessels[i];

				if (v == null)
					continue;

				if (v.vesselType == VesselType.Debris)
					continue;

				if (experiments.Contains(v))
					continue;

				List<SEP_ExperimentHandler> handlers = new List<SEP_ExperimentHandler>();

				if (v.loaded)
				{
					handlers =
						(from mod in v.FindPartModulesImplementing<ModuleSEPScienceExperiment>()
						where mod.Handler != null && mod.IsDeployed
						select mod.Handler).ToList();
				}
				else
				{
					var snaps = v.protoVessel.protoPartSnapshots;

					int s = snaps.Count;

					for (int j = 0; j < s; j++)
					{
						ProtoPartSnapshot p = snaps[j];

						if (!p.modules.Any(m => m.moduleName == "ModuleSEPScienceExperiment"))
							continue;

						var mods = p.modules;

						int d = mods.Count;

						for (int k = 0; k < d; k++)
						{
							ProtoPartModuleSnapshot mod = mods[k];

							if (mod.moduleName != "ModuleSEPScienceExperiment")
								continue;

							SEP_ExperimentHandler handler = new SEP_ExperimentHandler(mod, v);

							if (handler.loaded)
								handlers.Add(handler);
						}
					}
				}

				if (handlers.Count > 0)
				{
					if (VesselUtilities.VesselHasModuleName("ModuleSEPStation", v))
						experiments.Add(v, handlers);
				}
			}

			setup = true;
		}

		public void updateVessel(Vessel v)
		{
			List<SEP_ExperimentHandler> modules =
				(from mod in v.FindPartModulesImplementing<ModuleSEPScienceExperiment>()
				 where mod.Handler != null
				 select mod.Handler).ToList();

			if (experiments.Contains(v))
			{
				if (modules.Count > 0)
					experiments[v] = modules;
				else
				{
					experiments.Remove(v);
					SEP_AppLauncher.Instance.removeVesselSection(v);
				}
			}
			else if (modules.Count > 0)
			{
				if (VesselUtilities.VesselHasModuleName("ModuleSEPStation", v))
				{
					experiments.Add(v, modules);
					SEP_AppLauncher.Instance.addVesselSection(v);
				}
			}
		}

		public List<Vessel> GetAllVessels()
		{
			return experiments.Keys.ToList();
		}

		public bool VesselLoaded(Vessel v)
		{
			return experiments.Contains(v);
		}

		public SEP_ExperimentHandler getHandler(Vessel v, uint id)
		{
			if (!experiments.Contains(v))
				return null;

			List<SEP_ExperimentHandler> handlers = experiments[v];

			int l = handlers.Count;

			for (int i = 0; i < l; i++)
			{
				SEP_ExperimentHandler handler = handlers[i];

				if (handler == null)
					continue;

				if (!handler.loaded)
					continue;

				if (handler.flightID == id)
					return handler;
			}

			return null;
		}

		public List<SEP_ExperimentHandler> getHandlers(Vessel v)
		{
			if (!experiments.Contains(v))
				return new List<SEP_ExperimentHandler>();

			return experiments[v];
		}

		private void experimentCheck(double time)
		{
			int l = experiments.Count;

			//if (l > 0)
				//SEP_Utilities.log("Performing SEP background check on {0} experiments at time: {1:N0}", logLevels.log, l , time);

			for (int i = 0; i < l; i++)
			{
				List<SEP_ExperimentHandler> modules = experiments.At(i);

				int c = modules.Count;

				for (int j = 0; j < c; j++)
				{
					SEP_ExperimentHandler m = modules[j];

					if (m == null)
						continue;

					if (!m.loaded)
						continue;

					if (!m.experimentRunning)
						continue;

					if (m.usingECResource && !m.vessel.loaded)
					{
						if (!m.vessel.HasValidContractObjectives(new List<string> { "Generator" }))
							continue;
					}

					if (m.vessel.loaded)
					{
						if (m.host != null && m.host.Controller != null)
						{
							if (m.host.vessel != m.host.Controller.vessel)
								continue;
						}
						else
							continue;
					}

					double t = m.experimentTime;

					float calib = m.calibration;

					if (usingCommNet)
					{
						if (m.vessel.Connection != null)
						{
							float signal = (float)m.vessel.Connection.SignalStrength - 0.5f;

							if (signal < 0)
								signal /= 2;

							float bonus = calib * signal;

							calib += bonus;
						}
					}

					t /= calib;

					double length = time - m.lastBackgroundCheck;

					m.lastBackgroundCheck = time;

					double days = length / 21600;

					if (days < t)
					{
						double n = days / t;

						//SEPUtilities.log("Updating SEP Experiment Handler [{0}]: Add {1:P6}", logLevels.warning, m.experimentTitle, n);

						m.completion += (float)n;
					}
					else
						m.completion = 1;

					float max = m.getMaxCompletion();

					if (max <= 0.5f)
					{
						if (m.completion >= 0.5f)
							m.completion = 0.5f;
					}
					else if (max <= 0.75f)
					{
						if (m.completion >= 0.75f)
							m.completion = 0.75f;
					}
					else
					{
						if (m.completion >= 1f)
							m.completion = 1f;
					}

					bool transmitted = false;

					m.submittedData = m.getSubmittedData();

					if (m.canTransmit && m.controllerAutoTransmit && transmissionUpgrade)
						transmitted = checkTransmission(m);
					else
					{
						int level = m.getMaxLevel(false);

						if (!dataOnboard(m, level))
						{
							m.addData(SEP_Utilities.getScienceData(m, m.getExperimentLevel(level), level));

							if (m.vessel.loaded && m.host != null)
							{
								m.host.Events["ReviewDataEvent"].active = true;
								m.host.Events["TransferDataEvent"].active = m.host.hasContainer;
								m.host.Events["CollectData"].active = true;
								if (m.host.Controller != null)
									m.host.Controller.setCollectEvent();
							}
						}
					}

					if (transmitted)
						m.clearData();

					if (m.completion >= max)
					{
						m.experimentRunning = false;
						if (m.vessel.loaded && m.host != null)
						{
							int count = m.GetScienceCount();
							m.host.Events["ReviewDataEvent"].active = !transmitted && count > 0;
							m.host.Events["TransferDataEvent"].active = !transmitted && count > 0 && m.host.hasContainer;
							m.host.Events["CollectData"].active = !transmitted && count > 0;
							m.host.PauseExperiment();
							if (m.host.Controller != null)
								m.host.Controller.setCollectEvent();
						}
					}
				}
			}
		}

		private bool dataOnboard(SEP_ExperimentHandler handler, int level)
		{
			float science = handler.currentMaxScience(level);

			if (science > handler.submittedData)
			{
				if (handler.GetScienceCount() > 0)
				{
					ScienceData dat = handler.GetData()[0];

					ScienceSubject sub = ResearchAndDevelopment.GetSubjectByID(dat.subjectID);

					if (sub != null)
					{
						float d = dat.dataAmount / sub.dataScale;

						//SEP_Utilities.log("Science Data value check: {0:N2} - {1:N2}", logLevels.warning, science, d);

						if (science <= d)
							return true;
					}
					else
						return true;
				}

				return false;
			}

			return true;
		}

		private bool checkTransmission(SEP_ExperimentHandler exp)
		{
			int level = exp.getMaxLevel(false);
			float science = exp.currentMaxScience(level);

			if (science > exp.submittedData)
				return transmitData(exp, level, exp.submittedData, science);

			return false;
		}

		private bool transmitData(SEP_ExperimentHandler exp, int level, float submittedData, float newData)
		{
			ScienceData data = SEP_Utilities.getScienceData(exp, exp.getExperimentLevel(level), level);

			if (exp.vessel.loaded)
			{
				IScienceDataTransmitter bestTransmitter = ScienceUtil.GetBestTransmitter(exp.vessel);
				
				if (bestTransmitter != null)
				{
					ScienceExperiment e = exp.getExperimentLevel(level);

					float transmitterCost = getLoadedTransmitterCost(bestTransmitter);

					float ecCost = newData * e.dataScale * transmitterCost;

					if (ecCost > SEP_Utilities.getTotalVesselEC(exp.vessel))
					{
						if (!dataOnboard(exp, level))
						{
							SEP_Utilities.log("Not enough power for transmissionon this vessel: {1}; saving data: {0}", logLevels.log, data.title, exp.vessel.vesselName);
							exp.addData(data);
							if (exp.host != null)
							{
								exp.host.Events["ReviewDataEvent"].active = true;
								exp.host.Events["TransferDataEvent"].active = exp.host.hasContainer;
								exp.host.Events["CollectData"].active = true;
								if (exp.host.Controller != null)
									exp.host.Controller.setCollectEvent();
							}
						}
						return false;
					}

					SEP_Utilities.log("Sending data to vessel comms: {0}", logLevels.log, data.title);
					bestTransmitter.TransmitData(new List<ScienceData> { data });
					return true;
				}
				else
				{
					if (!dataOnboard(exp, level))
					{
						SEP_Utilities.log("No Comms Devices on this vessel: {1}. Cannot Transmit Data; saving data: {0}", logLevels.log, data.title, exp.vessel.vesselName);
						exp.addData(data);
						if (exp.host != null)
						{
							exp.host.Events["ReviewDataEvent"].active = true;
							exp.host.Events["TransferDataEvent"].active = exp.host.hasContainer;
							exp.host.Events["CollectData"].active = true;
							if (exp.host.Controller != null)
								exp.host.Controller.setCollectEvent();
						}
					}
					return false;
				}
			}
			else
			{
				List<ProtoPartSnapshot> transmitters = getProtoTransmitters(exp.vessel.protoVessel);

				float? transmitterCost = getBestTransmitterCost(transmitters);

				if (transmitterCost == null)
				{
					if (!dataOnboard(exp, level))
					{
						SEP_Utilities.log("No Comms Devices on this vessel: {1}. Cannot Transmit Data; saving data: {0}", logLevels.log, data.title, exp.vessel.protoVessel.vesselName);
						exp.addData(data);
					}
					return false;
				}

				//SEP_Utilities.log("Transmission Score: {0:N4}EC", logLevels.warning, transmitterCost);

				ScienceExperiment e = exp.getExperimentLevel(level);

				float ecCost = newData * e.dataScale * (float)transmitterCost;

				//SEP_Utilities.log("Transmission Cost: {0:N4}EC", logLevels.warning, ecCost);

				if (ecCost > SEP_Utilities.getTotalVesselEC(exp.vessel.protoVessel))
				{
					if (!dataOnboard(exp, level))
					{
						SEP_Utilities.log("Not enough electricity on this vessel: {1}. Cannot Transmit Data; saving data: {0}", logLevels.log, data.title, exp.vessel.protoVessel.vesselName);
						exp.addData(data);
					}
					return false;
				}

				ScienceSubject sub = SEP_Utilities.checkAndUpdateRelatedSubjects(exp, level, newData, submittedData);

				if (sub == null)
					return false;

				ResearchAndDevelopment.Instance.SubmitScienceData(newData * sub.dataScale, sub, 1, exp.vessel.protoVessel);

				if (exp.vessel.HasValidContractObjectives(new List<string> { "Generator" }))
					consumeResources(exp.vessel.protoVessel, ecCost);
				
				exp.submittedData += (newData - submittedData);

				return true;
			}
		}

		private List<ProtoPartSnapshot> getProtoTransmitters(ProtoVessel v)
		{
			List<ProtoPartSnapshot> snaps = new List<ProtoPartSnapshot>();
			
			for (int i = v.protoPartSnapshots.Count - 1; i >= 0; i--)
			{
				ProtoPartSnapshot p = v.protoPartSnapshots[i];

				if (p == null)
					continue;

				for (int j = SEP_Utilities.AntennaParts.Count - 1; j >= 0; j--)
				{
					AvailablePart a = SEP_Utilities.AntennaParts.At(j);

					if (a == null)
						continue;

					if (p.partName != a.name)
						continue;

					snaps.Add(p);
					break;
				}
			}

			return snaps;
		}

		private float? getBestTransmitterCost(List<ProtoPartSnapshot> parts)
		{
			List<ConfigNode> transmitterNodes = new List<ConfigNode>();

			for (int i = parts.Count - 1; i >= 0; i--)
			{
				ProtoPartSnapshot snap = parts[i];

				if (snap == null)
					continue;

				AvailablePart info = snap.partInfo;

				if (info == null)
					continue;

				ConfigNode node = info.partConfig;

				ConfigNode[] moduleNodes = node.GetNodes("MODULE");

				for (int j = moduleNodes.Length - 1; j >= 0; j--)
				{
					ConfigNode nod = moduleNodes[j];

					if(!nod.HasValue("name"))
						continue;

					string name = nod.GetValue("name");

					bool b = false;

					for (int k = SEP_Utilities.AntennaModules.Count - 1; k >= 0; k--)
					{
						PartModule mod = SEP_Utilities.AntennaModules[k];

						if (mod == null)
							continue;

						string s = mod.moduleName;

						if (s != name)
							continue;
					
						transmitterNodes.Add(nod);
						b = true;
						break;
					}

					if (b)
						break;
				}
			}

			if (transmitterNodes.Count <= 0)
				return null;

			float cost = getTransmitterCost(transmitterNodes);

			return cost;
		}

		private float getLoadedTransmitterCost(IScienceDataTransmitter transmitter)
		{
			double d = 1000000;

			PartModule pMod = transmitter as PartModule;

			if (pMod != null)
			{
				switch(pMod.moduleName)
				{
					case "ModuleDataTransmitter":
						ModuleDataTransmitter tran = pMod as ModuleDataTransmitter;

						if (tran != null)
							d = tran.packetResourceCost / tran.packetSize;
						break;
					case "ModuleRTDataTransmitter":
						float cost = pMod.Fields["PacketResourceCost"].GetValue<float>(pMod);
						float size = pMod.Fields["PacketSize"].GetValue<float>(pMod);
						d = cost / size;
						break;
					default:
						break;
				}
			}

			return (float)d;
		}

		private float getTransmitterCost(List<ConfigNode> nodes)
		{
			float f = 1000000;

			int l = nodes.Count;

			for (int i = 0; i < l; i++)
			{
				ConfigNode node = nodes[i];

				string name = node.GetValue("name");

				float packetCost = 0;
				float packetSize = 0;
				float packetRate = 0;

				switch(name)
				{
					case "ModuleDataTransmitter":
					case "ModuleRTDataTransmitter":
						node.TryGetValue("packetResourceCost", ref packetCost);
						node.TryGetValue("packetSize", ref packetSize);
						packetRate = packetCost / packetSize;
						break;
					case "ModuleRTAntenna":
					case "ModuleRTAntennaPassive":
						node.TryGetValue("RTPacketResourceCost", ref packetCost);
						node.TryGetValue("RTPacketSize", ref packetSize);
						packetRate = packetCost / packetSize;
						break;
					case "ModuleLimitedDataTransmitter":
						node.TryGetValue("packetResourceCost", ref packetCost);
						packetRate = packetCost;
						break;
					default:
						break;
				}

				if (packetRate < f)
					f = packetRate;
			}

			return f;
		}

		private void consumeResources(ProtoVessel v, float amount)
		{
			int l = v.protoPartSnapshots.Count;

			for (int i = 0; i < l; i++)
			{
				ProtoPartSnapshot part = v.protoPartSnapshots[i];

				if (part == null)
					continue;

				int r = part.resources.Count;

				for (int j = 0; j < r; j++)
				{
					ProtoPartResourceSnapshot resource = part.resources[j];

					if (resource == null)
						continue;

					if (resource.resourceName != "ElectricCharge")
						continue;

					double partialEC = amount;

					double rAmount = resource.amount;

					if (partialEC >= rAmount)
						partialEC = rAmount;

					rAmount -= partialEC;

					resource.amount = rAmount;

					amount -= (float)partialEC;

					if (amount <= 0)
						return;
				}
			}
		}

		
	}


}
