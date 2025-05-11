namespace HegelEngine2.ReleaseModel;

public class RmViewModel : ViewModel
{
    public float SaturatedConc { get; set; }

    public float D { get; set; }

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
