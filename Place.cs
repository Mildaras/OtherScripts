public class Place
{
    public string Name { get; set; }
    public double Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    public Place(string name, double id, double x, double y)
    {
        Name = name;
        Id = id;
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return string.Format("{0,-60} {1,10} {2} {3}", Name, Id, X, Y);
    }
}
