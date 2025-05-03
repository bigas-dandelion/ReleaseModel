namespace HegelEngine2.Utils;

public class VectorInt
{

    public static VectorInt Zero { get => new VectorInt(0, 0, 0); }
    public static VectorInt OneX { get => new VectorInt(1, 0, 0); }
    public static VectorInt OneY { get => new VectorInt(0, 1, 0); }
    public static VectorInt OneZ { get => new VectorInt(0, 0, 1); }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }


    public VectorInt(int X, int Y) // Vlad: this -- т. к. имена полей класса и параметров совпадают.

    {
        this.X = X;
        this.Y = Y;
        Z = 0;
    }
    public VectorInt(int X, int Y, int Z)
    {
        this.X = X;
        this.Y = Y;
        this.Z = Z;
    }
    public bool IsNullVectorInt()
    {
        return X == 0 && Y == 0 && Z == 0;
    }
    public float GetModule() // Vlad: Градиент.
    {
        return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
    }
    //public VectorInt NormalizeVectorInt()
    //{
    //    float m = GetModule();
    //    return new VectorInt(this.X / m, this.Y / m, this.Z / m);
    //}
    public static VectorInt MultipleModule(float m, VectorInt v)
    {
        return new VectorInt((int)(v.X * m), (int)(v.Y * m), (int)(v.Z * m));
    }
    public static VectorInt Reverse(VectorInt v)
    {
        return new VectorInt(-v.X, -v.Y, -v.Z);
    }

    public bool IsHasNotAngle()
    {
        return X != 0 && Y == 0 && Z == 0 || X == 0 && Y != 0 && Z == 0 || X == 0 && Y == 0 && Z != 0;
    }

    public static float ScalarMultiple(VectorInt v1, VectorInt v2)
    {
        return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
    }
    public static float CalculateDistance(VectorInt v1, VectorInt v2)
    {
        return (float)Math.Sqrt((v1.X - v2.X) * (v1.X - v2.X) +
            (v1.Y - v2.Y) * (v1.Y - v2.Y) +
            (v1.Z - v2.Z) * (v1.Z - v2.Z));
    }
    public static VectorInt operator +(VectorInt v1, VectorInt v2) // Vlad: Переопределения примитивных операторов для целей работы.
    {
        return new VectorInt(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
    }
    public static VectorInt operator -(VectorInt v)
    {
        return new VectorInt(-v.X, -v.Y, -v.Z);
    }
    public static VectorInt operator -(VectorInt v1, VectorInt v2)
    {
        return new VectorInt(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
    }


    //public static VectorInt operator *(float value, VectorInt v)
    //{
    //    return new VectorInt((int)(v.X * value), (int)(v.Y * value), (int)(v.Z * value));
    //}

    //public static VectorInt operator /(float value, VectorInt v)
    //{
    //    return new VectorInt((int)(v.X / value), (int)(v.Y / value), (int)(v.Z / value));
    //}

    public static bool operator ==(VectorInt v1, VectorInt v2)
    {
        return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
    }
    public static bool operator !=(VectorInt v1, VectorInt v2)
    {
        return v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z || v1.GetModule() != v2.GetModule();
    }

    public override bool Equals(object obj)
    {
        if (obj != null && obj is VectorInt otherVectorInt) return X == otherVectorInt.X && Y == otherVectorInt.Y && Z == otherVectorInt.Z;
        return false;
    }
    public override int GetHashCode() => X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
    public override string ToString()
    {
        return $"({X}; {Y}; {Z})";
    }
    public enum Dimension : int
    {
        X = 0,
        Y = 1,
        Z = 2,
    }
}
