using System;
using System.Collections.Generic;
using UnityEngine;

namespace SEPScience
{
	public class SEP_GameParameters : GameParameters.CustomParameterNode
	{

		[GameParameters.CustomParameterUI("Display All Vessels", toolTip = "The window will display all vessels in the same list, rather than separating them by celestial body", autoPersistance = true)]
		public bool showAllVessels;
		[GameParameters.CustomParameterUI("Window Fade Out", toolTip = "This controls whether the window fades out when not in focus", autoPersistance = true)]
		public bool fadeOut = true;
		[GameParameters.CustomFloatParameterUI("Scale", minValue = 0.5f, maxValue = 2, asPercentage = true, displayFormat = "P0", autoPersistance = true)]
		public float scale = 1;
		[GameParameters.CustomParameterUI("Use As Default", autoPersistance = false)]
		public bool useAsDefault;

		public SEP_GameParameters()
		{

		}

		public override GameParameters.GameMode GameMode
		{
			get { return GameParameters.GameMode.CAREER | GameParameters.GameMode.SCIENCE; }
		}

		public override bool HasPresets
		{
			get { return false; }
		}

		public override string Section
		{
			get { return "Surface Experiment Package"; }
		}

		public override int SectionOrder
		{
			get { return 0; }
		}

		public override string Title
		{
			get { return "Surface Experiment Package"; }
		}
	}
}
