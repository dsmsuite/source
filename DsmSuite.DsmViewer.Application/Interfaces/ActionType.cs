namespace DsmSuite.DsmViewer.Application.Interfaces
{
    /// <summary>
    /// The enumeration of all available user actions (i.e. <c>IAction</c> implementors).
    /// Every enumeration tag should be equal to the name of its implementing class.
    /// </summary>
    public enum ActionType
    {
        ElementChangeName,
        ElementChangeParent,
        ElementChangeType,
        ElementCreate,
        ElementDelete,
        ElementMoveUp,
        ElementMoveDown,
        ElementSort,
        ElementCopy,
        ElementCut,
        ElementPaste,
        
        RelationChangeType,
        RelationChangeWeight,
        RelationCreate,
        RelationDelete,

        Snapshot
    }
}
