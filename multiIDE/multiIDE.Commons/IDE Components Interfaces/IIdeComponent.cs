namespace multiIDE
{
    public interface IIdeComponent : IComponent
    {
        #region Environment properties
        IIDE ParentIDE { get; set; }
        #endregion
    }
}
