using System.Collections;
using UnityEngine;
using BansheeGz.BGSpline.Components;

/// <summary>
/// 
/// </summary>
public class CurveControl : MonoBehaviour 
{
    #region Variable Declarations 
    // Private Serializable
    [SerializeField] float speed = 5f;
    [SerializeField] BGCcCursorChangeLinear[] cursorChanger;
    // Private
    int currentCurve;
    #endregion
 
 
 
    #region Public Properties
        
    #endregion
 
 
 
    #region Unity Event Functions
    private void Start()
    {
        cursorChanger[0].Speed = speed;
        currentCurve++;
        //for (int i = 0; i < cursorChanger.Length; i++)
        //{
        //    StartCoroutine(StartCurve(cursorChanger[i]));
        //}
    }


    private void Update()
    {
        if (currentCurve >= cursorChanger.Length) return;

        if (cursorChanger[currentCurve - 1].Stopped)
        {
            cursorChanger[currentCurve].Speed = speed;
            currentCurve++;
        }
    }
    #endregion



    #region Public Functions

    #endregion



    #region Private Functions

    #endregion



    #region Coroutines
    //IEnumerator StartCurve(Curve curve)
    //{
    //    for (int i = 0; i < cursorChanger.Length; i++)
    //    {
    //        yield return new WaitForSeconds(curve.delay);
    //        curve.cursorChanger.Stopped
    //        curve.cursorChanger.Speed = curve.speed;
    //    }
    //}
    #endregion
}