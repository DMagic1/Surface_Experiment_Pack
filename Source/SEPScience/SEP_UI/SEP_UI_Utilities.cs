﻿#region license
/*The MIT License (MIT)
SEP_UI_Utilities - Static utilities class for UI related functions

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

using KSP.UI.Screens.Flight.Dialogs;
using UnityEngine;
using UnityEngine.UI;
using SEPScience.Unity;
using SEPScience.Unity.Unity;

namespace SEPScience.SEP_UI
{
	public static class SEP_UI_Utilities
	{
		private static Sprite sliderFrontForeground;
		private static Sprite sliderBackBackground;
		private static Sprite sliderBackForeground;
		private static Color sliderFrontForeColor;
		private static Color sliderBackBackColor;
		private static Color sliderBackForeColor;

		public static void processComponents(GameObject obj)
		{
			if (obj == null)
				return;

			if (sliderFrontForeground == null || sliderBackBackground == null || sliderBackForeground == null)
			{
				ExperimentsResultDialog scienceDialogPrefab = UnityEngine.Object.Instantiate<GameObject>(AssetBase.GetPrefab("ScienceResultsDialog")).GetComponent<ExperimentsResultDialog>();

				if (scienceDialogPrefab != null)
				{
					Slider[] sliders = scienceDialogPrefab.GetComponentsInChildren<Slider>(); ;

					Slider backSlider = sliders[0];
					Slider frontSlider = sliders[1];

					sliderBackBackground = processSliderSprites(backSlider, true, ref sliderBackBackColor);
					sliderBackForeground = processSliderSprites(backSlider, false, ref sliderBackForeColor);

					sliderFrontForeground = processSliderSprites(frontSlider, false, ref sliderFrontForeColor);
				}
			}

			SEP_Style[] styles = obj.GetComponentsInChildren<SEP_Style>();

			if (styles == null)
				return;

			for (int i = 0; i < styles.Length; i++)
				processCompenents(styles[i]);
		}

		private static Sprite processSliderSprites(Slider slider, bool back, ref Color color)
		{
			if (slider == null)
				return null;

			if (back)
			{
				Image background = slider.GetComponentInChildren<Image>();

				if (background == null)
					return null;

				color = background.color;

				return background.sprite;
			}
			else
			{
				RectTransform fill = slider.fillRect;

				if (fill == null)
					return null;

				Image fillImage = fill.GetComponent<Image>();

				if (fillImage == null)
					return null;

				color = fillImage.color;

				return fillImage.sprite;
			}
		}

		private static Styles getStyle(UIStyle style, UIStyleState state)
		{
			Styles s = new Styles();

			if (style != null)
			{
				s.Font = style.font;
				s.Style = style.fontStyle;
				s.Size = style.fontSize;
			}

			if (state != null)
			{
				s.Color = state.textColor;
			}

			return s;
		}

		private static void processCompenents(SEP_Style style)
		{
			if (style == null)
				return;

			UISkinDef skin = UISkinManager.defaultSkin;

			if (skin == null)
				return;

			switch (style.ElementType)
			{
				case SEP_Style.ElementTypes.Window:
					style.setImage(skin.window.normal.background, Image.Type.Sliced);
					break;
				case SEP_Style.ElementTypes.Box:
					style.setImage(skin.box.normal.background, Image.Type.Sliced);
					break;
				case SEP_Style.ElementTypes.Button:
					style.setButton(skin.button.normal.background, skin.button.highlight.background, skin.button.active.background, skin.button.disabled.background);
					break;
				case SEP_Style.ElementTypes.ToggleButton:
					style.setToggle(skin.button.normal.background, skin.button.highlight.background, skin.button.active.background, skin.button.disabled.background);
					break;
				case SEP_Style.ElementTypes.ToggleAlwaysOn:
					style.setToggle(skin.button.normal.background, skin.button.highlight.background, skin.button.active.background, skin.button.normal.background);
					break;
				case SEP_Style.ElementTypes.Label:
					style.setText(getStyle(skin.label, skin.label.normal));
					break;
				case SEP_Style.ElementTypes.VertScroll:
					style.setScrollbar(skin.verticalScrollbar.normal.background, skin.verticalScrollbarThumb.normal.background);
					break;
				case SEP_Style.ElementTypes.Slider:
					style.setSlider(sliderFrontForeground, sliderFrontForeColor);
					break;
				case SEP_Style.ElementTypes.SliderBackground:
					style.setSlider(sliderBackBackground, sliderBackForeground, sliderBackBackColor, sliderBackForeColor);
					break;
			}
		}

	}
}
