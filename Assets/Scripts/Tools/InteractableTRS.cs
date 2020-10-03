using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableTRS : MonoBehaviour
{
    [Header("Movement Properties")]
    public TransformPreset[] PresetsTRS;
    public bool movementInteruptible = true;
    public float transitionSpeed = 2.0f;

    private bool isMoving = false;
    private int currentTransformIndex = 0;
    private int currentRotationIndex = 0; 
    private int currentLocalScaleIndex = 0;

    private void Update()
    {
        //Translation
        if (this.transform.localPosition != PresetsTRS[currentTransformIndex].localPosition &&
            Vector3.Distance(this.transform.localPosition, PresetsTRS[currentTransformIndex].localPosition) <= 0.3f)
        {
            this.transform.localPosition = PresetsTRS[currentTransformIndex].localPosition;
        }
        else
        {
            this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, 
                                                        PresetsTRS[currentTransformIndex].localPosition, 
                                                        transitionSpeed * Time.deltaTime);
        }

        //Rotation
        if (this.transform.localEulerAngles != PresetsTRS[currentRotationIndex].localEulerAngles &&
            Vector3.Distance(this.transform.localEulerAngles, PresetsTRS[currentRotationIndex].localEulerAngles) <= 0.3f)
        {
            this.transform.localEulerAngles = PresetsTRS[currentRotationIndex].localEulerAngles;
        }
        else
        {
            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation,
                                                           Quaternion.Euler(PresetsTRS[currentRotationIndex].localEulerAngles),
                                                           transitionSpeed * Time.deltaTime);
        }

        //Scaling
        if (this.transform.localScale != PresetsTRS[currentLocalScaleIndex].localScale &&
            Vector3.Distance(this.transform.localScale, PresetsTRS[currentLocalScaleIndex].localScale) <= 0.3f)
        {
            this.transform.localScale = PresetsTRS[currentLocalScaleIndex].localScale;
        }
        else
        {
            this.transform.localScale = Vector3.Lerp(this.transform.localScale,
                                                     PresetsTRS[currentLocalScaleIndex].localScale,
                                                     transitionSpeed * Time.deltaTime);
        }
    }



    #region Translation Methods
    public void TranslateToIndex(int index)
    {
        if (movementInteruptible &&
            Vector3.Distance(this.transform.localPosition, PresetsTRS[currentTransformIndex].localPosition) > 0.1f)
        {
            currentTransformIndex = index;
            PresetsTRS[currentTransformIndex].onPositionUpdate.Invoke();
        }
    }

    public void TranslateToNextIndex()
    {
        if (!movementInteruptible) return;

        currentTransformIndex = currentTransformIndex + 1 >= PresetsTRS.Length ? 0 : currentTransformIndex + 1;
        PresetsTRS[currentTransformIndex].onPositionUpdate.Invoke();
    }
    #endregion


    #region Rotation Methods
    public void RotateToIndex(int index)
    {
        if (movementInteruptible &&
            Vector3.Distance(this.transform.localEulerAngles, PresetsTRS[currentRotationIndex].localEulerAngles) > 0.1f)
        {
            currentRotationIndex = index;
            PresetsTRS[currentRotationIndex].onRotationUpdate.Invoke();
        }
    }
    
    public void RotateToNextIndex()
    {
        if (!movementInteruptible) return;
        
        currentRotationIndex = currentRotationIndex + 1 >= PresetsTRS.Length ? 0 : currentRotationIndex + 1;
        PresetsTRS[currentRotationIndex].onRotationUpdate.Invoke();
    }
    #endregion


    #region Scaling Methods
    public void ScaleToIndex(int index)
    {
        if (movementInteruptible &&
            Vector3.Distance(this.transform.localEulerAngles, PresetsTRS[currentLocalScaleIndex].localEulerAngles) > 0.1f)
        {
            currentLocalScaleIndex = index;
            PresetsTRS[currentLocalScaleIndex].onScaleUpdate.Invoke();
        }
    }

    public void ScaleToNextIndex()
    {
        if (!movementInteruptible) return;
        
        currentLocalScaleIndex = currentLocalScaleIndex + 1 >= PresetsTRS.Length ? 0 : currentLocalScaleIndex + 1;
        PresetsTRS[currentLocalScaleIndex].onScaleUpdate.Invoke();
    }
    #endregion
}
