using System.Collections.Generic;

namespace DsmSuite.DsmViewer.Application.Interfaces
{
    /// <summary>
    /// An action the user can perform in the application.<para/>
    /// Actions execute on the data in the <see cref="Data"/> property. This data is injected
    /// into the constructor.<br/>
    /// When the model is saved, the <see cref="Data"/> property is saved with the action.<br/>
    /// When loading the model again, the action is re-created using a
    /// <c>constructor(IDsmModel m, IActionContext c, IReadOnlyDictionary&lt;string,string&gt; data)</c>.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// The unique type for this action.
        /// </summary>
        /// todo only used by ActionStore for impex. Implement locally there and remove here.
        ActionType Type { get; }

        /// <summary>
        /// String that identifies this action in menus and action lists (e.g. undo/redo).<para/>
        /// This must be the same for all instances of the action.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// String that describes the details of this particular action for informative purposes
        /// (e.g. in undo/redo).<para/>
        /// This usually returns a different text for different instances of the action, based
        /// on the contents of <see cref="Data"/>.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Perform this action using the data stored in <c>Data</c>.<br/>
        /// This usually returns null, except for actions that are expected by the application to
        /// return some useful object, like create actions.
        /// </summary>
        /// <returns>Some relevant object or null</returns>
        object Do();

        /// <summary>
        /// Undo the result of <c>Do()</c>.
        /// </summary>
        void Undo();

        /// <summary>
        /// Return true iff this action has all the data it needs to be done/undone.
        /// </summary>
        bool IsValid();

        /// <summary>
        /// A key->value map of the data this action needs to be performed, injected into the
        /// constructor.<para/>
        /// The implementing class can determine the contents and format of Data itself, but
        /// <c>ActionAttributes</c> and <c>ActionReadOnlyAttributes</c> provide convenience
        /// functions for this.
        /// </summary>
        IReadOnlyDictionary<string, string> Data { get; }
    }
}
