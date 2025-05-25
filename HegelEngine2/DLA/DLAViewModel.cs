using HegelEngine2.ParametersClasses;

namespace HegelEngine2.DLA;
public class DLAViewModel : ViewModel
{
    public int Iterations { get; set; }

    public float SolidMass { get; set; }

    public float Diameter { get; set; }

    public string FileName { get; set; }

    public int BootParam { get; set; }

    private float _reactionProbability;
    public float ReactionProbability
    {
        get
        {
            return _reactionProbability;
        }
        set
        {
            if (value < 0)
                _reactionProbability = 0;
            else if (value > 1)
                _reactionProbability = 1;
            else
                _reactionProbability = value;
        }
    }

    private int _porosity;

    public int Porosity
    {
        get 
        { 
            return _porosity; 
        }
        set 
        { 
            _porosity = 100 - value; 
        }
    }
}