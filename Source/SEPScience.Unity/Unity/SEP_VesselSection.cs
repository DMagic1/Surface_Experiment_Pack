#region license
/*The MIT License (MIT)
SEP_VesselSection - Unity UI element for SEP vessels

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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SEPScience.Unity.Interfaces;

namespace SEPScience.Unity.Unity
{
	public class SEP_VesselSection : MonoBehaviour
	{
		//[SerializeField]
		//private Toggle Transmission = null;
		[SerializeField]
		private Image TransmissionBackground = null;
		//[SerializeField]
		//private Sprite TransmissionSprite = null;
		[SerializeField]
		private Toggle Minimize = null;
		[SerializeField]
		private Button PauseAll = null;
		[SerializeField]
		private Button StartAll = null;
		[SerializeField]
		private Image Connection = null;
		[SerializeField]
		private TextHandler TotalEC = null;
		[SerializeField]
		private TextHandler ExpCount = null;
		[SerializeField]
		private TextHandler VesselTitle = null;
		[SerializeField]
		private TextHandler SituationText = null;
		[SerializeField]
		private TextHighlighter TitleHighlighter = null;
		[SerializeField]
		private GameObject ExperimentSectionPrefab = null;
		[SerializeField]
		private Transform ExperimentSectionTransform = null;

		private Color vesselGreen = new Color(0.552941f, 1, 0, 1);
		private Color white = new Color(1, 1, 1, 1); 
		private Color green = new Color(0.345098f, 1, .082353f, 1);
		private Color grey = new Color(0.329412f, 0.329412f, 0.329412f, 1);
		private IVesselSection vesselInterface;
		private List<SEP_ExperimentSection> experiments = new List<SEP_ExperimentSection>();
		private Guid id;

		public Guid ID
		{
			get { return id; }
		}

		private void OnDestroy()
		{

		}

		private void Update()
		{
			if (vesselInterface == null)
				return;

			if (!vesselInterface.IsVisible)
				return;

			vesselInterface.Update();

			if (TotalEC != null)
				TotalEC.OnTextUpdate.Invoke(vesselInterface.ECTotal);

			if (Connection != null)
			{
				Sprite connect = vesselInterface.SignalSprite;

				if (connect != null)
					Connection.sprite = connect;
			}

			if (StartAll != null && PauseAll != null)
			{
				if (anyRunning())
				{
					if (!PauseAll.gameObject.activeSelf)
						PauseAll.gameObject.SetActive(true);
				}
				else if (PauseAll.gameObject.activeSelf)
					PauseAll.gameObject.SetActive(false);

				if (anyPaused())
				{
					if (!StartAll.gameObject.activeSelf)
						StartAll.gameObject.SetActive(true);
				}
				else if (StartAll.gameObject.activeSelf)
					StartAll.gameObject.SetActive(false);
			}
		}

		public void setVessel(IVesselSection vessel)
		{
			if (vessel == null)
				return;

			vesselInterface = vessel;

			id = vessel.ID;

			if (VesselTitle != null)
				VesselTitle.OnTextUpdate.Invoke(vessel.Name);

			if (SituationText != null)
				SituationText.OnTextUpdate.Invoke(vessel.Situation);

			if (TransmissionBackground != null)
			{
				if (vesselInterface.CanTransmit && vesselInterface.AutoTransmitAvailable)
					TransmissionBackground.color = green;
				else
					TransmissionBackground.color = grey;
			}

			if (SEP_Window.Window != null && SEP_Window.Window.ScrollRect != null && TitleHighlighter != null)
				TitleHighlighter.setScroller(SEP_Window.Window.ScrollRect);

			vesselInterface.IsVisible = true;

			CreateExperimentSections(vesselInterface.GetExperiments());

			if (StartAll != null && PauseAll != null)
			{
				if (anyRunning())
					PauseAll.gameObject.SetActive(true);
				else
					PauseAll.gameObject.SetActive(false);

				if (anyPaused())
					StartAll.gameObject.SetActive(true);
				else
					StartAll.gameObject.SetActive(false);
			}

			setExpCount(vesselInterface.ExpCount);
			setBiome(vesselInterface.Situation);

			vesselInterface.setParent(this);
		}

		public void setExpCount(string s)
		{
			if (ExpCount != null)
				ExpCount.OnTextUpdate.Invoke(s);
		}

		public void setBiome(string s)
		{
			if (SituationText != null)
				SituationText.OnTextUpdate.Invoke(s);
		}

		public void setTransmission()
		{
			if (vesselInterface == null)
				return;

			vesselInterface.CanTransmit = !vesselInterface.CanTransmit && vesselInterface.AutoTransmitAvailable;

			if (TransmissionBackground != null)
			{
				if (vesselInterface.CanTransmit && vesselInterface.AutoTransmitAvailable)
					TransmissionBackground.color = green;
				else
					TransmissionBackground.color = grey;
			}
		}

		public void setTransmissionSilent(bool isOn)
		{
			if (TransmissionBackground == null)
				return;

			if (isOn)
				TransmissionBackground.color = green;
			else
				TransmissionBackground.color = grey;		
		}

		public void setVesselVisible(bool on)
		{
			if (vesselInterface == null)
				return;

			//vesselInterface.IsVisible = !vesselInterface.IsVisible;

			/////

			
		}

		public void setExperimentVisibility(bool on)
		{
			for (int i = experiments.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentSection experiment = experiments[i];

				if (experiment == null)
					return;

				experiment.toggleVisibility(on);
			}
		}

		public void StartAllExperiments()
		{
			if (vesselInterface == null)
				return;

			vesselInterface.StartAll();

			if (StartAll != null && PauseAll != null)
			{
				PauseAll.gameObject.SetActive(false);

				StartAll.gameObject.SetActive(true);
			}
		}

		public void PauseAllExperiments()
		{
			if (vesselInterface == null)
				return;

			vesselInterface.PauseAll();

			if (StartAll != null && PauseAll != null)
			{
				PauseAll.gameObject.SetActive(true);

				StartAll.gameObject.SetActive(false);
			}
		}

		public void AddExperimentSection(IExperimentSection section)
		{
			if (section == null)
				return;

			CreateExperimentSection(section);
		}

		public void RemoveExperimentSection(SEP_ExperimentSection section)
		{
			if (section == null)
				return;

			if (experiments.Contains(section))
				experiments.Remove(section);
		}

		private void CreateExperimentSections(IList<IExperimentSection> sections)
		{
			if (sections == null)
				return;

			if (ExperimentSectionPrefab == null)
				return;

			if (ExperimentSectionTransform == null)
				return;

			for (int i = sections.Count - 1; i >= 0 ; i--)
			{
				IExperimentSection section = sections[i];

				if (section == null)
					continue;

				CreateExperimentSection(section);
			}
		}

		private void CreateExperimentSection(IExperimentSection section)
		{
			GameObject sectionObject = Instantiate(ExperimentSectionPrefab);

			if (sectionObject == null)
				return;

			sectionObject.transform.SetParent(ExperimentSectionTransform, false);

			SEP_ExperimentSection experiment = sectionObject.GetComponent<SEP_ExperimentSection>();

			if (experiment == null)
				return;

			experiment.setExperiment(section, this);

			if (Minimize != null)
				experiment.toggleVisibility(Minimize.isOn);
			else
				experiment.toggleVisibility(true);

			experiments.Add(experiment);
		}

		private bool anyRunning()
		{
			bool b = false;

			for (int i = experiments.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentSection section = experiments[i];

				if (section == null)
					continue;

				if (section.experimentRunning)
				{
					b = true;
					break;
				}
			}

			return b;
		}

		private bool anyPaused()
		{
			bool b = false;

			for (int i = experiments.Count - 1; i >= 0; i--)
			{
				SEP_ExperimentSection section = experiments[i];

				if (section == null)
					continue;

				if (!section.experimentRunning)
				{
					b = true;
					break;
				}
			}

			return b;
		}

	}
}
