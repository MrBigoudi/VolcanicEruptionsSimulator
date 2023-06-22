using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FrameRateCounter : MonoBehaviour{
    [SerializeField]
    TextMeshProUGUI _Display;

    [SerializeField, Range(0.1f, 2f)]
	float _SampleDuration = 1.0f;

    public enum DisplayMode {FPS, MS};

    [SerializeField]
    DisplayMode _Mode = DisplayMode.FPS;

    int _Frames;
    float _Duration;
    float _WorstDuration, _BestDuration = float.MaxValue;

    void Update(){
        float frameDuration = Time.unscaledDeltaTime;
		_Frames += 1;
		_Duration += frameDuration;

        if(frameDuration < _BestDuration){
            _BestDuration = frameDuration;
        }

        if(frameDuration > _WorstDuration){
            _WorstDuration = frameDuration;
        }

        if(_Duration >= _SampleDuration){
            switch(_Mode){
                case DisplayMode.FPS:
                    _Display.SetText("FPS\n{0:0}\n{1:0}\n{2:0}", 
                        1.0f / _BestDuration, 
                        _Frames / _Duration, 
                        1.0f / _WorstDuration);
                    break;
                case DisplayMode.MS:
                    _Display.SetText("MS\n{0:1}\n{1:1}\n{2:1}", 
                        1000.0f * _BestDuration, 
                        1000.0f * _Duration / _Frames, 
                        1000.0f * _WorstDuration);
                    break;
                default:
                    break;
            }

            _Frames = 0;
            _Duration = 0.0f;
            _BestDuration = float.MaxValue;
            _WorstDuration = 0.0f;
        }
    }
}
