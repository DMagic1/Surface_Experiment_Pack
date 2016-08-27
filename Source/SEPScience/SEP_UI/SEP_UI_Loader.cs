using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI;

namespace SEPScience.SEP_UI
{
	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	public class SEP_UI_Loader : MonoBehaviour
	{
		private static bool loaded;
		private static bool running;

		private static AssetBundle images;
		private static AssetBundle prefabs;

		public static AssetBundle Images
		{
			get { return images; }
		}

		public static AssetBundle Prefabs
		{
			get { return prefabs; }
		}

		private void Awake()
		{
			string path = KSPUtil.ApplicationRootPath + "GameData/SurfaceExperimentPackage/Resources";

			images = AssetBundle.CreateFromFile(path + "/sep_images.ksp");
			prefabs = AssetBundle.CreateFromFile(path + "/sep_prefab.ksp");
		}

		private static GameObject uiPrefab;
		private RectTransform uiWindow;

		//private void Start()
		//{
		//	if (running)
		//		Destroy(gameObject);

		//	running = true;

		//	//StartCoroutine(UILoad());

		//	if (loaded && uiPrefab != null)
		//		UICreate(uiPrefab);
		//	else
		//		UILoad();
		//}

		private void OnDestroy()
		{
			running = false;
		}

		private void UILoad()
		{
			var path = KSPUtil.ApplicationRootPath + "GameData/SurfaceExperimentPackage/Resources/sep_ui.ksp";

			print("[SEP] Asset bundles load");
			WWW bundleRequest = new WWW("file://" + path);

			//yield return bundleRequest;


			if (bundleRequest.assetBundle == null)
			{
				print("[SEP] Failed to load AssetBundle!\n" + path + "\n" + bundleRequest.error + "\n" + bundleRequest.assetBundle);
				return;
				//yield break;
			}

			var loadRequest = bundleRequest.assetBundle.LoadAssetAsync<GameObject>("assets/prefabs/sep_window.prefab");

			//yield return loadRequest;

			uiPrefab = loadRequest.asset as GameObject;

			if (uiPrefab == null)
			{
				print("[SEP] Failed to find the prefab in the AssetBundle!");
				return;
				//yield break;
			}

			loaded = true;

			UICreate(uiPrefab);
		}

		private void UICreate(GameObject prefab)
		{
			print("[SEP] UI Instantiate");
			GameObject go = Instantiate(prefab);

			// Set the parrent to the stock appCanvas
			go.transform.SetParent(UIMasterController.Instance.appCanvas.transform, false);

			uiWindow = go.transform as RectTransform;

			uiWindow.gameObject.SetActive(true);

		}
	}
}
