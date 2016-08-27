using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SEPScience.Unity
{
	public class ToggleButton : Toggle
	{
		private Sprite sprite;

		protected override void Start()
		{
			base.Start();

			sprite = ((Image)targetGraphic).sprite;

			onValueChanged.AddListener(value =>
				{
					switch (transition)
					{
						case Transition.ColorTint:
							image.color = isOn ? colors.pressedColor : colors.normalColor;
							break;
						case Transition.SpriteSwap:
							image.sprite = isOn ? spriteState.pressedSprite : sprite;
							break;
						default:
							throw new NotImplementedException();
					}
				});
		}
	}
}
