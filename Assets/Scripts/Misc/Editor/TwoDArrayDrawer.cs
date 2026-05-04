using UnityEngine;
using MonkeyBusiness.Misc;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;

namespace MonkeyBusiness.Misc.Editor
{
    #if UNITY_EDITOR
    public class TwoDArrayDrawerFloat : OdinValueDrawer<TwoDArray<float>>
    {
        const float MIN_CELL_WIDTH = 45;
        const float MIN_CELL_HEIGHT = 15;

        const float PADDING = 5;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            TwoDArray<float> array = ValueEntry.SmartValue;

            var height = MIN_CELL_HEIGHT * array.Height + PADDING * (array.Height - 1);

            Rect rect = EditorGUILayout.GetControlRect(label!=null, height);

            if(label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            rect.height = array.Height * MIN_CELL_HEIGHT + PADDING * (array.Height - 1);
            rect.width = array.Width * MIN_CELL_WIDTH + PADDING * (array.Width - 1);

            /*rect.SetHeight(Mathf.Max(rect.height, MIN_CELL_HEIGHT * array.Height));
            rect.SetWidth(Mathf.Max(rect.width, MIN_CELL_WIDTH * array.Width));*/

            GUIHelper.PushLabelWidth(5);

            float colWidth =  MIN_CELL_WIDTH; /*Mathf.Max(rect.width / array.Width, MIN_CELL_WIDTH);*/
            float rowHeight = MIN_CELL_HEIGHT; /*Mathf.Max(rect.height / array.Height, MIN_CELL_HEIGHT);*/

            for(int i = 0; i < array.Height; i++)
            {
                Rect rowRect = rect.AlignTop(rowHeight).AddY(i * rowHeight + PADDING * i).AddX(-150);

                for(int j = 0; j < array.Width; j++)
                {
                    Rect colRect = rowRect.AlignLeft(colWidth).AddX(j * colWidth + PADDING * j);
                    array[i,j] = EditorGUI.DelayedFloatField(colRect, Mathf.Round(array[i,j] * 100f) / 100f); // Round to 3 decimal places for better readability
                }
            }
            GUIHelper.PopLabelWidth();

            this.ValueEntry.SmartValue = array;
        }
    }
    #endif
}
