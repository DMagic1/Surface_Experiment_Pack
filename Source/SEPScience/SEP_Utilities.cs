#region license
/*The MIT License (MIT)
SEPUtilities - Utilities class

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
using System.Reflection;
using UnityEngine;

namespace SEPScience
{
	public static class SEP_Utilities
	{
		public static List<Type> loadedPartModules = new List<Type>();
		public static bool partModulesLoaded = false;

		public static EventData<UIPartActionWindow> onWindowSpawn = new EventData<UIPartActionWindow>("onWindowSpawn");
		public static EventData<UIPartActionWindow> onWindowDestroy = new EventData<UIPartActionWindow>("onWindowDestroy");
		public static EventData<Vessel, SEP_ExperimentHandler> onExperimentActivate = new EventData<Vessel, SEP_ExperimentHandler>("onExperimentActivate");
		public static EventData<Vessel, SEP_ExperimentHandler> onExperimentDeactivate = new EventData<Vessel, SEP_ExperimentHandler>("onExperimentDeactivate");

		public static List<PartModule> AntennaModules = new List<PartModule>();
		public static DictionaryValueList<string, AvailablePart> AntennaParts = new DictionaryValueList<string, AvailablePart>();
		public static bool antennaModulesLoaded = false;
		
		public static Sprite[] CommNetSprites;
		public static bool spritesLoaded = false;

		public static void log(string message, logLevels l, params object[] objs)
		{
			message = string.Format(message, objs);
			string log = string.Format("[SEP Science] {0}", message);
			switch (l)
			{
				case logLevels.log:
					Debug.Log(log);
					break;
				case logLevels.warning:
					Debug.LogWarning(log);
					break;
				case logLevels.error:
					Debug.LogError(log);
					break;
			}
		}

		public static void loadPartModules()
		{
			partModulesLoaded = true;

			try
			{
				loadedPartModules = AssemblyLoader.loadedAssemblies.Where(a => a.types.ContainsKey(typeof(PartModule))).SelectMany(b => b.types[typeof(PartModule)]).ToList();
			}
			catch (Exception e)
			{
				log("Failure Loading Part Module List:\n", logLevels.error, e);
				loadedPartModules = new List<Type>();
			}
		}

		public static void loadAntennaParts()
		{
			antennaModulesLoaded = true;

			for (int i = PartLoader.LoadedPartsList.Count - 1; i >= 0; i--)
			{
				AvailablePart part = PartLoader.LoadedPartsList[i];

				if (part == null)
					continue;

				if (part.partPrefab == null)
					continue;

				for (int j = part.partPrefab.Modules.Count - 1; j >= 0; j--)
				{
					PartModule mod = part.partPrefab.Modules[j];

					if (mod == null)
						continue;

					if (!mod.IsValidContractObjective("Antenna"))
						continue;

					//log("Antenna Part Found: {0} - Module: {1}", logLevels.log, part.title, mod.moduleName);

					AntennaModules.Add(mod);

					if (!AntennaParts.Contains(part.name))
						AntennaParts.Add(part.name, part);
				}
			}
		}

		public static void attachWindowPrefab()
		{
			var prefab = UIPartActionController.Instance.windowPrefab;

			if (prefab == null)
			{
				log("Error in assigning Part Action Window prefab listener...", logLevels.warning);
				return;
			}

			prefab.gameObject.AddOrGetComponent<SEP_UIWindow>();
		}

		public static void loadSprites(Sprite ss1, Sprite ss2, Sprite ss3, Sprite ss4, Sprite ss5)
		{
			CommNetSprites = new Sprite[5] { ss1, ss2, ss3, ss4, ss5 };

			spritesLoaded = true;
		}

		public static List<string> parsePartStringList(string source)
		{
			List<string> list = new List<string>();

			if (string.IsNullOrEmpty(source))
				return list;

			string[] s = source.Split(',');

			int l = s.Length;

			for (int i = 0; i < l; i++)
			{
				string p = s[i];

				AvailablePart a = PartLoader.getPartInfoByName(p.Replace('_', '.'));

				if (a == null)
					continue;

				list.Add(p);
			}

			return list;
		}

		public static List<string> parseModuleStringList(string source)
		{
			List<string> list = new List<string>();

			if (string.IsNullOrEmpty(source))
				return list;

			string[] s = source.Split(',');

			int l = s.Length;

			for (int i = 0; i < l; i++)
			{
				string m = s[i];
				
				for (int j = 0; j < SEP_Utilities.loadedPartModules.Count; j++)
				{
					Type t = SEP_Utilities.loadedPartModules[j];

					if (t == null)
						continue;

					if (t.Name == m)
					{
						list.Add(m);
						break;
					}
				}
			}

			return list;
		}

		private static string currentBiome(ScienceExperiment e, Vessel v)
		{
			if (e == null)
				return "";

			if (v == null)
				return "";

			if (!e.BiomeIsRelevantWhile(ExperimentSituations.SrfLanded))
				return "";

			if (string.IsNullOrEmpty(v.landedAt))
				return ScienceUtil.GetExperimentBiome(v.mainBody, v.latitude, v.longitude);

			return Vessel.GetLandedAtString(v.landedAt);
		}

		public static ScienceSubject subjectIsValid(SEP_ExperimentHandler handler)
		{
			ScienceSubject subject = null;

			List<ScienceSubject> subjects = ResearchAndDevelopment.GetSubjects();

			if (subjects == null || subjects.Count <= 0)
				return null;

			for (int i = 1; i <= 3; i++)
			{
				ScienceExperiment exp = handler.getExperimentLevel(i);

				if (exp == null)
					continue;

				string biome = currentBiome(exp, handler.vessel);

				string id = string.Format("{0}@{1}{2}{3}", exp.id, handler.vessel.mainBody.name, ExperimentSituations.SrfLanded, biome);

				if (subjects.Any(s => s.id == id))
				{
					subject = ResearchAndDevelopment.GetSubjectByID(id);
					//log("Subject ID Confirmed: Science Level - {0:N2}", logLevels.warning, subject.science);
				}

				//log("Subject ID Checked: ID {0}", logLevels.warning, id);
			}

			return subject;
		}

		public static ScienceData getScienceData(SEP_ExperimentHandler handler, ScienceExperiment exp, int level)
		{
			ScienceData data = null;

			string biome = currentBiome(exp, handler.vessel);

			ScienceSubject sub = ResearchAndDevelopment.GetExperimentSubject(exp, ExperimentSituations.SrfLanded, handler.vessel.mainBody, biome);
			sub.title = exp.experimentTitle + situationCleanup(handler.vessel.mainBody, ExperimentSituations.SrfLanded, biome);

			sub.science = handler.submittedData * sub.subjectValue;
			sub.scientificValue = 1 - (sub.science / sub.scienceCap);

			data = new ScienceData(exp.baseValue * exp.dataScale, handler.xmitDataScalar, 1, sub.id, sub.title, false, (uint)handler.flightID);

			//log("Science Data Generated: {0} - Data: {1:F2} - Xmit: {2:F2}", logLevels.warning, data.subjectID, data.dataAmount, data.baseTransmitValue);

			return data;
		}

		public static ScienceSubject checkAndUpdateRelatedSubjects(SEP_ExperimentHandler handler, int level, float data, float submitted)
		{
			ScienceSubject subject = null;

			for (int i = 1; i <= level; i++)
			{
				ScienceExperiment exp = handler.getExperimentLevel(i);

				if (exp == null)
					continue;

				string biome = currentBiome(exp, handler.vessel);

				subject = ResearchAndDevelopment.GetExperimentSubject(exp, ExperimentSituations.SrfLanded, handler.vessel.mainBody, biome);
				subject.title = exp.experimentTitle + situationCleanup(handler.vessel.mainBody, ExperimentSituations.SrfLanded, biome);

				if (i == level)
					subject.science = submitted * subject.subjectValue;
				else
					subject.science = data * subject.subjectValue;

				if (subject.science > subject.scienceCap)
					subject.science = subject.scienceCap;

				subject.scientificValue = 1 - (subject.science / subject.scienceCap);

				//log("Related Subject Checked: ID {0} - Science Level {1:N0}", logLevels.warning, subject.id, subject.science);
			}

			return subject;
		}

		//public static void checkAndUpdateRelatedSubjects(ScienceSubject sub, float data)
		//{
		//	string s = sub.id.Substring(sub.id.Length - 1, 1);

		//	int level = 0;

		//	if (!int.TryParse(s, out level))
		//		return;

		//	if (!sub.IsFromSituation(ExperimentSituations.SrfLanded))
		//		return;

		//	string biome = getBiomeString(sub.id);

		//	string exp = getExperimentString(sub.id, level);

		//	if (string.IsNullOrEmpty(exp))
		//		return;

		//	string body = getBodyString(sub.id);

		//	if (string.IsNullOrEmpty(body))
		//		return;

		//	CelestialBody Body= FlightGlobals.Bodies.FirstOrDefault(b => b.bodyName == body);

		//	if (Body == null)
		//		return;

		//	for (int i = 1; i < level; i++)
		//	{
		//		string fullExp = exp + ((SEPExperiments)i).ToString();

		//		ScienceExperiment experiment = ResearchAndDevelopment.GetExperiment(fullExp);

		//		if (experiment == null)
		//			continue;

		//		ScienceSubject subject = ResearchAndDevelopment.GetExperimentSubject(experiment, ExperimentSituations.SrfLanded, Body, biome);
		//		subject.title = experiment.experimentTitle + situationCleanup(Body, ExperimentSituations.SrfLanded, biome);

		//		subject.science += (data / HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier);

		//		if (subject.science > subject.scienceCap)
		//			subject.science = subject.scienceCap;

		//		subject.scientificValue = 1 - (subject.science / subject.scienceCap);

		//		//log("Related Subject Checked From Event: ID {0} - Science Level {1:N0}", logLevels.warning, subject.id, subject.science);
		//	}
		//}

		public static void checkAndUpdateRelatedSubjects(List<ScienceSubject> allSubs, ScienceSubject sub, float data)
		{
			data /= HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

			if (data <= 0)
				return;

			if (!sub.IsFromSituation(ExperimentSituations.SrfLanded))
				return;

			string biome = getBiomeString(sub.id);

			string exp = getExperimentFullString(sub.id);

			if (string.IsNullOrEmpty(exp))
				return;

			int level = getExperimentLevel(exp);

			if (level == 0)
				return;

			exp = getExperimentStartString(exp, level);

			if (string.IsNullOrEmpty(exp))
				return;

			string body = getBodyString(sub.id);

			if (string.IsNullOrEmpty(body))
				return;

			CelestialBody Body = FlightGlobals.Bodies.FirstOrDefault(b => b.bodyName == body);

			if (Body == null)
				return;

			ScienceSubject subject = null;

			//log("Science Subject Parsed [{0}]: Level: {1} - Experiment: {2}", logLevels.warning, sub.id, level, exp);

			for (int i = 1; i <= 3; i++)
			{
				if (i == level)
					continue;

				string fullExp = exp + ((SEPExperiments)i).ToString();

				ScienceExperiment experiment = ResearchAndDevelopment.GetExperiment(fullExp);

				if (experiment == null)
					continue;

				string id = string.Format("{0}@{1}{2}{3}", experiment.id, body, ExperimentSituations.SrfLanded, biome);

				if (allSubs.Any(a => a.id == id))
				{
					subject = ResearchAndDevelopment.GetSubjectByID(id);
					//log("Subject ID Confirmed: Science Amount - {0:N2}", logLevels.warning, subject.science);
				}
				else
					continue;

				if (i < level)
				{
					subject.science = sub.science;

					if (subject.science > subject.scienceCap)
						subject.science = subject.scienceCap;

					subject.scientificValue = 1 - (subject.science / subject.scienceCap);
				}
				else
				{
					if (subject.science < sub.science)
					{
						subject.science = sub.science;

						if (subject.science > subject.scienceCap)
							subject.science = subject.scienceCap;

						subject.scientificValue = 1 - (subject.science / subject.scienceCap);
					}
				}

				//log("Related Subject Checked From Recovery: ID {0} - Science Level {1:N0}", logLevels.warning, subject.id, subject.science);
			}
		}

		private static string getBiomeString(string sub)
		{
			string sit = ExperimentSituations.SrfLanded.ToString();

			int lastIndex = sub.IndexOf(sit) + sit.Length;

			string b = sub.Substring(lastIndex);

			b = b.Substring(0, b.Length);

			return b;
		}

		public static int getExperimentLevel(string exp)
		{
			int UnderIndex = exp.LastIndexOf('_');

			if (UnderIndex == -1)
				return 0;

			string levelString = exp.Substring(UnderIndex, exp.Length - UnderIndex);

			switch (levelString)
			{
				case "_Basic":
					return 1;
				case "_Detailed":
					return 2;
				case "_Exhaustive":
					return 3;
				default:
					return 0;
			}
		}

		public static string getExperimentFullString(string sub)
		{
			int AtIndex = sub.IndexOf('@');

			if (AtIndex == -1)
				return "";

			return sub.Substring(0, AtIndex);
		}

		private static string getExperimentStartString(string exp, int l)
		{
			int i = exp.IndexOf(((SEPExperiments)l).ToString());

			if (i == -1)
				return "";

			exp = exp.Substring(0, i);

			return exp;
		}

		private static string getBodyString(string sub)
		{
			int AtIndex = sub.IndexOf('@');

			int SitIndex = sub.IndexOf("Srf");

			if (AtIndex == -1 || SitIndex == -1)
				return "";

			if (AtIndex >= SitIndex)
				return "";

			return sub.Substring(AtIndex + 1, SitIndex - AtIndex - 1);
		}

		public static string situationCleanup(CelestialBody body, ExperimentSituations expSit, string b)
		{
			if (b == "")
			{
				switch (expSit)
				{
					case ExperimentSituations.SrfLanded:
						return " from  " + body.theName + "'s surface";
					case ExperimentSituations.SrfSplashed:
						return " from " + body.theName + "'s oceans";
					case ExperimentSituations.FlyingLow:
						return " while flying at " + body.theName;
					case ExperimentSituations.FlyingHigh:
						return " from " + body.theName + "'s upper atmosphere";
					case ExperimentSituations.InSpaceLow:
						return " while in space near " + body.theName;
					default:
						return " while in space high over " + body.theName;
				}
			}
			else
			{
				switch (expSit)
				{
					case ExperimentSituations.SrfLanded:
						return " from " + body.theName + "'s " + b;
					case ExperimentSituations.SrfSplashed:
						return " from " + body.theName + "'s " + b;
					case ExperimentSituations.FlyingLow:
						return " while flying over " + body.theName + "'s " + b;
					case ExperimentSituations.FlyingHigh:
						return " from the upper atmosphere over " + body.theName + "'s " + b;
					case ExperimentSituations.InSpaceLow:
						return " from space just above " + body.theName + "'s " + b;
					default:
						return " while in space high over " + body.theName + "'s " + b;
				}
			}
		}

		public static float getTotalVesselEC(Vessel v)
		{
			double ec = 0;

			for (int i = v.Parts.Count - 1; i >= 0; i--)
			{
				Part p = v.Parts[i];

				if (p == null)
					continue;

				for (int j = p.Resources.Count - 1; j >= 0; j--)
				{
					PartResource r = p.Resources[j];

					if (r == null)
						continue;

					if (r.resourceName != "ElectricCharge")
						continue;

					ec += r.amount;
				}
			}

			return (float)ec;
		}

		public static float getTotalVesselEC(ProtoVessel v)
		{
			double ec = 0;

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

					double amount = resource.amount;

					ec += amount;
				}
			}

			//log("Vessel EC: {0:N4}", logLevels.warning, ec);

			return (float)ec;
		}

		public static float getMaxTotalVesselEC(ProtoVessel v)
		{
			double ec = 0;

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

					double amount = resource.maxAmount;

					ec += amount;
				}
			}

			//log("Vessel EC: {0:N4}", logLevels.warning, ec);

			return (float)ec;
		}
	}

	public enum logLevels
	{
		log = 1,
		warning = 2,
		error = 3,
	}

	public enum SEPComplexity
	{
		Simple = 1,
		Moderate = 2,
		Complex = 3,
		Fiendish = 4,
	}

	public enum SEPExperiments
	{
		_Basic = 1,
		_Detailed = 2,
		_Exhaustive = 3,
	}
}
