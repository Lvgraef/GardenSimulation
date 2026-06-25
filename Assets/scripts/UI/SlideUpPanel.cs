using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
   public class SlideUpPanel : MonoBehaviour
   {
      [SerializeField] private RectTransform panel;
      [SerializeField] private float hiddenY;
      [SerializeField] private float shownY;
      [SerializeField] private float animationDuration;
      [SerializeField] private GameObject miniReport;
      [SerializeField] private TMP_Text PanelText;
      private bool _isShown;
      private Coroutine _animationCoroutine;

      public void Toggle()
      {
         if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
         
         _animationCoroutine = StartCoroutine(Animate(_isShown ? 1f : 0f, _isShown ? 0f : 1f, _isShown));
         _isShown = !_isShown;
      }

      public IEnumerator Animate(float from, float to, bool isShown)
      {
         float t = 0f;
         if (!isShown)
         {
            PanelText.text = "Close Mini Report";
            miniReport.SetActive(true);
         }
        
         while (t < animationDuration)
         {
            // using unscaled deltatime for adding the frames
            t += Time.unscaledDeltaTime;
            

            // using lerp to smoothly animate towards end point
            float v = Mathf.Lerp(from, to, Mathf.Clamp01(t / animationDuration));
            panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, Mathf.Lerp(hiddenY, shownY, v));
            yield return null;
         }

         panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, Mathf.Lerp(hiddenY, shownY, to));
         if (isShown)
         {
            PanelText.text = "Open Mini Report";
            miniReport.SetActive(false);
         }
      }

      private void Start()
      {
         Toggle();
      }
   }
}
