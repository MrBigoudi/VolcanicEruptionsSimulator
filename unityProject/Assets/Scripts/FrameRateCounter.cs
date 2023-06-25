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

    [SerializeField]
    ParticleGenerator _ParticleGenerator;
    int _NbParticles;

    void GetNbParticles(){
        _NbParticles = _ParticleGenerator.mSph.mNbCurParticles;
    }

    void Update(){
        GetNbParticles();
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
                    _Display.SetText("FPS\nMax {0:0}\nAvg {1:0}\nMin {2:0}\nParticles {3:0}", 
                        1.0f / _BestDuration, 
                        _Frames / _Duration, 
                        1.0f / _WorstDuration,
                        _NbParticles);
                    break;
                case DisplayMode.MS:
                    _Display.SetText("MS\n{0:1}\n{1:1}\n{2:1}\nParticles {3:0}", 
                        1000.0f * _BestDuration, 
                        1000.0f * _Duration / _Frames, 
                        1000.0f * _WorstDuration,
                        _NbParticles);
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
