#region license
/*The MIT License (MIT)
ModuleSEPScienceExperiment - Part Module for SEP experiments

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
using System.Reflection;
using FinePrint.Utilities;
using UnityEngine;
using KSP.UI.Screens.Flight.Dialogs;

namespace SEPScience
{
    public class ModuleSEPScienceExperiment : PartModule, IScienceDataContainer
    {
		[KSPField]
		public string transmitWarningText;
		[KSPField]
		public string situationFailMessage = "Can't conduct experiment here";
		[KSPField]
		public string collectWarningText;
		[KSPField]
		public bool resettable;
		[KSPField]
		public bool excludeAtmosphere;
		[KSPField]
		public string excludeAtmosphereMessage = "This experiment can't be conducted within an atmosphere";
		[KSPField]
		public string includeAtmosphereMessage = "This experiment can only be conducted within an atmosphere";
		[KSPField]
		public float interactionRange = 1.5f;
		[KSPField(isPersistant = true)]
		public bool IsDeployed;
		[KSPField(isPersistant = true)]
		public bool Inoperable;
		[KSPField]
		public string experimentActionName = "Deploy Experiment";
		[KSPField]
		public string stopExperimentName = "Pause Experiment";
		[KSPField]
		public string collectActionName = "Collect Data";
		[KSPField]
		public string reviewActionName = "Review Data";
		[KSPField]
		public string requiredModules;
		[KSPField]
		public string requiredParts;
		[KSPField]
		public string requiredPartsMessage = "Required parts are not present on this vessel";
		[KSPField]
		public string requiredModulesMessage = "Required part modules are not present on this vessel";
		[KSPField]
		public string controllerModule = "ModuleSEPStation";
		[KSPField]
		public string controllerModuleMessage = "Controller module is not connected to the experiment";
		[KSPField]
		public string calibrationEventName = "Calibrate";
		[KSPField]
		public string retractEventName = "Shut Down";
		[KSPField]
		public bool animated;
		[KSPField]
		public string animationName;
		[KSPField]
		public float animSpeed = 1;
		[KSPField]
		public bool oneShotAnim;
		[KSPField]
		public int complexity = 1;

		//Persistent fields read by the handler module
		[KSPField(isPersistant = true)]
		public bool canTransmit = true;
		[KSPField(isPersistant = true)]
		public bool instantResults;
		[KSPField(isPersistant = true)]
		public bool experimentRunning;
		[KSPField(isPersistant = true)]
		public bool controllerCanTransmit;
		[KSPField(isPersistant = true)]
		public bool controllerAutoTransmit;
		[KSPField(isPersistant = true)]
		public string experimentID;
		[KSPField(isPersistant = true)]
		public float xmitDataScalar = 1;
		[KSPField(isPersistant = true)]
		public float calibration;
		[KSPField(isPersistant = true)]
		public float experimentTime = 50;
		[KSPField(isPersistant = true)]
		public float completion;
		[KSPField(isPersistant = true)]
		public float submittedData;
		[KSPField(isPersistant = true)]
		public double lastBackgroundCheck;
		[KSPField(isPersistant = true)]
		public int flightID;
		[KSPField(isPersistant = true)]
		public bool usingEC;

		//Right click menu status fields
		[KSPField(guiActive = true)]
		public string status = "Inactive";
		[KSPField]
		public string calibrationLevel = "0%";
		[KSPField]
		public string dataCollected = "0%";
		[KSPField]
		public string daysRemaining = "";

		private SEP_ExperimentHandler handler;
		private Animation anim;
		private string failMessage = "";
		private ExperimentsResultDialog results;
		private ModuleSEPStation controller;
		public List<ModuleResource> resources = new List<ModuleResource>();
		private UIPartActionWindow window;
		private bool powerIsProblem;
		private int powerTimer;

		public SEP_ExperimentHandler Handler
		{
			get { return handler; }
		}

		public ModuleSEPStation Controller
		{
			get { return controller; }
		}

		private List<string> requiredPartList = new List<string>();
		private List<string> requiredModuleList = new List<string>();

		public override void OnStart(PartModule.StartState state)
		{
			base.OnStart(state);

			flightID = (int)part.flightID;

			if (complexity > 4)
				complexity = 4;
			else if (complexity < 1)
				complexity = 1;

			if (state == StartState.Editor)
				return;

			if (animated && !string.IsNullOrEmpty(animationName))
				anim = part.FindModelAnimators(animationName)[0];

			if (IsDeployed && animated)
				animator(anim, animationName, 1, 1);

			setupEvents();

			requiredPartList = SEP_Utilities.parsePartStringList(requiredParts);
			requiredModuleList = SEP_Utilities.parseModuleStringList(requiredModules);

			GameEvents.onVesselSituationChange.Add(sitChange);
			SEP_Utilities.onWindowSpawn.Add(onWindowSpawn);
			SEP_Utilities.onWindowDestroy.Add(onWindowDestroy);
		}

		private void setupEvents()
		{
			Events["DeployExperiment"].guiName = experimentActionName;
			Events["DeployExperiment"].unfocusedRange = interactionRange;			
			Events["PauseExperiment"].guiName = stopExperimentName;
			Events["PauseExperiment"].unfocusedRange = interactionRange;
			Events["ReCalibrate"].active = IsDeployed;
			Events["ReCalibrate"].guiName = "Re-Calibrate";
			Events["ReCalibrate"].unfocusedRange = interactionRange;
			Events["CalibrateEvent"].active = !IsDeployed;
			Events["CalibrateEvent"].guiName = calibrationEventName;
			Events["CalibrateEvent"].unfocusedRange = interactionRange;
			Events["RetractEvent"].active = IsDeployed;
			Events["RetractEvent"].guiName = retractEventName;
			Events["RetractEvent"].unfocusedRange = interactionRange;
			Events["CollectData"].guiName = collectActionName;
			Events["CollectData"].unfocusedRange = interactionRange;
			Events["ReviewDataEvent"].guiName = reviewActionName;
			Events["ReviewDataEvent"].unfocusedRange = interactionRange;

			calibrationLevel = calibration.ToString("P0");
			dataCollected = completion.ToString("P0");

			Fields["status"].guiName = "Status";
			Fields["calibrationLevel"].guiName = "Calibration Level";
			Fields["calibrationLevel"].guiActive = IsDeployed;
			Fields["dataCollected"].guiActive = experimentRunning;
			Fields["dataCollected"].guiName = "Data Collected";
			Fields["daysRemaining"].guiName = "Time Remaining";
			Fields["daysRemaining"].guiActive = experimentRunning;
		}

		private void OnDestroy()
		{
			GameEvents.onVesselSituationChange.Remove(sitChange);
			SEP_Utilities.onWindowSpawn.Remove(onWindowSpawn);
			SEP_Utilities.onWindowDestroy.Remove(onWindowDestroy);
		}

		private void Update()
		{
			if (!FlightGlobals.ready)
				return;

			if (IsDeployed && controller != null)
			{
				if (vessel != controller.vessel)
					RetractEvent();
			}

			if (UIPartActionController.Instance == null)
				return;

			if (UIPartActionController.Instance.ItemListContains(part, false))
			{
				if (handler == null)
					return;

				status = statusString();

				if (experimentRunning)
				{
					dataCollected = handler.completion.ToString("P2");
					daysRemaining = getTimeRemaining();
					Fields["daysRemaining"].guiActive = true;
					Fields["dataCollected"].guiActive = true;
				}
				else
				{
					Fields["daysRemaining"].guiActive = false;
					Fields["dataCollected"].guiActive = false;
				}
			}
		}

		private void FixedUpdate()
		{
			if (handler == null)
				return;

			if (powerIsProblem)
			{
				if (powerTimer < 30)
				{
					powerTimer++;
					return;
				}

				//SEPUtilities.log("Re-deploying SEP Experiment after power out", logLevels.error);

				DeployExperiment();
			}

			if (!handler.experimentRunning)
				return;

			int l = resources.Count;

			for (int i = 0; i < l; i++)
			{
				ModuleResource resource = resources[i];
				resource.currentRequest = resource.rate * TimeWarp.fixedDeltaTime;
				resource.currentAmount = part.RequestResource(resource.id, resource.currentRequest);

				if (resource.currentAmount < resource.currentRequest * 0.8999)
				{
					//SEPUtilities.log("Not enough power for SEP Experiment [{0}]", logLevels.error, experimentID);
					PauseExperiment();
					Events["DeployExperiment"].active = false;
					Events["PauseExperiment"].active = true;
					experimentRunning = true;
					powerIsProblem = true;
					powerTimer = 0;
					break;
				}
				else
					powerIsProblem = false;
			}
		}

		public override string GetInfo()
		{
			string s = base.GetInfo();

			s += string.Format("<color={0}>{1}</color>\n", XKCDColors.HexFormat.Cyan, experimentActionName);
			s += string.Format("Can Transmit: {0}\n", RUIutils.GetYesNoUIString(canTransmit));

			if (canTransmit)
				s += string.Format("Transmission: {0:P0}\n", xmitDataScalar);

			if (excludeAtmosphere)
				s += string.Format("Exclude Atmosphere: {0}\n", RUIutils.GetYesNoUIString(true));

			s += string.Format("Experiment Complexity: {0}\n", complexity);

			s += string.Format("Std. Time To Completion: {0:N0} Days\n", getDays(experimentTime));

			if (resources.Count > 0)
				s += string.Format("{0}\n", PartModuleUtil.PrintResourceRequirements("Requires:", resources.ToArray()));

			if (animated && oneShotAnim)
				s += string.Format("One Shot: {0}\n", RUIutils.GetYesNoUIString(true));

			return s;
		}

		private float getDays(float time)
		{
			time *= 21600;

			time /= KSPUtil.dateTimeFormatter.Day;

			return time;
		}

		public override void OnLoad(ConfigNode node)
		{
			if (node.HasNode("RESOURCE"))
			{
				resources = new List<ModuleResource>();

				ConfigNode[] resourceNodes = node.GetNodes("RESOURCE");

				int r = resourceNodes.Length;

				for (int j = 0; j < r; j++)
				{
					ConfigNode resource = resourceNodes[j];
					ModuleResource mod = new ModuleResource();
					mod.Load(resource);
					resources.Add(mod);
				}
			}

			if (resources.Count > 0)
				usingEC = true;

			StartCoroutine(delayedLoad(node));
		}

		private IEnumerator delayedLoad(ConfigNode node)
		{
			//SEPUtilities.log("Delayed SEP Science Experiment Loading...", logLevels.warning);

			int timer = 0;

			while (!FlightGlobals.ready)
				yield return null;

			while (timer < 10)
			{
				timer++;
				yield return null;
			}

			if (SEP_Controller.Instance.VesselLoaded(vessel))
			{
				handler = SEP_Controller.Instance.getHandler(vessel, part.flightID);

				if (handler == null)
					handler = new SEP_ExperimentHandler(this, node);
				else
					handler.host = this;
			}
			else
				handler = new SEP_ExperimentHandler(this, node);

			if (handler == null)
				yield break;

			//SEPUtilities.log("SEP Science Experiment Loading...", logLevels.warning);

			delayedEvents();
		}

		private void delayedEvents()
		{
			Events["CollectData"].active = handler.GetScienceCount() > 0;
			Events["ReviewDataEvent"].active = handler.GetScienceCount() > 0;
			Events["DeployExperiment"].active = IsDeployed && !experimentRunning && handler.completion < handler.getMaxCompletion();
			Events["PauseExperiment"].active = IsDeployed && experimentRunning;
		}

		public override void OnSave(ConfigNode node)
		{
			if (handler == null)
				return;

			handler.OnSave(node);
		}

		private string statusString()
		{
			if (animated && anim != null && anim[animationName] != null && anim.IsPlaying(animationName))
				return "Moving...";

			if (handler == null)
				return "Error...";

			if (handler.GetScienceCount() >= 1)
				return "Data Ready";

			if (!IsDeployed)
				return "Inactive";

			if (!handler.experimentRunning)
				return "Calibrated";

			return "Collecting Data";
		}

		private string getTimeRemaining()
		{
			if (handler == null)
				return "Error...";

			float next = getNextCompletion(handler.completion);

			if (calibration <= 0)
				return "∞";

			float time = experimentTime / calibration;

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

			return string.Format("{0:N2} {1}", f, units);
		}

		private float getNextCompletion(float f)
		{
			if (f < 0.5f)
				return 0.5f;
			if (f < 0.75f)
				return 0.75f;
			return 1f;
		}

		private void onWindowSpawn(UIPartActionWindow win)
		{
			if (win == null)
				return;

			if (win.part.flightID != part.flightID)
				return;

			if (FlightGlobals.ActiveVessel == vessel)
				return;

			//SEPUtilities.log("Spawning UI Window", logLevels.log);

			window = win;
		}

		private void onWindowDestroy(UIPartActionWindow win)
		{
			if (win == null)
				return;

			if (win.part.flightID != part.flightID)
				return;

			if (window == null)
				return;

			//SEPUtilities.log("Destroying UI Window", logLevels.log);

			window = null;
		}

		private void LateUpdate()
		{
			if (window == null)
				return;

			if (!SEP_Utilities.UIWindowReflectionLoaded)
				return;

			if (FlightGlobals.ActiveVessel == vessel)
				return;

			int l = Fields.Count;

			for (int i = 0; i < l; i++)
			{
				BaseField b = Fields[i];

				if (!b.guiActive)
					continue;

				try
				{
					window.GetType().InvokeMember("AddFieldControl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreReturn | BindingFlags.InvokeMethod, null, window, new object[] { b, part, this });
				}
				catch (Exception e)
				{
					SEP_Utilities.log("Error in adding KSP Field to unfocused UI Part Action Window\n{0}", logLevels.error, e);
					continue;
				}

				try
				{
					var items = SEP_Utilities.UIActionListField(window).GetValue(window) as List<UIPartActionItem>;

					int c = items.Count;

					for (int j = 0; j < c; j++)
					{
						UIPartActionItem item = items[j];

						if (item is UIPartActionLabel)
							item.UpdateItem();
					}
				}
				catch (Exception e)
				{
					SEP_Utilities.log("Error in setting KSP Field on unfocused UI Part Action Window\n{0}", logLevels.error, e);
				}
			}
		}

		public void setController(ModuleSEPStation m)
		{
			controller = m;

			if (handler == null)
				return;

			handler.updateController(controller);
		}

		public ModuleSEPStation findController()
		{
			return vessel.FindPartModulesImplementing<ModuleSEPStation>().Where(c => c.connectedExperimentCount < c.maxExperiments).FirstOrDefault();
		}

		public void updateHandler(bool forward)
		{
			if (handler == null)
				return;

			handler.submittedData = submittedData;
			handler.experimentRunning = experimentRunning;

			if (forward)
			{
				handler.lastBackgroundCheck = lastBackgroundCheck;
				handler.calibration = calibration;
				handler.completion = completion;
			}
			else
			{
				lastBackgroundCheck = handler.lastBackgroundCheck;
				calibration = handler.calibration;
				completion = handler.completion;
			}
		}

		[KSPEvent(guiActive = false, externalToEVAOnly = true, guiActiveUnfocused = true, active = true)]
		public void CalibrateEvent()
		{
			if (FlightGlobals.ActiveVessel == null)
				return;

			if (!FlightGlobals.ActiveVessel.isEVA)
			{
				ScreenMessages.PostScreenMessage("Must be one EVA to activate this experiment", 5f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}

			ProtoCrewMember crew = FlightGlobals.ActiveVessel.GetVesselCrew().FirstOrDefault();

			if (crew == null)
				return;

			if (!canConduct(crew))
			{
				ScreenMessages.PostScreenMessage(failMessage, 5f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}

			calibration = calculateCalibration(crew.experienceTrait.CrewMemberExperienceLevel(), complexity);			

			IsDeployed = true;

			if (animated)
				animator(anim, animationName, animSpeed, 0);

			submittedData = handler.getSubmittedData();			

			setController(findController());

			SEP_Utilities.onExperimentActivate.Fire(vessel, handler);

			powerIsProblem = false;

			updateHandler(true);

			if (controller != null)
				controller.addConnectecExperiment(this);

			calibrationLevel = calibration.ToString("P0");
			
			Fields["calibrationLevel"].guiActive = true;
			Events["ReCalibrate"].active = true;
			Events["DeployExperiment"].active = true;
			Events["CalibrateEvent"].active = false;
			Events["RetractEvent"].active = !oneShotAnim;
		}

		[KSPEvent(guiActive = false, externalToEVAOnly = true, guiActiveUnfocused = true, active = false)]
		public void ReCalibrate()
		{
			if (!IsDeployed)
			{
				Events["ReCalibrate"].active = false;
				return;
			}

			if (FlightGlobals.ActiveVessel == null)
				return;

			if (!FlightGlobals.ActiveVessel.isEVA)
			{
				ScreenMessages.PostScreenMessage("Must be one EVA to activate this experiment", 5f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}

			ProtoCrewMember crew = FlightGlobals.ActiveVessel.GetVesselCrew().FirstOrDefault();

			if (crew == null)
				return;

			if (!canConduct(crew))
			{
				ScreenMessages.PostScreenMessage(failMessage, 5f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}

			calibration = calculateCalibration(crew.experienceTrait.CrewMemberExperienceLevel(), complexity);

			if (handler != null)
				handler.calibration = calibration;

			calibrationLevel = calibration.ToString("P0");
		}

		[KSPEvent(guiActive = false, externalToEVAOnly = true, guiActiveUnfocused = true, active = false)]
		public void RetractEvent()
		{
			IsDeployed = false;

			if (animated)
				animator(anim, animationName, -1 * animSpeed, 1);

			controller.removeConnectedExperiment(this);

			powerIsProblem = false;

			setController(null);

			calibration = 0;
			experimentRunning = false;
			submittedData = 0;

			updateHandler(false);

			SEP_Utilities.onExperimentDeactivate.Fire(handler.vessel, handler);

			lastBackgroundCheck = 0;
			completion = 0;

			SEP_Controller.Instance.updateVessel(vessel);

			Fields["calibrationLevel"].guiActive = false;
			Events["ReCalibrate"].active = false;
			Events["DeployExperiment"].active = false;
			Events["PauseExperiment"].active = false;
			Events["CalibrateEvent"].active = true;
			Events["RetractEvent"].active = false;
		}

		private void animator(Animation a, string name, float speed, float time)
		{
			if (!animated)
				return;

			if (a == null)
				return;

			if (a[name] == null)
				return;

			a[name].speed = speed;

			if (!a.IsPlaying(name))
			{
				a[name].normalizedTime = time;
				a.Blend(name);
			}
		}

		private float calculateCalibration(int l, int c)
		{
			float f = 1;

			int i = l - c;

			float mod = i * 0.25f;

			f += mod;

			if (f >= 2f)
				f = 2f;
			else if (f < 0.25f)
				f = 0.25f;

			return f;
		}

		[KSPEvent(guiActive = false, guiActiveUnfocused = true, externalToEVAOnly = true, active = false)]
		public void ReviewDataEvent()
		{
			ReviewData();

			Events["CollectData"].active = false;
			Events["ReviewDataEvent"].active = false;
		}

		[KSPEvent(guiActive = false, guiActiveUnfocused = true, externalToEVAOnly = true, active = false)]
		public void CollectData()
		{
			transferToEVA();

			Events["CollectData"].active = false;
			Events["ReviewDataEvent"].active = false;
		}

		[KSPEvent(guiActive = false, guiActiveUnfocused = true, externalToEVAOnly = true, active = false)]
		public void DeployExperiment()
		{
			if (!IsDeployed)
				return;

			if (!vessel.Landed)
				return;

			if (controller != null && !controller.IsDeployed)
				controller.DeployEvent();

			if (instantResults)
				gatherScience();
			else
			{
				experimentRunning = true;

				lastBackgroundCheck = Planetarium.GetUniversalTime();

				submittedData = handler.getSubmittedData();

				updateHandler(true);

				SEP_Controller.Instance.updateVessel(vessel);

				Fields["dataCollected"].guiActive = true;
				Fields["daysRemaining"].guiActive = true;
				Events["DeployExperiment"].active = false;
				Events["PauseExperiment"].active = true;
			}
		}

		[KSPEvent(guiActive = false, guiActiveUnfocused = true, externalToEVAOnly = true, active = false)]
		public void PauseExperiment()
		{
			experimentRunning = false;

			powerIsProblem = false;
			powerTimer = 0;

			submittedData = 0;

			updateHandler(false);

			SEP_Controller.Instance.updateVessel(vessel);

			Fields["dataCollected"].guiActive = false;
			Fields["daysRemaining"].guiActive = false;
			Events["DeployExperiment"].active = true;
			Events["PauseExperiment"].active = false;
		}

		private void sitChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> FT)
		{
			if (FT.host == null)
				return;

			if (FT.host != vessel)
				return;

			if (FT.to == Vessel.Situations.LANDED)
				return;

			completion = 0;
			calibration = 0;

			if (handler != null)
			{
				handler.completion = 0;
				handler.calibration = 0;
			}

			if (!experimentRunning)
				return;

			PauseExperiment();
		}

		public void gatherScience(bool silent = false)
		{
			int level = handler.getMaxLevel(instantResults);

			if (level <= 0)
				return;

			ScienceData data = SEP_Utilities.getScienceData(handler, handler.getExperimentLevel(instantResults), level);

			if (data == null)
			{
				SEP_Utilities.log("Null Science Data returned; something went wrong here...", logLevels.warning);
				return;
			}

			GameEvents.OnExperimentDeployed.Fire(data);

			if (handler == null)
				return;

			handler.GetData().Add(data);

			if (silent)
				transferToEVA();
			else
				ReviewData();
		}

		private void transferToEVA()
		{
			if (handler == null)
				return;

			if (!FlightGlobals.ActiveVessel.isEVA)
			{
				handler.clearData();
				Inoperable = false;
				return;
			}

			List<ModuleScienceContainer> EVACont = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceContainer>();

			if (EVACont.Count <= 0)
				return;

			if (handler.GetScienceCount() > 0)
			{
				if (!EVACont.First().StoreData(new List<IScienceDataContainer> { this }, false))
				{
					handler.clearData();
					Inoperable = false;
				}
			}

			if (controller != null)
				controller.setCollectEvent();
		}

		private bool canConduct(ProtoCrewMember c)
		{
			failMessage = "";

			ExperimentSituations sit = ScienceUtil.GetExperimentSituation(vessel);

			if (handler == null)
			{
				SEP_Utilities.log("SEP Experiment Handler is null; Stopping any experiments...", logLevels.warning);
				failMessage = "Whoops, something went wrong with the SEP Experiment";
				return false;
			}

			if (Inoperable)
			{
				failMessage = "Experiment is no longer functional";
				return false;
			}
			else if (!handler.basicExperiment.IsAvailableWhile(sit, vessel.mainBody))
			{
				if (!string.IsNullOrEmpty(situationFailMessage))
					failMessage = situationFailMessage;
				return false;
			}
			else if (excludeAtmosphere && vessel.mainBody.atmosphere)
			{
				if (!string.IsNullOrEmpty(excludeAtmosphereMessage))
					failMessage = excludeAtmosphereMessage;
				return false;
			}
			else if (handler.basicExperiment.requireAtmosphere && !vessel.mainBody.atmosphere)
			{
				failMessage = includeAtmosphereMessage;
				return false;
			}
			else if (!string.IsNullOrEmpty(controllerModule))
			{
				if (!VesselUtilities.VesselHasModuleName(controllerModule, vessel))
				{
					failMessage = controllerModuleMessage;
					return false;
				}
			}

			if (requiredPartList.Count > 0)
			{
				for (int i = 0; i < requiredPartList.Count; i++)
				{
					string partName = requiredPartList[i];

					if (string.IsNullOrEmpty(partName))
						continue;

					if (!VesselUtilities.VesselHasPartName(partName, vessel))
					{
						failMessage = requiredPartsMessage;
						return false;
					}
				}
			}

			if (requiredModuleList.Count > 0)
			{
				for (int i = 0; i < requiredModuleList.Count; i++)
				{
					string moduleName = requiredModuleList[i];

					if (string.IsNullOrEmpty(moduleName))
						continue;

					if (!VesselUtilities.VesselHasModuleName(moduleName, vessel))
					{
						failMessage = requiredModulesMessage;
						return false;
					}
				}
			}

			if (c.experienceTrait.TypeName != "Scientist")
			{
				failMessage = "Kerbal must be scientist";
				return false;
			}

			return true;
		}

		#region Experiment Results Page

		private void onKeepData(ScienceData data)
		{
			transferToEVA();
		}

		private void onTransmitData(ScienceData data)
		{
			if (handler == null)
				return;

			List<IScienceDataTransmitter> tranList = vessel.FindPartModulesImplementing<IScienceDataTransmitter>();
			if (tranList.Count > 0 && handler.GetScienceCount() > 0)
			{
				tranList.OrderBy(ScienceUtil.GetTransmitterScore).First().TransmitData(new List<ScienceData> { data });
				DumpData(data);
			}
			else
				ScreenMessages.PostScreenMessage("No Comms Devices on this vessel. Cannot Transmit Data.", 3f, ScreenMessageStyle.UPPER_CENTER);
		}

		private void onSendToLab(ScienceData data)
		{
			ScienceLabSearch labSearch = new ScienceLabSearch(vessel, data);

			if (labSearch.NextLabForDataFound)
			{
				StartCoroutine(labSearch.NextLabForData.ProcessData(data, null));
				DumpData(data);
			}
			else
				labSearch.PostErrorToScreen();
		}

		private void onDiscardData(ScienceData data)
		{
			if (handler == null)
				return;

			if (handler.GetData().Contains(data))
				handler.removeData(data);
		}

		#endregion

		#region IScienceDataContainer methods

		public void ReviewData()
		{
			if (handler == null)
				return;

			if (handler.GetScienceCount() <= 0)
				return;

			ScienceData data = handler.GetData()[0];

			results =  ExperimentsResultDialog.DisplayResult(new ExperimentResultDialogPage(part, data, data.transmitValue, ModuleScienceLab.GetBoostForVesselData(vessel, data), false, transmitWarningText, true, new ScienceLabSearch(vessel, data), new Callback<ScienceData>(onDiscardData), new Callback<ScienceData>(onKeepData), new Callback<ScienceData>(onTransmitData), new Callback<ScienceData>(onSendToLab)));
		}

		public void ReviewDataItem(ScienceData data)
		{
			ReviewData();
		}

		public void ReturnData(ScienceData data)
		{
			if (data == null)
				return;

			if (handler == null)
				return;

			handler.GetData().Add(data);

			Events["CollectData"].active = true;
			Events["ReviewDataEvent"].active = true;

			if (controller != null)
				controller.setCollectEvent();
		}

		public bool IsRerunnable()
		{
			return true;
		}

		public int GetScienceCount()
		{
			if (handler == null)
				return 0;

			return handler.GetScienceCount();
		}

		public ScienceData[] GetData()
		{
			if (handler == null)
				return new ScienceData[0];

			return handler.GetData().ToArray();
		}

		public void DumpData(ScienceData data)
		{
			if (handler == null)
				return;

			if (handler.GetData().Contains(data))
				handler.removeData(data);

			Events["CollectData"].active = false;
			Events["ReviewDataEvent"].active = false;

			if (controller != null)
				controller.setCollectEvent();
		}

		#endregion

	}
}
