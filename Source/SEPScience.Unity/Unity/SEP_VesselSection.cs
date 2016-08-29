using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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
		//[SerializeField]
		//private Image Connection = null;
		[SerializeField]
		private Text TotalEC = null;
		[SerializeField]
		private Text VesselTitle = null;
		[SerializeField]
		private Text SituationText = null;
		//[SerializeField]
		//private Sprite ConnectedSprite = null;
		//[SerializeField]
		//private Sprite DisconnectedSprite = null;
		[SerializeField]
		private GameObject ExperimentSectionPrefab = null;
		[SerializeField]
		private Transform ExperimentSectionTransform = null;

		private Color green = new Color(0.345098f, 1, .082353f, 1);
		private Color grey = new Color(0.329412f, 0.329412f, 0.329412f, 1);
		private IVesselSection vesselInterface;
		private List<SEP_ExperimentSection> experiments = new List<SEP_ExperimentSection>();

		private void Update()
		{
			if (vesselInterface == null)
				return;

			if (!vesselInterface.IsVisible)
				return;

			//if (Transmission != null)
			//	Transmission.isOn = vesselInterface.CanTransmit;

			vesselInterface.Update();

			if (TotalEC != null)
				TotalEC.text = vesselInterface.ECTotal;

			//if (Connection != null)
			//	Connection.sprite = vesselInterface.IsConnected ? ConnectedSprite : DisconnectedSprite;

			if (SituationText != null)
				SituationText.text = vesselInterface.Situation;

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
		}

		public void setVessel(IVesselSection vessel)
		{
			if (vessel == null)
				return;

			vesselInterface = vessel;

			if (VesselTitle != null)
				VesselTitle.text = vessel.Name;

			if (SituationText != null)
				SituationText.text = vessel.Situation;

			if (TransmissionBackground != null)
			{
				if (vesselInterface.CanTransmit && vesselInterface.AutoTransmitAvailable)
					TransmissionBackground.color = green;
				else
					TransmissionBackground.color = grey;
			}

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

			print("[SEP UI] Start All Experiments...");

			vesselInterface.StartAll();

			if (StartAll != null && PauseAll != null)
			{
				print("[SEP UI] Setting Start Button Inactive...");

				PauseAll.gameObject.SetActive(false);

				StartAll.gameObject.SetActive(true);
			}
		}

		public void PauseAllExperiments()
		{
			if (vesselInterface == null)
				return;

			print("[SEP UI] Pause All Experiments...");

			vesselInterface.PauseAll();

			if (StartAll != null && PauseAll != null)
			{
				print("[SEP UI] Setting Pause Button Inactive...");

				PauseAll.gameObject.SetActive(true);

				StartAll.gameObject.SetActive(false);
			}
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

			vesselInterface.ProcessStyle(sectionObject);

			sectionObject.transform.SetParent(ExperimentSectionTransform, false);

			SEP_ExperimentSection experiment = sectionObject.GetComponent<SEP_ExperimentSection>();

			if (experiment == null)
				return;

			experiment.setExperiment(section);

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
