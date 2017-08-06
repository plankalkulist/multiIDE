namespace multiIDE
{
    public interface IWorkplaceComponent : IComponent
    {
        #region Environment properties
        IWorkplace ParentWorkplace { get; set; }
        #endregion
    }
}
