namespace HegelEngine2.ReleaseModel;

public class RmViewModel : ViewModel
{
    public float SaturatedConc { get; set; }

    public string FileName { get; set; }

    public int MembraneWidth { get; set; }

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

    public bool IsTherePorosity { get; set; }
    public float D { get; set; }

    public float D2 { get; set; }

    public float K { get; set; }

    public float dt { get; set; }

    public float dx { get; set; }

    public float Diameter { get; set; }

    private float _solidMass;

    public float SolidMass
    {
        get { return _solidMass; }
        set { _solidMass = value; }
    }

    private float _liquidMass;

    public float LiquidMass
    {
        get
        {
            return _liquidMass;
        }
        set
        {
            if (_liquidMass >= _solidMass)
                _liquidMass /= 2;
            else
                _liquidMass = value;
        }
    }

    public int Iterations { get; set; }
}
