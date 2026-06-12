namespace GridSystem
{
    public interface IMaterial
    {
        public string MaterialName { get; set; }
        public MaterialCategory Category { get; set; }

        public IMaterial Assign(Tile tile, GridManager manager);

        public void Clear();
    }
}