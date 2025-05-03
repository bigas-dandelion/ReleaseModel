namespace HegelEngine2.TestCA;

public class GofViewModel : ViewModel
{
    private float _lifeFraction;
    public float LifeFraction
    {
        get
        {
            return _lifeFraction;
        }
        set
        {
            if (value < 0)
                _lifeFraction = 0;
            else if (value > 1)
                _lifeFraction = 1;
            else
                _lifeFraction = value;
        }
    }

    public int Iterations { get; set; }
}
