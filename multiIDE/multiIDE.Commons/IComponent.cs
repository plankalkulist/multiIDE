namespace multiIDE
{
    public interface IComponent
    {
        #region Profile properties
        string DefaultName { get; }
        string Version { get; }
        string Author { get; }
        string Description { get; }
        #endregion

        #region Environment properties
        string CustomName { get; set; }
        int Id { get; set; }
        string Title { get; set; }
        object Tag { get; set; }
        #endregion
    }
}
