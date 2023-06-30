namespace YoloV8Net.Segment
{
    public class SegClass
    {
        public int Id { get; init; }
        public string? Name { get; init; }

        public SegClass()
        {
        }

        public SegClass(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
