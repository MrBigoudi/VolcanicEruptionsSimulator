using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/**
 * The display mode of the fps counter
*/
public enum DisplayMode {FPS, MS};

/**
 * A class to display the frame rate
*/
public class FrameRateCounter : MonoBehaviour{

// ################################################################################################################################################################################################################
// ################################################################################################## ATTRIBUTES ##################################################################################################
// ################################################################################################################################################################################################################

    /**
     * The unity gui shown on screen
    */
    [SerializeField]
    public TextMeshProUGUI _Display;

    /**
     * The duration of a sample
    */
    [SerializeField, Range(0.1f, 2f)]
	public float _SampleDuration = 1.0f;

    /**
     * Tells if the values should be displayed in Frame per seconds or in Milliseconds
    */
    [SerializeField]
    public DisplayMode _Mode = DisplayMode.FPS;

    /**
     * The frame counter
    */
    private int _Frames;

    /**
     * The current time spent
    */
    private float _Duration;

    /**
     * The current longest duration time
    */
    private float _WorstDuration;
    
    /**
     * The current smallest duration time
    */
    private float _BestDuration = float.MaxValue;

    /**
     * The particle generator used to get the number of particles
    */
    [SerializeField]
    public ParticleGenerator _ParticleGenerator;

    /**
     * The current number of particles
    */
    private int _NbParticles;


// ################################################################################################################################################################################################################
// ################################################################################################### METHODS ####################################################################################################
// ################################################################################################################################################################################################################

    /**
     * Updates the current number of particles
    */
    void GetNbParticles(){
        _NbParticles = _ParticleGenerator.GetNbCurParticles();
    }

    /**
     * Update the FPS display
    */
    void Update(){
        // update values
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

        // update display if needed
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

            // reset values
            _Frames = 0;
            _Duration = 0.0f;
            _BestDuration = float.MaxValue;
            _WorstDuration = 0.0f;
        }
    }
}