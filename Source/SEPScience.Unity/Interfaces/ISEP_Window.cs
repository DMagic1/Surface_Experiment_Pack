
using System.Collections.Generic;
using UnityEngine;

namespace SEPScience.Unity.Interfaces
{
	public interface ISEP_Window
	{
		bool IsVisible { get; set; }

		bool IsMinimized { get; set; }

		IList<IVesselSection> GetVessels();

		void ProcessStyle(GameObject obj);

		void UpdateWindow();
	}
}
